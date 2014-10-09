using Customers;
using Infrastructure.Web;
using Infrastructure.Web.GridProfile;
using KendoUIMvcApplication.Infrastructure;
using Northwind;
using StructureMap;
using StructureMap.Web;

namespace KendoUIMvcApplication
{
    public static class StructureMap
    {
        public static void Register()
        {
            ObjectFactory.Initialize(i =>
            {
                i.Scan(scan =>
                {
                    scan.AssemblyContainingType<IMediator>();
                    scan.AssemblyContainingType<ProductServiceContext>();
                    scan.AssemblyContainingType<CustomerContext>();
                    scan.WithDefaultConventions();
                    scan.AddAllTypesOf(typeof(IQueryHandler<,>));
                    scan.AddAllTypesOf(typeof(ICommandHandler<,>));
                    scan.LookForRegistries();
                });
            });
        }
    }
}