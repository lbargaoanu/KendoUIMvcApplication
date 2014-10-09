using System.Data.Common;
using System.Data.Entity;
using Infrastructure.Web;
using StructureMap.Configuration.DSL;

namespace Customers
{
    public class CustomersRegistry : Registry
    {
        public CustomersRegistry()
        {
            this.RegisterContext<CustomerContext>();
        }
    }

    public abstract class CustomersQueryHandler<TQuery, TResponse> : QueryHandler<CustomerContext, TQuery, TResponse> where TQuery : IQuery<TResponse>
    {
    }

    public abstract class CustomersCommandHandler<TCommand, TResult> : CommandHandler<CustomerContext, TCommand, TResult> where TCommand : ICommand<TResult>
    {
    }

    public abstract class CustomersCommandHandler<TCommand> : CommandHandler<CustomerContext, TCommand> where TCommand : ICommand
    {
    }

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