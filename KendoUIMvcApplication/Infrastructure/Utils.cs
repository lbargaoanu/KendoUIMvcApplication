using System;
using System.Data.Entity;
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