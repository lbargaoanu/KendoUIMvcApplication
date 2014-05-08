using System;
using System.Collections;
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
using System.Web.Http.Dependencies;
using System.Web.Http.Results;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Kendo.Mvc.UI;
using KendoUIMvcApplication;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Kernel;
using Ploeh.AutoFixture.Xunit;
using StructureMap;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Test.Controllers.Integration
{
    public static class Extensions
    {
        private const int PageSize = 3;
        private static readonly ProductServiceContext dbContext = ContextHelper.BuildContext();
        private static readonly StructureMapResolver resolver = CreateResolver();

        private static StructureMapResolver CreateResolver()
        {
            KendoUIMvcApplication.StructureMap.Register();
            ObjectFactory.Configure(c => c.For<ProductServiceContext>().Use(()=>new TestProductServiceContext()));
            return new StructureMapResolver(ObjectFactory.Container);
        }

        public static bool IsInt(this Type type)
        {
            return type == typeof(int?) || type == typeof(int);
        }

        public static void HandleAndSave<TCommand>(this CommandHandler<ProductServiceContext, TCommand> handler, TCommand command) where TCommand : ICommand
        {
            if(handler.Context == null)
            {
                handler.Context = new TestProductServiceContext();
            }
            handler.Handle(command);
            handler.Context.SaveChanges();
        }

        public static IHttpActionResult PutAndSave<TEntity>(this NorthwindController<TEntity> controller, TEntity entity) where TEntity : Entity
        {
            return controller.Action(c => c.Put(entity.Id, entity));
        }

        public static IHttpActionResult PutAndSave<TEntity>(this NorthwindController<TEntity> controller, int id, TEntity entity) where TEntity : Entity
        {
            return controller.Action(c => c.Put(id, entity));
        }

        public static IHttpActionResult DeleteAndSave<TEntity>(this NorthwindController<TEntity> controller, int id) where TEntity : Entity
        {
            return controller.Action(c => c.Delete(id));
        }

        public static IHttpActionResult PostAndSave<TEntity>(this NorthwindController<TEntity> controller, TEntity entity) where TEntity : Entity
        {
            return controller.Action(c => c.Post(entity));
        }

        public static DataSourceResult HandleGetAll<TEntity>(this NorthwindController<TEntity> controller) where TEntity : Entity
        {
            return controller.Action(c => c.GetAll(new DataSourceRequest{ PageSize = PageSize, Page = 2 }));
        }

        public static IHttpActionResult HandleGetById<TEntity>(this NorthwindController<TEntity> controller, int id) where TEntity : Entity
        {
            return controller.Action(c => c.Get(id));
        }

        public static TResult Action<TController, TResult>(this TController controller, Func<TController, TResult> action) where TController : BaseController
        {
            using(var scope = resolver.BeginScope())
            {
                if(controller.Context == null)
                {
                    controller.Context = (TestProductServiceContext) scope.GetService(typeof(ProductServiceContext));
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

        public static OkNegotiatedContentResult<Wrapper> AssertIsOk<TEntity>(this IHttpActionResult result, TEntity entity) where TEntity : Entity
        {
            var content = Assert.IsAssignableFrom<OkNegotiatedContentResult<Wrapper>>(result);
            Assert.Equal(1, content.Content.Total);
            content.Content.ShouldContain(entity);
            return content;
        }

        public static CreatedAtRouteNegotiatedContentResult<Wrapper> AssertIsCreatedAtRoute<TEntity>(this IHttpActionResult result, TEntity entity) where TEntity : Entity
        {
            var content = Assert.IsAssignableFrom<CreatedAtRouteNegotiatedContentResult<Wrapper>>(result);
            Assert.Equal(content.RouteName, "DefaultApi");
            Assert.Equal(content.RouteValues["id"], entity.Id);
            Assert.Equal(1, content.Content.Total);
            content.Content.ShouldContain(entity);
            return content;
        }

        public static void ShouldContain<TEntity>(this Wrapper wrapper, TEntity entity) where TEntity : Entity
        {
            ((TEntity)wrapper.Data[0]).ShouldBeEquivalentTo(entity);
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
            return c => c.Excluding(e => e.Id).Excluding(e => e.RowVersion).ExcludeNavigationProperties();
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

    public class TestProductServiceContext : ProductServiceContext
    {
        private static UInt32SequenceGenerator sequence = new UInt32SequenceGenerator();

        public TestProductServiceContext(DbConnection connection) : base(connection)
        {
            Init();
        }

        public TestProductServiceContext()
        {
            Init();
        }

        private void Init()
        {
            Database.SetInitializer<TestProductServiceContext>(null);
            Database.Log = s =>
            {
                if(Trace.Listeners.OfType<DefaultTraceListener>().Count() == 0)
                {
                    Trace.Listeners.Add(new DefaultTraceListener());
                }
                Trace.WriteLine(s);
            };
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Properties<byte[]>().Configure(c => c.HasColumnType("BLOB").HasDatabaseGeneratedOption(DatabaseGeneratedOption.None));
        }

        public override int SaveChanges()
        {
            var modifiedEntities = new List<object>();
            foreach(var entry in ChangeTracker.Entries())
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

            var result = base.SaveChanges();

            if(SetRowVersion(modifiedEntities))
            {
                base.SaveChanges();
            }
            return result;
        }

        private bool SetRowVersion(IEnumerable<object> entities)
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
            ((Entity)entity).RowVersion = BitConverter.GetBytes(sequence.CreateAnonymous());
        }
    }

    public static partial class ContextHelper
    {
        private static string[] IntegerTypes = new[] { "int", "bigint", "smallint", "tinyint", "bit" };
        private static string[] RealTypes = new[] { "double", "decimal", "float", "real" };

        public static void DeleteAndSave<TEntity>(this DbContext context, int id) where TEntity : Entity
        {
            var entities = context.Set<TEntity>();
            entities.Remove(entities.Find(id));
            context.SaveChanges();
        }

        public static TEntity AddAndSave<TEntity>(this DbContext context, params TEntity[] entities) where TEntity : Entity
        {
            var dbSet = context.Set<TEntity>();
            foreach(var entity in entities)
            {
                dbSet.Add(entity);
            }
            context.SaveChanges();
            return entities.First();
        }

        public static ProductServiceContext BuildContext(bool createDatabase = true)
        {
            var connection = new SQLiteConnection("FullUri=file::memory:?cache=shared;");
            connection.Open();
            var context = new TestProductServiceContext(connection);
            if(!createDatabase)
            {
                return context;
            }
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "ssdltosqlite3.sql");
            var script = File.ReadAllText(path);
            var result = context.Database.ExecuteSqlCommand(script);
            Assert.Equal(0, result);
            result = context.Database.ExecuteSqlCommand("PRAGMA foreign_keys = ON;");
            Assert.Equal(0, result);

            var fixture = CreateFixture();
            SeedDatabase(context, fixture);

            context.SaveChanges();

            return context;
        }

        public static void DetachAll(this DbContext context)
        {
            foreach(var entry in context.ChangeTracker.Entries())
            {
                entry.State = EntityState.Detached;
            }
        }

        public static ItemCollection GetSchema()
        {
            var dbContext = ContextHelper.BuildContext(createDatabase: false);
            var objContext = ((IObjectContextAdapter)dbContext).ObjectContext;
            return objContext.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
        }

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

        public static IFixture CreateFixture()
        {
            var fixture = new Fixture().Customize(new MyCustomization()).Customize(new AutoMoqCustomization());
            fixture.Behaviors.Remove(fixture.Behaviors.OfType<ThrowingRecursionBehavior>().First());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }

    public class MyAutoDataAttribute : AutoDataAttribute
    {
        private static ConcurrentDictionary<Type, MethodInfo> customizeMethods = new ConcurrentDictionary<Type, MethodInfo>();

        public MyAutoDataAttribute() : base(ContextHelper.CreateFixture())
        {
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
    }

    public class MyCustomization : ICustomization, ISpecimenBuilder
    {
        public const int CollectionCount = 8;

        public void Customize(IFixture fixture)
        {
            fixture.Customize<ProductServiceContext>(c => c.FromFactory(() => new TestProductServiceContext()).OmitAutoProperties());
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