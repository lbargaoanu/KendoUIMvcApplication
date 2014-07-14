using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using AutoMapper;
using Newtonsoft.Json;

namespace Infrastructure.Web
{
    public static class Utils
    {
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
    }

    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if(actionContext.ModelState.IsValid == false)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, actionContext.ModelState);
            }
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
            return reader.ReadAsBytes();
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
            var dependencyScope = (StructureMapDependencyScope) actionExecutedContext.Request.GetDependencyScope();
            var context = dependencyScope.Container.Model.GetAllPossible<DbContext>().Single();
            context.SaveChanges();
        }
    }
}