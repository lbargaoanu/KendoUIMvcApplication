using System.Data.Entity;
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
                i.For<DbContext>().HttpContextScoped().Use<ProductServiceContext>();
                i.Forward<DbContext, ProductServiceContext>();
                i.Scan(s =>
                {
                    s.AssemblyContainingType<IMediator>();
                    s.WithDefaultConventions();
                    s.AddAllTypesOf(typeof(IQueryHandler<,>));
                    s.AddAllTypesOf(typeof(ICommandHandler<,>));
                });
            });
        }
    }
}