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
using System.Web.Http;
using System.Web.Http.Results;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Infrastructure.Web;
using Kendo.Mvc.UI;
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
    public static class Extensions
    {
        private const int PageSize = 3;
        private static string[] IntegerTypes = new[] { "int", "bigint", "smallint", "tinyint", "bit" };
        private static string[] RealTypes = new[] { "double", "decimal", "float", "real" };

        public static string GetType(EdmProperty property)
        {
            TypeUsage typeUsage = property.TypeUsage;
            if(typeUsage != null)
            {
                EdmType edmType = typeUsage.EdmType;
                if(edmType != null)
                {
                    if(edmType.BaseType != null && edmType.BaseType.Name == "String")
                    {
                        return "TEXT";
                    }
                    var typeName = edmType.Name.ToLower();
                    if(IntegerTypes.Contains(typeName))
                    {
                        return "INTEGER";
                    }
                    else if(RealTypes.Contains(typeName))
                    {
                        return "REAL";
                    }
                    else if(typeName == "guid")
                    {
                        return "UNIQUEIDENTIFIER";
                    }
                    else if(typeName == "datetime" || typeName == "datetimeoffset")
                    {
                        return "DATETIME";
                    }
                }
            }
            return "BLOB";
        }

        public static void DeleteAndSave<TEntity>(this DbContext context, int id) where TEntity : VersionedEntity
        {
            var entities = context.Set<TEntity>();
            entities.Remove(entities.Find(id));
            context.SaveChanges();
        }

        public static TEntity AddAndSave<TEntity>(this DbContext context, params TEntity[] entities) where TEntity : VersionedEntity
        {
            var dbSet = context.Set<TEntity>();
            foreach(var entity in entities)
            {
                dbSet.Add(entity);
            }
            context.SaveChanges();
            return entities.First();
        }

        public static void DetachAll(this DbContext context)
        {
            foreach(var entry in context.ChangeTracker.Entries())
            {
                entry.State = EntityState.Detached;
            }
        }

        public static bool IsInt(this Type type)
        {
            return type == typeof(int?) || type == typeof(int);
        }

        public static void HandleAndSave<TCommand, TContext>(this CommandHandler<TContext, TCommand> handler, TCommand command) where TCommand : ICommand where TContext : BaseContext
        {
            if(handler.Context == null)
            {
                handler.Context = TestContextFactory<TContext>.New();
            }
            handler.Handle(command);
            handler.Context.SaveChanges();
        }

        public static IHttpActionResult PutAndSave<TEntity, TContext>(this CrudController<TContext, TEntity> controller, TEntity entity) where TEntity : VersionedEntity where TContext : BaseContext
        {
            return controller.Action(c => c.Put(entity.Id, entity));
        }

        public static IHttpActionResult PutAndSave<TEntity, TContext>(this CrudController<TContext, TEntity> controller, int id, TEntity entity) where TEntity : VersionedEntity where TContext : BaseContext
        {
            return controller.Action(c => c.Put(id, entity));
        }

        public static IHttpActionResult DeleteAndSave<TEntity, TContext>(this CrudController<TContext, TEntity> controller, int id) where TEntity : VersionedEntity where TContext : BaseContext
        {
            return controller.Action(c => c.Delete(id));
        }

        public static IHttpActionResult PostAndSave<TEntity, TContext>(this CrudController<TContext, TEntity> controller, TEntity entity) where TEntity : VersionedEntity where TContext : BaseContext
        {
            return controller.Action(c => c.Post(entity));
        }

        public static DataSourceResult HandleGetAll<TEntity, TContext>(this CrudController<TContext, TEntity> controller) where TEntity : VersionedEntity where TContext : BaseContext
        {
            return controller.Action(c => c.GetAll(new DataSourceRequest{ PageSize = PageSize, Page = 2 }));
        }

        public static IHttpActionResult HandleGetById<TEntity, TContext>(this CrudController<TContext, TEntity> controller, int id) where TEntity : VersionedEntity where TContext : BaseContext
        {
            return controller.Action(c => c.Get(id));
        }

        public static TResult Action<TContext, TEntity, TResult>(this CrudController<TContext, TEntity> controller, Func<CrudController<TContext, TEntity>, TResult> action)
            where TEntity : VersionedEntity
            where TContext : BaseContext
        {
            using(var scope = new StructureMapDependencyScope(ObjectFactory.Container.GetNestedContainer()))
            {
                if(controller.Context == null)
                {
                    controller.Context = (TContext) scope.GetService(typeof(TContext));
                    controller.Mediator = Mediator.Create(scope);
                }
                var result = action(controller);
                controller.Context.SaveChanges();
                return result;
            }
        }

        public static void AssertIs<TEntity>(this DataSourceResult result, int length)
        {
            Assert.Equal(null, result.Errors);
            var data = (List<TEntity>) result.Data;
            data.Count(e => e is TEntity).Should().Be(PageSize, "Sa intoarca PageSize entities.");
            result.Total.Should().BeGreaterOrEqualTo(length, "Se poate sa avem date din alte teste.");
        }

        public static BadRequestResult AssertIsBadRequest(this IHttpActionResult result)
        {
            return Assert.IsType<BadRequestResult>(result);
        }

        public static InvalidModelStateResult AssertIsInvalid(this IHttpActionResult result)
        {
            return Assert.IsType<InvalidModelStateResult>(result);
        }

        public static NotFoundResult AssertIsNotFound(this IHttpActionResult result)
        {
            return Assert.IsType<NotFoundResult>(result);
        }

        public static OkResult AssertIsOk(this IHttpActionResult result)
        {
            return Assert.IsType<OkResult>(result);
        }

        public static OkNegotiatedContentResult<Wrapper> AssertIsOk<TEntity>(this IHttpActionResult result, TEntity entity) where TEntity : VersionedEntity
        {
            var content = Assert.IsAssignableFrom<OkNegotiatedContentResult<Wrapper>>(result);
            Assert.Equal(1, content.Content.Total);
            content.Content.ShouldContain(entity);
            return content;
        }

        public static CreatedAtRouteNegotiatedContentResult<Wrapper> AssertIsCreatedAtRoute<TEntity>(this IHttpActionResult result, TEntity entity) where TEntity : VersionedEntity
        {
            var content = Assert.IsAssignableFrom<CreatedAtRouteNegotiatedContentResult<Wrapper>>(result);
            Assert.Equal(content.RouteName, "DefaultApi");
            Assert.Equal(content.RouteValues["id"], entity.Id);
            Assert.Equal(1, content.Content.Total);
            content.Content.ShouldContain(entity);
            return content;
        }

        public static void ShouldContain<TEntity>(this Wrapper wrapper, TEntity entity) where TEntity : VersionedEntity
        {
            wrapper.Data[0].ShouldBeEquivalentTo(entity);
        }

        public static void ShouldAllBeEquivalentTo<TEntity>(this IEnumerable<TEntity> subject, IEnumerable<TEntity> expectation, string reason = "", params object[] reasonArgs) where TEntity : Entity
        {
            subject.ShouldAllBeEquivalentTo(expectation, c=>c.ExcludeNavigationProperties<TEntity>(), reason, reasonArgs);
        }

        public static void ShouldAllBeQuasiEquivalentTo<TEntity>(this IEnumerable<TEntity> subject, IEnumerable<TEntity> expectation, string reason = "", params object[] reasonArgs) where TEntity : Entity
        {
            subject.ShouldAllBeEquivalentTo(expectation, ExcludeInfrastructure<TEntity>(), reason, reasonArgs);
        }

        public static void ShouldBeQuasiEquivalentTo<TEntity>(this TEntity subject, TEntity expectation, string reason = "", params object[] reasonArgs) where TEntity : Entity
        {
            AssertionExtensions.ShouldBeEquivalentTo(subject, expectation, ExcludeInfrastructure<TEntity>(), reason, reasonArgs);
        }

        private static Func<EquivalencyAssertionOptions<TEntity>, EquivalencyAssertionOptions<TEntity>> ExcludeInfrastructure<TEntity>() where TEntity : Entity
        {
            return c => c.Excluding(e => e.Id).Excluding(s=>s.PropertyPath=="RowVersion").ExcludeNavigationProperties();
        }

        public static void ShouldBeEquivalentTo<TEntity>(this TEntity subject, TEntity expectation, string reason = "", params object[] reasonArgs) where TEntity : Entity
        {
            AssertionExtensions.ShouldBeEquivalentTo(subject, expectation, e=>e.ExcludeNavigationProperties(), reason, reasonArgs);
        }

        public static void ShouldHaveTheSameIdsAs<TEntity>(this IEnumerable<TEntity> subject, IEnumerable<TEntity> expectation) where TEntity : Entity
        {
            Assert.Equal(expectation.OrderBy(t=>t.Id).Select(t => t.Id), subject.OrderBy(t=>t.Id).Select(t => t.Id));
        }

        public static EquivalencyAssertionOptions<TSubject> ExcludeNavigationProperties<TSubject>(this EquivalencyAssertionOptions<TSubject> options)
        {
            return options.Excluding(s => s.PropertyInfo.IsNavigationProperty());
        }

        public static IEnumerable<ITestCommand> Repeat(this IEnumerable<ITestCommand> commands, int count)
        {
            var result = commands.SelectMany(tc => Enumerable.Repeat(tc, count));
            return (count == 1) ? result : result.RepeatCore(count);
        }

        public static IEnumerable<ITestCommand> RepeatCore(this IEnumerable<ITestCommand> commands, int count)
        {
            foreach(var command in commands)
            {
                var theoryCommand = command as TheoryCommand;
                if(theoryCommand != null && theoryCommand.Parameters != null)
                {
                    foreach(var context in theoryCommand.Parameters.OfType<DbContext>())
                    {
                        context.DetachAll();
                    }
                }
                yield return command;
            }
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

    public static class TestContextFactory<TContext> where TContext : BaseContext
    {
        private static UInt32SequenceGenerator sequence = new UInt32SequenceGenerator();
        private static readonly TContext context = CreateDatabase();

        public static TContext New(DbConnection connection)
        {
            var context = (TContext) Activator.CreateInstance(typeof(TContext), connection);
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
                var modifiedEntities = new List<object>();
                foreach(var entry in args.Context.ChangeTracker.Entries().Where(entry => entry.Entity is VersionedEntity))
                {
                    if(entry.State == EntityState.Modified)
                    {
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
                if(SetRowVersion((IEnumerable<object>)args.State))
                {
                    args.Context.SaveChanges();
                }
            };
            return context;
        }

        public static StoreItemCollection GetSchema()
        {
            var objContext = ((IObjectContextAdapter)New()).ObjectContext;
            return (StoreItemCollection) objContext.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
        }

        public static TContext New()
        {
            return New(context.Database.Connection);
        }

        private static TContext CreateDatabase()
        {
            Database.SetInitializer<TContext>(null);
            var connection = new SQLiteConnection("FullUri=file::memory:?cache=shared;");
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
            result = context.Database.ExecuteSqlCommand("PRAGMA foreign_keys = ON;");
            Assert.Equal(0, result);
            return context;
        }

        public static void Initialize(Action<TContext, IFixture> seedDatabase = null)
        {
            ObjectFactory.Configure(c => c.For<TContext>().Transient().Use(_ => New()));
            if(seedDatabase != null)
            {
                seedDatabase(context, ContextAutoDataAttribute.CreateFixture(typeof(TContext)));
            }
            context.SaveChanges();
        }

        private static bool SetRowVersion(IEnumerable<object> entities)
        {
            bool changes = false;
            foreach(var entity in entities)
            {
                SetRowVersion(entity);
                changes = true;
            }
            return changes;
        }

        private static void SetRowVersion(object entity)
        {
            ((VersionedEntity)entity).RowVersion = BitConverter.GetBytes(sequence.CreateAnonymous());
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
                return type.GetMethod("Customize", new[]{entityType});
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
        public const int CollectionCount = 8;

        public void Customize(IFixture fixture)
        {
            fixture.Customize<TContext>(c => c.FromFactory(TestContextFactory<TContext>.New).OmitAutoProperties());
            fixture.RepeatCount = CollectionCount;
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