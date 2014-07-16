using System;
using System.Data.Entity;
using System.Linq;
using Infrastructure.Web;

namespace Northwind
{
    public class ProductResponse
    {
        public IQueryable<Product> Data { get; set; }
    }

    public class ProductQuery : IQuery<ProductResponse>
    {
        public bool Discontinued { get; set; }
    }

    public class AllProductsHandler : ProductsQueryHandler<ProductQuery, ProductResponse>
    {
        public override ProductResponse Handle(ProductQuery query)
        {
            return new ProductResponse { Data = Context.Products.Where(p => p.Discontinued == query.Discontinued) };
        }
    }

    public class ModifyProduct : ICommand
    {
        public Product Product { get; set; }
    }

    public class ModifyProductHandler : ProductsCommandHandler<ModifyProduct>
    {
        public IDisposable Disposable { get; set; }

        public override void Handle(ModifyProduct command)
        {
            Context.Entry(command.Product).State = EntityState.Modified;
            Disposable.Dispose();
        }
    }
}