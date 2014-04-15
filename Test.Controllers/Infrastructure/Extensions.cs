using System;
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
using AutoMapper;
using FluentAssertions;
using KendoUIMvcApplication;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Kernel;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Test.Controllers.Integration
{
    public static class Extensions
    {
        private static readonly ProductServiceContext context = ContextHelper.BuildContext();

        public static void Initialize()
        {
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

        public static IHttpActionResult PutAndSave<TEntity>(this CrudController<TEntity> controller, TEntity entity) where TEntity : Entity
        {
            return controller.Action(c => c.Put(entity.Id, entity));
        }

        public static IHttpActionResult PutAndSave<TEntity>(this CrudController<TEntity> controller, int id, TEntity entity) where TEntity : Entity
        {
            return controller.Action(c => c.Put(id, entity));
        }

        public static IHttpActionResult DeleteAndSave<TEntity>(this CrudController<TEntity> controller, int id) where TEntity : Entity
        {
            return controller.Action(c => c.Delete(id));
        }

        public static IHttpActionResult PostAndSave<TEntity>(this CrudController<TEntity> controller, TEntity entity) where TEntity : Entity
        {
            return controller.Action(c => c.Post(entity));
        }

        public static IQueryable<TEntity> HandleGetAll<TEntity>(this CrudController<TEntity> controller) where TEntity : Entity
        {
            return controller.Action(c => c.GetAll());
        }

        public static IHttpActionResult HandleGetById<TEntity>(this CrudController<TEntity> controller, int id) where TEntity : Entity
        {
            return controller.Action(c => c.Get(id));
        }

        public static TResult Action<TController, TResult>(this TController controller, Func<TController, TResult> action) where TController : BaseController
        {
            if(controller.Context == null)
            {
                controller.Context = new TestProductServiceContext();
            }
            var result = action(controller);
            controller.Context.SaveChanges();
            return result;
        }

        public static BadRequestResult AssertIsBadRequest(this  IHttpActionResult result)
        {
            return Assert.IsType<BadRequestResult>(result);
        }

        public static InvalidModelStateResult AssertIsInvalid(this  IHttpActionResult result)
        {
            return Assert.IsType<InvalidModelStateResult>(result);
        }

        public static NotFoundResult AssertIsNotFound(this  IHttpActionResult result)
        {
            return Assert.IsType<NotFoundResult>(result);
        }

        public static OkNegotiatedContentResult<Wrapper> AssertIsOk<TEntity>(this  IHttpActionResult result, TEntity entity) where TEntity : Entity
        {
            var content = Assert.IsAssignableFrom<OkNegotiatedContentResult<Wrapper>>(result);
            Assert.Equal(1, content.Content.Total);
            entity.ShouldBeEquivalentTo((TEntity)content.Content.Data[0]);
            return content;
        }

        public static CreatedAtRouteNegotiatedContentResult<Wrapper> AssertIsCreatedAtRoute<TEntity>(this  IHttpActionResult result, TEntity entity) where TEntity : Entity
        {
            var content = Assert.IsAssignableFrom<CreatedAtRouteNegotiatedContentResult<Wrapper>>(result);
            Assert.Equal(content.RouteName, "DefaultApi");
            Assert.Equal(content.RouteValues["id"], entity.Id);
            Assert.Equal(1, content.Content.Total);
            entity.ShouldBeEquivalentTo((TEntity)content.Content.Data[0]);
            return content;
        }

        public static void ShouldBeQuasiEquivalentTo<TEntity>(this TEntity subject, TEntity expectation, string reason = "", params object[] reasonArgs) where TEntity : Entity
        {
            AssertionExtensions.ShouldBeEquivalentTo(subject, expectation, c=>c.Excluding(e=>e.Id).Excluding(e=>e.RowVersion), reason, reasonArgs);
        }

        public static void ShouldBeEquivalentTo<TEntity>(this TEntity subject, TEntity expectation, string reason = "", params object[] reasonArgs) where TEntity : Entity
        {
            AssertionExtensions.ShouldBeEquivalentTo(subject, expectation, reason, reasonArgs);
        }

        public static IEnumerable<ITestCommand> Repeat(this IEnumerable<ITestCommand> commands, int count)
        {
            var result = commands.SelectMany(tc => Enumerable.Repeat(tc, count));
            foreach(var command in result)
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
        private static readonly Type ProviderType = typeof(SQLiteProviderFactory).Assembly.GetType("System.Data.SQLite.EF6.SQLiteProviderServices");
        public static string Name = ProviderType.Assembly.GetName().Name;
        public static readonly DbProviderServices Provider = (DbProviderServices)ProviderType.GetField("Instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

        public object GetService(Type type, object key)
        {
            if(type == typeof(DbProviderServices) && key.ToString() == Name)
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
            modelBuilder.Types().Configure(c =>
            {
                if(Mapper.FindTypeMapFor(c.ClrType, c.ClrType) == null)
                {
                    Mapper.CreateMap(c.ClrType, c.ClrType);//.ForMember("Id", p => p.Ignore()).ForMember("RowVersion", p => p.Ignore());
                }
            });
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

    public static class ContextHelper
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
            TEntity entity;
            if(entities.Length == 1)
            {
                entity = dbSet.Add(entities[0]);
            }
            else
            {
                entity = dbSet.AddRange(entities).First();
            }
            context.SaveChanges();
            return entity;
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
                    var typeName = edmType.Name;
                    if(IntegerTypes.Contains(typeName))
                    {
                        return "INTEGER";
                    }
                    else if(RealTypes.Contains(typeName))
                    {
                        return "REAL";
                    }
                    else if(typeName == "Guid")
                    {
                        return "UNIQUEIDENTIFIER";
                    }
                    else if(typeName == "DateTime" || typeName == "DateTimeOffset")
                    {
                        return "DATETIME";
                    }
                }
            }
            return "BLOB";
        }
    }

    public class MyAutoDataAttribute : AutoDataAttribute
    {
        public MyAutoDataAttribute()
            : base(new Fixture().Customize(new MyCustomization()).Customize(new AutoMoqCustomization()))
        {
            Extensions.Initialize();
        }
    }

    public class MyCustomization : ICustomization, ISpecimenBuilder
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<ProductServiceContext>(c => c.FromFactory(()=>new TestProductServiceContext()).OmitAutoProperties());
            fixture.RepeatCount = 8;
            fixture.Customizations.Add(this);
        }

        public object Create(object request, ISpecimenContext context)
        {
            var pi = request as PropertyInfo;
            if(pi == null)
            {
                return new NoSpecimen(request);
            }
            if(pi.PropertyType != typeof(int) || pi.Name != "Id")
            {
                return new NoSpecimen(request);
            }
            return 0;
        }
    }
}