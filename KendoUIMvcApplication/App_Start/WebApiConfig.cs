using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.ModelBinding;
using Infrastructure.Web;
using StructureMap;

namespace KendoUIMvcApplication
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.DependencyResolver = new StructureMapResolver(ObjectFactory.Container);
            ObjectFactory.Configure(c => c.For<IDependencyResolver>().Use(config.DependencyResolver));

            config.Services.Insert(typeof(ModelBinderProvider), 0, // Insert at front to ensure other catch-all binders don’t claim it first
                                                new DataRequestModelBinderProvider());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new JQueryArrayConverter());
            config.Filters.Add(new SaveChangesFilter());
            config.Filters.Add(new ValidateModelAttribute());
            //config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize;
            //config.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}