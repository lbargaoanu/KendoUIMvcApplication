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
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //var httpClient = new HttpClient();
            //var content = httpClient.PostAsJsonAsync("http://localhost:47148/api/GridProfile", new GridProfile { GridId = "ss", Children = new Child[]{ new Child { Name = "ABC" }, new Child { Name = "XYZ" } } }).Result.Content.ReadAsStringAsync().Result;
            //ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
