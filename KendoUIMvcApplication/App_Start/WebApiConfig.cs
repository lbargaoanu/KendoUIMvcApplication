using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.ModelBinding;
using Infrastructure.Web;
using Infrastructure.Web.GridProfile;
using Kendo.Mvc.UI;
using StructureMap;
using DataSourceRequestModelBinder = Infrastructure.Web.DataSourceRequestModelBinder;

namespace KendoUIMvcApplication
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.DependencyResolver = new StructureMapResolver(ObjectFactory.Container);
            ObjectFactory.Configure(c => c.For<IDependencyResolver>().Use(config.DependencyResolver));

            config.BindParameter(typeof(DataSourceRequest), new DataSourceRequestModelBinder());
            config.BindParameter(typeof(GridProfileDataSourceRequest), new GridProfileDataSourceRequestModelBinder());

            var formatters = config.Formatters;
            formatters.Remove(formatters.XmlFormatter);
            //formatters.JsonFormatter.SerializerSettings.Converters.Add(new JQueryArrayConverter());
            config.Filters.Add(new SaveChangesFilter());
            config.Filters.Add(new ValidateModelAttribute(config));
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