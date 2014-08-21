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
        public static void RegisterContext<TContext>(this IInitializationExpression init) where TContext : BaseContext, new()
        {
            init.For<TContext>().HttpContextScoped().Use<TContext>().SelectConstructor(() => new TContext());
        }

        public static void Register()
        {
            ObjectFactory.Initialize(i =>
            {
                i.RegisterContext<ProductServiceContext>();
                i.RegisterContext<CustomerContext>();
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