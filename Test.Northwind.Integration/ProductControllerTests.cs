using System;
using Moq;
using System.Collections.Generic;
using Common;
using Infrastructure.Test;
using Infrastructure.Web;
using Northwind;
using Northwind.Controllers;
using Xunit.Extensions;

namespace Test.Northwind.Integration
{
    using NotifyHandler = ICommandHandler<NotifyCustomer, NoResult>;

    public class ProductControllerTests : NorthwindControllerTests<ProductController, Product>
    {
        public static void Customize(Product entity)
        {
        }

        public override void ShouldModify(Product newEntity, Product modified, ProductServiceContext createContext, ProductServiceContext readContext)
        {
        }

        [Theory, ContextAutoData]
        public void ShouldModifyAndNotify(Product newEntity, Product modified, ProductServiceContext createContext, ProductServiceContext readContext, NotifyHandler handler)
        {
            base.ShouldModifyCore(newEntity, modified, createContext, readContext, GetNotifyHandler(handler));

            VerifyNotify(modified, handler);
        }

        private static void VerifyNotify(Product modified, NotifyHandler handler)
        {
            handler.Verify(h => h.Execute(It.Is<NotifyCustomer>(n => n.ProductId == modified.Id)));
        }

        private static Dictionary<Type, object> GetNotifyHandler(NotifyHandler handler)
        {
            return new Dictionary<Type, object> { { typeof(NotifyHandler), handler } };
        }

        [Theory, ContextAutoData]
        public void ShouldNotModifyConcurrentAndNotify(Product entity, ProductServiceContext createContext, ProductServiceContext modifyContext, byte[] rowVersion, NotifyHandler handler)
        {
            base.ShouldNotModifyConcurrentCore(entity, createContext, modifyContext, rowVersion, GetNotifyHandler(handler));

            VerifyNotify(entity, handler);
        }

        public override void ShouldNotModifyConcurrent(Product entity, ProductServiceContext createContext, ProductServiceContext modifyContext, byte[] rowVersion)
        {
        }

        [Theory, ContextAutoData]
        public override void ShouldGetAll(Product[] newEntities, ProductServiceContext createContext, Func<Product, bool> where)
        {
            base.ShouldGetAll(newEntities, createContext, p => !p.Discontinued);
        }

        //[Theory, MyAutoData]
        //public override void ShouldModify(Product modified, ProductServiceContext createContext, ProductServiceContext readContext)
        //{
        //    //createContext.AddAndSave(modified.Category);
        //    //createContext.AddAndSave(modified.Supplier);
        //    modified.Category = readContext.Categories.First();
        //    modified.Supplier = readContext.Suppliers.First();
        //    base.ShouldModify(modified, createContext, readContext);
        //}

        //[Theory, ContextAutoData]
        //public void ShouldReturnAllProducts(Product[] products, ProductServiceContext createContext, AllProductsHandler handler)
        //{
        //    // arrange
        //    createContext.AddAndSave(products);
        //    var count = products.Count(p => !p.Discontinued);
        //    // act
        //    var response = handler.Handle(new ProductQuery { Discontinued = false });
        //    // assert
        //    response.Data.Count().Should().BeGreaterOrEqualTo(count, "Se poate sa avem date din alte teste");
        //    Assert.True(response.Data.All(p => !p.Discontinued), "Sa nu fie nici una discontinued");
        //}

        //[Theory, ContextAutoData]
        //public void ShouldModifyTheProduct(Product modified, ProductServiceContext createContext, ProductServiceContext readContext, ModifyProductHandler handler)
        //{
        //    // arrange
        //    var product = createContext.AddAndSave(new Product());
        //    modified.Id = product.Id;
        //    modified.RowVersion = product.RowVersion;
        //    Mapper.Map(modified, product);
        //    // act
        //    handler.HandleAndSave(new ModifyProduct { Product = product });
        //    // assert
        //    product = readContext.Products.Find(product.Id);
        //    product.ShouldBeQuasiEquivalentTo(modified);
        //}

        //public class DeleteProduct
        //{
        //    [Theory, MyAutoData]
        //    public void ShouldDeleteTheProduct(ProductServiceContext createContext, ProductServiceContext readContext)
        //    {
        //        // arrange
        //        var product = createContext.AddAndSave(new Product());
        //        // act
        //        var result = new ProductController().DeleteAndSave(product.Id);
        //        // assert
        //        result.AssertIsOk(product);
        //        Assert.Null(readContext.Products.Find(product.Id));
        //    }

        //    [Fact]
        //    public void ShouldDetectNotExistingProduct()
        //    {
        //        new ProductController().DeleteAndSave(0).AssertIsNotFound();
        //    }
        //}

        //public class AddProduct
        //{
        //    [Theory, MyAutoData]
        //    public void ShouldAddTheProduct(ProductServiceContext readContext)
        //    {
        //        // arrange
        //        var product = new Product();
        //        // act
        //        var result = new ProductController().PostAndSave(product);
        //        // assert
        //        result.AssertIsCreatedAtRoute(product);
        //        Assert.NotNull(readContext.Products.Find(product.Id));
        //    }
        //}
    }
}