using Infrastructure.Web;
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
                i.Scan(s =>
                {
                    s.AssemblyContainingType<IMediator>();
                    s.AssemblyContainingType<ProductServiceContext>();
                    s.WithDefaultConventions();
                    s.AddAllTypesOf(typeof(IQueryHandler<,>));
                    s.AddAllTypesOf(typeof(ICommandHandler<,>));
                });
            });
        }
    }
}