using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Infrastructure.Test;
using Infrastructure.Web;
using Northwind;
using Northwind.Controllers;
using Xunit.Extensions;

namespace Test.Northwind.Integration
{
    public class CategoryControllerTests : NorthwindControllerTests<CategoryController, Category>
    {
        [ContextAutoData, Theory]
        public void NonGenericFilter(ProductServiceContext context)
        {
            var ids = Enumerable.Range(1, Extensions.CollectionCount);
            var categories = Utils.GetEntities(context, typeof(Category), ids);
            var typedCategories = context.Categories.Where(c => ids.Contains(c.Id));

            categories.ShouldAllBeEquivalentTo(typedCategories);
        }
    }
}
