using System;
using System.Linq;
using AutoMapper;
using Customers;
using Customers.Controllers;
using FluentAssertions;
using KendoUIMvcApplication;
using Xunit;
using Xunit.Extensions;

namespace Test.Customers.Integration
{
    public class CustomerControllerTests : CustomersControllerTests<CustomerController, Customer>
    {
    }
}
