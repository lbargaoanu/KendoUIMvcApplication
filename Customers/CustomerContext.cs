using System.Data.Common;
using System.Data.Entity;
using Infrastructure.Web;

namespace Customers
{
    public class CustomerContext : BaseContext
    {
        public CustomerContext(DbConnection connection) : base(connection)
        {
        }

        public CustomerContext() : base("ProductServiceContext")
        {
        }

        public virtual DbSet<CustomerDemographic> CustomerDemographics { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
    }
}