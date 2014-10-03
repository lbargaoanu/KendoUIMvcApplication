using Customers;
using Infrastructure.Test;
using Infrastructure.Web;
using KendoUIMvcApplication;
using Ploeh.AutoFixture;

namespace Test.Customers.Integration
{
    public abstract class CustomersControllerTests<TController, TEntity> : ControllerTests<TController, CustomerContext, TEntity>
        where TController : CrudController<CustomerContext, TEntity>, new()
        where TEntity : VersionedEntity
    {
        static CustomersControllerTests()
        {
            Initialize(Initializer);
        }

        public static void Initializer(CustomerContext context, IFixture fixture)
        {
            KendoUIMvcApplication.StructureMap.Register();
            //var categories = fixture.CreateMany<Category>();
            //var suppliers = fixture.CreateMany<Supplier>();
            //fixture.Customize<Region>(c => c.Without(r => r.Territories));
            //var regions = fixture.CreateMany<Region>();
            //fixture.Customize<Territory>(c => c.Without(r => r.Employees));
            //var territories = fixture.CreateMany<Territory>();

            //context.Categories.AddRange(categories);
            //context.Suppliers.AddRange(suppliers);
            //context.Regions.AddRange(regions);
            //context.Territories.AddRange(territories);
        }
    }
}