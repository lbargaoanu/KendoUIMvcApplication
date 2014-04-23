using System.Linq;
using KendoUIMvcApplication;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;

namespace Test.Controllers.Integration
{
    public static partial class ContextHelper
    {
        private static void SeedDatabase(ProductServiceContext context, IFixture fixture)
        {
            var categories = fixture.CreateMany<Category>().ToArray();
            var suppliers = fixture.CreateMany<Supplier>().ToArray();
            fixture.Customize<Region>(c => c.Without(r => r.Territories));
            var regions = fixture.CreateMany<Region>().ToArray();
            fixture.Customize<Territory>(c => c.Without(r => r.Employees));
            var territories = fixture.CreateMany<Territory>().ToArray();

            context.Categories.AddRange(categories);
            context.Suppliers.AddRange(suppliers);
            context.Regions.AddRange(regions);
            context.Territories.AddRange(territories);
        }
    }
}