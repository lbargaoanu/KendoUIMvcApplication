using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Metadata;
using System.Web.Http.Validation;
using AutoMapper;
using Infrastructure.Web.GridProfile;
using Newtonsoft.Json;
using StructureMap.Pipeline;
using StructureMap.Web;

namespace Infrastructure.Web
{
    public static class Utils
    {
        private static readonly MethodInfo Id = Utils.Getter<Entity, int>(e => e.Id);
        private static readonly MethodInfo Contains = Utils.Method<object>(o => Enumerable.Contains<int>(null, 0));

        public static void SetRowVersion<TEntity>(this TEntity destination, TEntity source, DbContext context) where TEntity : VersionedEntity
        {
            if(source.RowVersion != destination.RowVersion)
            {
                context.Entry(destination).Property(e => e.RowVersion).OriginalValue = source.RowVersion;
            }
        }

        public static TEntity Find<TEntity>(this ICollection<TEntity> entities, int id) where TEntity : Entity
        {
            return entities.SingleOrDefault(e => e.Id == id);
        }

        public static void Add<T>(this ICollection<T> collection, IEnumerable<T> newItems)
        {
            foreach(var item in newItems)
            {
                collection.Add(item);
            }
        }
        
        public static bool IsEntity(this Type type)
        {
            return typeof(Entity).IsAssignableFrom(type);
        }

        public static void Set<TEntity>(this ICollection<TEntity> children, params int[] ids) where TEntity : Entity, new()
        {
            children.Set(null, ids);
        }

        public static bool IsNavigationProperty(this PropertyInfo property)
        {
            return property.PropertyType.IsEntity() || property.PropertyType.IsEntityCollection();
        }

        public static bool IsEntityCollection(this Type type)
        {
            return type != typeof(string) && !type.IsArray && typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static Type GetNomenclatorCollectionElementType(this Type type)
        {
            if(!type.IsEntityCollection() || !type.IsConstructedGenericType || type.GetGenericTypeDefinition() != typeof(ICollection<>))
            {
                return null;
            }
            var elementType = type.GetGenericArguments()[0];
            return typeof(NomEntity).IsAssignableFrom(elementType) ? elementType : null;
        }

        public static IQueryable NonGenericWhere(this IQueryable source, Expression predicate)
        {
            return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Where", new[] { source.ElementType }, source.Expression, predicate));
        }

        public static IEnumerable<Entity> GetEntities(DbContext context, Type entityType, IEnumerable<int> ids)
        {
            var dbSet = context.Set(entityType);
            var parameter = Expression.Parameter(entityType, "item");
            var getId = Expression.Property(parameter, Id);
            var enumerableIds = Expression.Constant(ids);
            var predicate = Expression.Call(Contains, enumerableIds, getId);
            var where = Expression.Lambda(predicate, parameter);
            return (IEnumerable<Entity>)dbSet.NonGenericWhere(where);
        }

        public static void SetNomCollectionsUnchanged(object model, Type modelType, DbContext context, ChildCollectionsModelMetadataProvider metadataProvider)
        {
            if(!modelType.IsEntity())
            {
                return;
            }
            foreach(var metadata in metadataProvider.GetChildCollections(model, modelType))
            {
                var childrenColection = (IEnumerable)metadata.Model;
                foreach(var child in childrenColection)
                {
                    context.Entry(child).State = EntityState.Unchanged;
                }
            }
        }

        public static IMappingExpression IgnoreProperties(this IMappingExpression mappingExpression, Type destinationType, Func<PropertyInfo, bool> filter)
        {
            var destInfo = new AutoMapper.TypeInfo(destinationType);
            foreach(var destProperty in destInfo.GetPublicWriteAccessors().OfType<PropertyInfo>().Where(filter))
            {
                mappingExpression = mappingExpression.ForMember(destProperty.Name, opt => opt.Ignore());
            }
            return mappingExpression;
        }

        public static void Set<TEntity>(this ICollection<TEntity> children, DbContext context, params int[] ids) where TEntity : Entity, new()
        {
            TEntity[] existingChildren;
            if(ids.Length == 0)
            {
                existingChildren = children.Where(e => e.Exists).ToArray();
                foreach(var child in existingChildren)
                {
                    children.Remove(child);
                }
                ids = existingChildren.Select(e => e.Id).ToArray();
            }
            foreach(int childId in ids)
            {
                var child = (context == null) ? new TEntity{ Id = childId } : context.Set<TEntity>().Find(childId);
                children.Add(child);
            }
        }

        public static TEntity GetWithInclude<TEntity, TChild>(this DbSet<TEntity> dbSet, int id, Expression<Func<TEntity, TChild>> path) where TEntity : Entity
        {
            return dbSet.Where(e => e.Id == id).Include(path).Single();
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static string PropertyOrFieldName<TObject, TProperty>(this Expression<Func<TObject, TProperty>> e)
        {
            return e.PropertyOrField().Name;
        }

        public static MethodInfo Method<T>(this Expression<Action<T>> e)
        {
            return e.UntypedMethod();
        }

        public static MethodInfo Getter<TObject, TProperty>(this Expression<Func<TObject, TProperty>> e)
        {
            return ((PropertyInfo)e.UntypedPropertyOrField()).GetGetMethod();
        }

        public static MemberInfo PropertyOrField<TObject, TProperty>(this Expression<Func<TObject, TProperty>> e)
        {
            return e.UntypedPropertyOrField();
        }

        public static MemberInfo UntypedPropertyOrField(this LambdaExpression lambda)
        {
            var method = lambda.Body as MemberExpression;
            if(method != null)
            {
                return method.Member;
            }
            throw new ArgumentException("'" + lambda.Body + "' is not a property or field access.");
        }

        private static MethodInfo UntypedMethod(this LambdaExpression lambda)
        {
            var method = lambda.Body as MethodCallExpression;
            if(method != null)
            {
                return method.Method;
            }
            throw new ArgumentException("'" + lambda.Body + "' is not a method access.");
        }

        public static void Assert(this JsonReader reader, JsonToken token)
        {
            if(reader.TokenType != token)
            {
                throw new InvalidOperationException("Expected "+token);
            }
        }
    }

    public class ChildCollectionsModelMetadataProvider : ModelMetadataProvider
    {
        private readonly ModelMetadataProvider metadataProvider;

        public ChildCollectionsModelMetadataProvider()
        {
        }

        public ChildCollectionsModelMetadataProvider(ModelMetadataProvider metadataProvider)
        {
            this.metadataProvider = metadataProvider;
        }

        public override IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType)
        {
            return metadataProvider.GetMetadataForProperties(container, containerType).Where(m=>m.ModelType.GetNomenclatorCollectionElementType() == null);
        }

        public virtual IEnumerable<ModelMetadata> GetChildCollections(object container, Type containerType)
        {
            return metadataProvider.GetMetadataForProperties(container, containerType).Where(m => m.ModelType.GetNomenclatorCollectionElementType() != null);
        }

        public override ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName)
        {
            return metadataProvider.GetMetadataForProperty(modelAccessor, containerType, propertyName);
        }

        public override ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType)
        {
            return metadataProvider.GetMetadataForType(modelAccessor, modelType);
        }
    }

    public class ChildCollectionsBodyModelValidator : IBodyModelValidator
    {
        private readonly IBodyModelValidator validator;

        public ChildCollectionsBodyModelValidator(IBodyModelValidator validator)
        {
            this.validator = validator;
        }

        public bool Validate(object model, Type type, ModelMetadataProvider metadataProvider, HttpActionContext actionContext, string keyPrefix)
        {
            var context = ((IContextController)actionContext.ControllerContext.Controller).Context;
            Utils.SetNomCollectionsUnchanged(model, type, context, (ChildCollectionsModelMetadataProvider) metadataProvider);
            return validator.Validate(model, type, metadataProvider, actionContext, keyPrefix);
        }
    }

    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public ValidateModelAttribute(HttpConfiguration config)
        {
            config.Services.Replace(typeof(IBodyModelValidator), new ChildCollectionsBodyModelValidator(config.Services.GetBodyModelValidator()));
            config.Services.Replace(typeof(ModelMetadataProvider), new ChildCollectionsModelMetadataProvider(config.Services.GetModelMetadataProvider()));
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if(actionContext.ModelState.IsValid)
            {
                return;
            }
            actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, actionContext.ModelState);
        }
    }

    public class JQueryArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Assert(JsonToken.StartArray);
            var items = new List<byte>();
            int? item;
            while((item = reader.ReadAsInt32()) != null)
            {                
                items.Add(checked((byte) item.Value));
            }
            reader.Assert(JsonToken.EndArray);
            return items.ToArray();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach(var item in (byte[])value)
            {
                writer.WriteValue(item);
            }
            writer.WriteEndArray();
        }
    }

    public class ContextLifecyleObjectCache : LifecycleObjectCache
    {
        private List<DbContext> contexts = new List<DbContext>();

        public void SaveChanges()
        {
            foreach(var dbContext in contexts)
            {
                dbContext.SaveChanges();
            }
        }

        protected override object buildWithSession(Type pluginType, Instance instance, StructureMap.IBuildSession session)
        {
            var obj = base.buildWithSession(pluginType, instance, session);
            var context = obj as DbContext;
            if(context != null)
            {
                contexts.Add(context);
            }
            return obj;
        }
    }

    public class SaveChangesFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var request = actionExecutedContext.Request;
            var response = actionExecutedContext.Response;
            if(request.Method == HttpMethod.Get || response == null || !response.IsSuccessStatusCode)
            {
                return;
            }
            var contexts = (ContextLifecyleObjectCache) WebLifecycles.HttpContext.FindCache(null);
            contexts.SaveChanges();
        }
    }
}