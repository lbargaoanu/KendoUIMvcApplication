using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Infrastructure.Web.GridProfile;
using Northwind;

namespace KendoUIMvcApplication.Controllers
{
    [NoCache]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var httpClient = new HttpClient();
            var content = httpClient.PostAsJsonAsync("http://localhost:47148/api/GridProfile", new GridProfile { GridId = "ss", Children = new Child[] { new Child { Name = "ABC" }, new Child { Name = "XYZ" } } }).Result.Content.ReadAsStringAsync().Result;
            //ViewBag.Message = "Welcome to ASP.NET MVC!";

            return null;// View();
        }

        public ActionResult About()
        {
            return View();
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class NoCacheAttribute : FilterAttribute, IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var cache = filterContext.HttpContext.Response.Cache;
            cache.SetCacheability(HttpCacheability.NoCache);
            cache.SetRevalidation(HttpCacheRevalidation.ProxyCaches);
            cache.SetExpires(DateTime.Now.AddYears(-5));
            cache.AppendCacheExtension("private");
            cache.AppendCacheExtension("no-cache=Set-Cookie");
            cache.SetProxyMaxAge(TimeSpan.Zero);
        }
    }
}
