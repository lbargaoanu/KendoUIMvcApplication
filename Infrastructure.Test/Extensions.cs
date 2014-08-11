using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Results;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Infrastructure.Web;
using Kendo.Mvc.UI;
using Moq;
using Moq.Language.Flow;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using StructureMap;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Infrastructure.Test
{
    public static class Extensions
    {
        private const int PageSize = 3;
        private const int PageCount = 2;
        private static string[] IntegerTypes = new[] { "int", "bigint", "smallint", "tinyint", "bit" };
        private static string[] RealTypes = new[] { "double", "decimal", "float", "real" };

        public static ISetupGetter<TMocked, TProperty> SetupGet<TMocked, TProperty>(this TMocked obj, Expression<Func<TMocked, TProperty>> expression) where TMocked : class
        {
            return Mock.Get(obj).SetupGet(expression);
        }

        public static ISetup<TMocked, TResult> Setup<TMocked, TResult>(this TMocked obj, Expression<Func<TMocked, TResult>> expression) where TMocked : class
        {
            return Mock.Get(obj).Setup(expression);
        }

        public static void Verify<TMocked>(this TMocked obj, Expression<Action<TMocked>> expression) where TMocked : class
        {
            obj.Verify(expression, Times.Once());
        }

        public static void Verify<TMocked>(this TMocked obj, Expression<Action<TMocked>> expression, Times times) where TMocked : class
        {
            Mock.Get(obj).Verify(expression, times);
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

        public static IHttpActionResult PutAndSave<TEntity, TContext>(this CrudController<TContext, TEntity> controller, TEntity entity, Dictionary<Type, object> injected = null)
            where TEntity : VersionedEntity
            where TContext : BaseContext
        {
            return controller.Action(c => c.Put(entity.Id, entity), injected);
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
            return controller.Action(c => c.GetAll(new DataSourceRequest{ PageSize = PageSize, Page = PageCount }));
        }

        public static IHttpActionResult HandleGetById<TEntity, TContext>(this CrudController<TContext, TEntity> controller, int id) where TEntity : VersionedEntity where TContext : BaseContext
        {
            return controller.Action(c => c.Get(id));
        }

        public static TResult Action<TContext, TEntity, TResult>(this CrudController<TContext, TEntity> controller, Func<CrudController<TContext, TEntity>, TResult> action, Dictionary<Type, object> injected = null)
            where TEntity : VersionedEntity
            where TContext : BaseContext
        {
            using(var scope = new AutofixtureDependencyScope<TContext>(injected))
            {
                if(controller.Context == null)
                {
                    controller.Context = (TContext)scope.GetService(typeof(TContext));
                    controller.Mediator = new Mediator(scope);
                }
                var result = action(controller);
                controller.Context.SaveChanges();
                return result;
            }
        }

        public static void AssertIs<TEntity>(this DataSourceResult result, TEntity[] entities, Func<TEntity, bool> where)
        {
             if(where == null)
            {
                where = e => true;
            }
            var length = entities.Count(where);
            Assert.Equal(null, result.Errors);
            var data = (List<TEntity>) result.Data;
            var expectedCount = (length >= PageCount * PageSize) ? PageSize : length - PageSize;
            data.Count(e => e is TEntity).Should().BeGreaterOrEqualTo(expectedCount, "Sa intoarca cel putin PageSize entities (se poate sa avem date din alte teste)");
            result.Total.Should().BeGreaterOrEqualTo(length, "Se poate sa avem date din alte teste");
            Assert.True(data.All(where), "Toate trebuie sa indeplineasca conditia de filtru");
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
            ((Entity)wrapper.Data[0]).ShouldBeEquivalentTo(entity);
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

        public static object Create(this IFixture fixture, Type type)
        {
            var context = new SpecimenContext(fixture);
            return context.Resolve(type);
        }
    }

    public class AutofixtureDependencyScope<TContext> : IDependencyScope
    {
        private List<IDisposable> disposables = new List<IDisposable>();
        private readonly IFixture fixture;
        private readonly Dictionary<Type, object> injected;

        public AutofixtureDependencyScope(Dictionary<Type, object> injected)
        {
            this.injected = injected;
            fixture = ContextAutoDataAttribute.CreateFixture(typeof(TContext));
            fixture.Freeze<TContext>();
        }

        public object GetService(Type serviceType)
        {
            object injectedValue;
            if(injected != null && injected.TryGetValue(serviceType, out injectedValue))
            {
                return injectedValue;
            }
            var realType = ObjectFactory.Container.Model.DefaultTypeFor(serviceType);
            var service = fixture.Create(realType);
            var disposable = service as IDisposable;
            if(disposable != null)
            {
                disposables.Add(disposable);
            }
            return service;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            foreach(var disposable in disposables)
            {
                disposable.Dispose();
            }
        }
    }
}