using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Infrastructure.Web;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Kernel;
using Ploeh.AutoFixture.Xunit;
using StructureMap;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Infrastructure.Test
{
    public static class TestContextFactory<TContext> where TContext : BaseContext
    {
        private static UInt32SequenceGenerator sequence = new UInt32SequenceGenerator();
        private static readonly TContext context = CreateDatabase();
        private static bool seedExecuted;

        public static TContext New(DbConnection connection)
        {
            var context = (TContext)Activator.CreateInstance(typeof(TContext), connection);
            context.Database.Log = s =>
            {
                if(Trace.Listeners.OfType<DefaultTraceListener>().Count() == 0)
                {
                    Trace.Listeners.Add(new DefaultTraceListener());
                }
                Trace.WriteLine(s);
            };
            context.ModelCreating += (e, args) =>
            {
                args.ModelBuilder.Properties<byte[]>().Configure(c => c.HasColumnType("BLOB").HasDatabaseGeneratedOption(DatabaseGeneratedOption.None));
            };
            context.SavingChanges += (e, args) =>
            {
                List<object> modifiedEntities = null;
                foreach(var entry in args.Context.ChangeTracker.Entries().Where(entry => entry.Entity is VersionedEntity))
                {
                    if(entry.State == EntityState.Modified)
                    {
                        if(modifiedEntities == null)
                        {
                            modifiedEntities = new List<object>();
                        }
                        modifiedEntities.Add(entry.Entity);
                    }
                    else if(entry.State == EntityState.Added)
                    {
                        SetRowVersion(entry.Entity);
                    }
                }
                args.State = modifiedEntities;
            };
            context.SavedChanges += (e, args) =>
            {
                if(args.State != null)
                {
                    SetRowVersion((IEnumerable<object>)args.State);
                    args.Context.SaveChanges();
                }
            };
            return context;
        }

        public static StoreItemCollection GetSchema()
        {
            var objContext = ((IObjectContextAdapter)New()).ObjectContext;
            return (StoreItemCollection)objContext.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
        }

        public static TContext New()
        {
            return New(context.Database.Connection);
        }

        private static TContext CreateDatabase()
        {
            Database.SetInitializer<TContext>(null);
            var connection = new SQLiteConnection("FullUri=file::memory:;foreign keys=True");
            connection.Open();
            var context = New(connection);
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "ssdltosqlite3.sql");
            string script;
            try
            {
                script = File.ReadAllText(path);
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex);
                return context;
            }
            var result = context.Database.ExecuteSqlCommand(script);
            Assert.Equal(0, result);
            return context;
        }

        public static void Initialize(Action<TContext, IFixture> initializer)
        {
            if(seedExecuted)
            {
                return;
            }
            lock(context)
            {
                if(seedExecuted)
                {
                    return;
                }
                if(initializer != null)
                {
                    initializer(context, ContextAutoDataAttribute.CreateFixture(typeof(TContext)));
                }
                context.SaveChanges();
                seedExecuted = true;
            }
        }

        private static void SetRowVersion(IEnumerable<object> entities)
        {
            foreach(var entity in entities)
            {
                SetRowVersion(entity);
            }
        }

        private static void SetRowVersion(object entity)
        {
            ((VersionedEntity)entity).RowVersion = BitConverter.GetBytes(sequence.CreateAnonymous());
        }
    }

    public class RepeatTheoryAttribute : TheoryAttribute
    {
        readonly int _count;

        public RepeatTheoryAttribute(int count)
        {
            _count = count;
        }

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            return base.EnumerateTestCommands(method).Repeat(_count);
        }
    }

    public class RepeatFactAttribute : FactAttribute
    {
        readonly int _count;

        public RepeatFactAttribute(int count)
        {
            _count = count;
        }

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            return base.EnumerateTestCommands(method).Repeat(_count);
        }
    }

    public class DbResolver : IDbDependencyResolver
    {
        public static readonly string FactoryName = typeof(SQLiteFactory).AssemblyQualifiedName;
        public static readonly Type ProviderType = typeof(SQLiteProviderFactory).Assembly.GetType("System.Data.SQLite.EF6.SQLiteProviderServices");
        public static readonly string ProviderName = ProviderType.Assembly.GetName().Name;
        public static readonly DbProviderServices Provider = (DbProviderServices)ProviderType.GetField("Instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

        public object GetService(Type type, object key)
        {
            if(type == typeof(DbProviderServices) && key.ToString() == ProviderName)
            {
                return Provider;
            }
            return null;
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            return Enumerable.Empty<object>();
        }
    }

    public class ContextAutoDataAttribute : AutoDataAttribute
    {
        private static ConcurrentDictionary<Type, MethodInfo> customizeMethods = new ConcurrentDictionary<Type, MethodInfo>();

        public static IFixture CreateFixture(Type contextType)
        {
            return Customize(new Fixture(), contextType);
        }

        private static IFixture Customize(IFixture fixture, Type contextType)
        {
            var customizationType = typeof(ContextCustomization<>).MakeGenericType(contextType);
            var customization = (ICustomization)Activator.CreateInstance(customizationType);
            fixture = fixture.Customize(customization).Customize(new AutoMoqCustomization());
            fixture.Behaviors.Remove(fixture.Behaviors.OfType<ThrowingRecursionBehavior>().First());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        public MethodInfo GetCustomizeMethod(Type reflectedType)
        {
            var testType = reflectedType.BaseType;
            if(testType == null || !testType.IsGenericType)
            {
                return null;
            }
            return customizeMethods.GetOrAdd(reflectedType, type =>
            {
                var entityType = type.BaseType.GenericTypeArguments[1];
                return type.GetMethod("Customize", new[] { entityType });
            });
        }

        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
        {
            var contextType = GetContextType(methodUnderTest);
            Customize(Fixture, contextType);
            var data = base.GetData(methodUnderTest, parameterTypes);
            var reflectedType = methodUnderTest.ReflectedType;
            var customizeMethod = GetCustomizeMethod(reflectedType);
            if(customizeMethod != null)
            {
                var entityType = reflectedType.BaseType.GenericTypeArguments[1];
                var methodParameters = data.First();
                foreach(var parameter in methodParameters.Where(p => p != null && p.GetType() == entityType))
                {
                    customizeMethod.Invoke(null, new[] { parameter });
                }
            }
            return data;
        }

        private static Type GetContextType(MethodInfo methodUnderTest)
        {
            Type testType = methodUnderTest.DeclaringType;
            while(!testType.Name.StartsWith("ControllerTests`"))
            {
                testType = testType.BaseType;
            }
            return testType.GenericTypeArguments[1];
        }
    }

    public class ContextCustomization<TContext> : ICustomization, ISpecimenBuilder where TContext : BaseContext
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<TContext>(c => c.FromFactory(TestContextFactory<TContext>.New).OmitAutoProperties());
            fixture.RepeatCount = Extensions.CollectionCount;
            fixture.Customizations.Add(this);
        }

        public object Create(object request, ISpecimenContext context)
        {
            var pi = request as PropertyInfo;
            if(pi != null)
            {
                var type = pi.PropertyType;
                if(type.IsEntity())
                {
                    return null;
                }
                else if(type.IsEntityCollection())
                {
                    var entityType = type.GenericTypeArguments[0];
                    return Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(entityType));
                }
                else if(type.IsInt())
                {
                    if(pi.Name == "Id")
                    {
                        return 0;
                    }
                    else if(pi.Name.EndsWith("ID") && pi.Name.Length > 2)
                    {
                        return 1;
                    }
                }
            }
            return new NoSpecimen(request);
        }
    }
}