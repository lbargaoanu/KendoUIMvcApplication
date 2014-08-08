using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Http;
using Common;

namespace Customers.Controllers
{
    public class CustomerController : CustomersController<Customer>
    {
    }

    public class NotifyCustomerHandler : CustomersCommandHandler<NotifyCustomer>
    {
        public override void Handle(NotifyCustomer command)
        {
        }
    }
}
