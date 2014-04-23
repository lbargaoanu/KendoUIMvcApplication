using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json;

namespace KendoUIMvcApplication
{
    public static class Utils
    {
        public static void Add<T>(this ICollection<T> collection, IEnumerable<T> newItems)
        {
            foreach(var item in newItems)
            {
                collection.Add(item);
            }
        }

        public static void Set<TEntity>(this ICollection<TEntity> children, params int[] ids) where TEntity : Entity, new()
        {
            children.Set(null, ids);
        }

        public static void Set<TEntity>(this ICollection<TEntity> children, DbContext context, params int[] ids) where TEntity : Entity, new()
        {
            TEntity[] existingChildren;
            if(ids.Length == 0)
            {
                existingChildren = children.Where(e => e.Id > 0).ToArray();
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
            var context = (DbContext)actionExecutedContext.Request.GetDependencyScope().GetService(typeof(DbContext));
            context.SaveChanges();
        }
    }
}