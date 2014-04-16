using KendoUIMvcApplication;

namespace Test.Controllers.Integration
{
    static partial class ContextHelper
    {
        private static void SeedDatabase(ProductServiceContext context)
        {
            context.Categories.Add(new Category { CategoryName = "Category1" });
            context.Suppliers.Add(new Supplier { CompanyName = "Supplier1" });
        }
    }
}