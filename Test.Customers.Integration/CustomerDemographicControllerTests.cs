using System;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using KendoUIMvcApplication;
using Customers.Controllers;
using Customers;
using Xunit;
using Xunit.Extensions;

namespace Test.Customers.Integration
{
    public class CustomerDemographicControllerTests : CustomersControllerTests<CustomerDemographicController, CustomerDemographic>
    {
    }
}
