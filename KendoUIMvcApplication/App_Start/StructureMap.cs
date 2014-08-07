using Customers;
using Infrastructure.Web;
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
                i.For<ProductServiceContext>().HttpContextScoped().Use<ProductServiceContext>().SelectConstructor(()=>new ProductServiceContext());
                i.For<CustomerContext>().HttpContextScoped().Use<CustomerContext>().SelectConstructor(() => new CustomerContext());
                i.Scan(s =>
                {
                    s.AssemblyContainingType<IMediator>();
                    s.AssemblyContainingType<ProductServiceContext>();
                    s.AssemblyContainingType<CustomerContext>();
                    s.WithDefaultConventions();
                    s.AddAllTypesOf(typeof(IQueryHandler<,>));
                    s.AddAllTypesOf(typeof(ICommandHandler<,>));
                });
            });
        }
    }
}