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
            var categories = fixture.CreateMany<Category>();
            var suppliers = fixture.CreateMany<Supplier>();
            fixture.Customize<Region>(c => c.Without(r => r.Territories));
            var regions = fixture.CreateMany<Region>();
            fixture.Customize<Territory>(c => c.Without(r => r.Employees));
            var territories = fixture.CreateMany<Territory>();

            context.Categories.AddRange(categories);
            context.Suppliers.AddRange(suppliers);
            context.Regions.AddRange(regions);
            context.Territories.AddRange(territories);
        }
    }
}