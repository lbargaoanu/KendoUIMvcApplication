using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using StructureMap;

namespace Infrastructure.Web
{
    public sealed class StructureMapResolver : StructureMapDependencyScope, IDependencyResolver
    {
        public StructureMapResolver(IContainer container):base(container)
        {
        }

        public IDependencyScope BeginScope()
        {
            return new StructureMapDependencyScope(Container.GetNestedContainer());
        }

        protected override void Dispose(bool disposing)
        {
        }
    }

    public class StructureMapDependencyScope : IDependencyScope
    {
        public IContainer Container { get; private set; }

        public StructureMapDependencyScope(IContainer container)
        {
            this.Container = container;
        }

        public object GetService(Type serviceType)
        {
            return serviceType.IsAbstract ? Container.TryGetInstance(serviceType) : Container.GetInstance(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Container.GetAllInstances(serviceType).Cast<object>();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(Container != null)
            {
                Container.Dispose();
                Container = null;
            }
        }
    }
}