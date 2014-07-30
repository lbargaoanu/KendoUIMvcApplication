using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Http;

namespace Northwind.Controllers
{
    public class ProductController : NorthwindController<Product>
    {
        public override IQueryable<Product> Include(IQueryable<Product> entities)
        {
            return entities.Include(p=>p.Category).Include(p=>p.Supplier);
        }

        public override IQueryable<Product> GetAllEntities()
        {
            return Get(new ProductQuery { Discontinued = false }).Data;
        }

        protected override void Modify(Product entity)
        {
            Send(new ModifyProduct { Product = entity });
        }
    }
}