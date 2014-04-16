using System.Linq;
using AutoMapper;
using FluentAssertions;
using KendoUIMvcApplication;
using KendoUIMvcApplication.Controllers;
using Xunit;
using Xunit.Extensions;

namespace Test.Controllers.Integration
{
    public class ProductControllerTests : ControllerTests<ProductController, Product>
    {
        public static void Customize(Product entity)
        {
        }

        //[RepeatTheory(1), MyAutoData]
        //public override void ShouldModify(Product modified, ProductServiceContext createContext, ProductServiceContext readContext)
        //{
        //    //createContext.AddAndSave(modified.Category);
        //    //createContext.AddAndSave(modified.Supplier);
        //    modified.Category = readContext.Categories.First();
        //    modified.Supplier = readContext.Suppliers.First();
        //    base.ShouldModify(modified, createContext, readContext);
        //}

        //[RepeatTheory(1), MyAutoData]
        //public void ShouldReturnAllProducts(Product[] products, ProductServiceContext createContext, AllProductsHandler handler)
        //{
        //    // arrange
        //    createContext.AddAndSave(products);
        //    var count = products.Count(p => !p.Discontinued);
        //    // act
        //    var response = handler.Handle(new ProductQuery { Discontinued = false });
        //    // assert
        //    response.Data.Count().Should().BeGreaterOrEqualTo(count, "Se poate sa avem date din alte teste.");
        //}

        //[RepeatTheory(1), MyAutoData]
        //public void ShouldModifyTheProduct(Product modified, ProductServiceContext createContext, ProductServiceContext readContext)
        //{
        //    // arrange
        //    var product = createContext.AddAndSave(new Product());
        //    modified.Id = product.Id;
        //    Mapper.Map(modified, product);
        //    // act
        //    new ModifyProductHandler().HandleAndSave(new ModifyProduct { Product = product });
        //    // assert
        //    product = readContext.Products.Find(product.Id);
        //    product.ShouldBeEquivalentTo(modified);
        //}

        //public class DeleteProduct
        //{
        //    [RepeatTheory(1), MyAutoData]
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

        //    [RepeatFact(1)]
        //    public void ShouldDetectNotExistingProduct()
        //    {
        //        new ProductController().DeleteAndSave(0).AssertIsNotFound();
        //    }
        //}

        //public class AddProduct
        //{
        //    [RepeatTheory(1), MyAutoData]
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