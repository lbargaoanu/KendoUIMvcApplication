using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using StructureMap;

namespace KendoUIMvcApplication
{
    public sealed class StructureMapResolver : StructureMapDependencyScope, IDependencyResolver
    {
        public StructureMapResolver(IContainer container):base(container)
        {
        }

        public IDependencyScope BeginScope()
        {
            return new StructureMapDependencyScope(container.GetNestedContainer());
        }

        protected override void Dispose(bool disposing)
        {
        }
    }

    public class StructureMapDependencyScope : IDependencyScope
    {
        protected IContainer container;

        public StructureMapDependencyScope(IContainer container)
        {
            this.container = container;
        }

        public object GetService(Type serviceType)
        {
            return serviceType.IsAbstract ? container.TryGetInstance(serviceType) : container.GetInstance(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return container.GetAllInstances(serviceType).Cast<object>();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(container != null)
            {
                container.Dispose();
                container = null;
            }
        }
    }
}