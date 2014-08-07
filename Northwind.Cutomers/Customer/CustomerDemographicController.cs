using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Http;
using Infrastructure.Web;

namespace Customers.Controllers
{
    public class CustomersController<TEntity> : CrudController<CustomerContext, TEntity> where TEntity : VersionedEntity
    {
    }

    public class CustomerDemographicController : CustomersController<CustomerDemographic>
    {
    }
}
