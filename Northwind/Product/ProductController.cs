using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Http;
using Kendo.Mvc.UI;
using WebApi.OutputCache.V2;
using System.Web.Http.ModelBinding;
using Common;

namespace Northwind.Controllers
{
    public class ProductController : NorthwindController<Product>
    {
        //[CacheOutput(ServerTimeSpan = int.MaxValue)]
        //[CacheOutput(ClientTimeSpan = 5, MustRevalidate = true)]
        public override DataSourceResult GetAll(DataSourceRequest request)
        {
            return base.GetAll(request);
        }

        protected override IQueryable<Product> Include(IQueryable<Product> entities)
        {
            return entities.Include(p=>p.Category).Include(p=>p.Supplier);
        }

        protected override IQueryable<Product> GetAllEntities()
        {
            return Get(new ProductQuery { Discontinued = false }).Data;
        }

        protected override void Modify(Product entity)
        {
            Send(new ModifyProduct { Product = entity });
            Send(new NotifyCustomer { ProductId = entity.Id });
        }
    }
}