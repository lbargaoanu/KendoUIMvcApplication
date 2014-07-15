using System;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Infrastructure.Test;
using KendoUIMvcApplication;
using KendoUIMvcApplication.Controllers;
using Xunit;
using Xunit.Extensions;

namespace Test.Northwind.Integration
{
    public class ProductControllerTests : NorthwindControllerTests<ProductController, Product>
    {
        public static void Customize(Product entity)
        {
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