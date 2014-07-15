using System;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using KendoUIMvcApplication;
using KendoUIMvcApplication.Controllers;
using Xunit;
using Xunit.Extensions;

namespace Test.Northwind.Integration
{
    public class ShipperControllerTests : NorthwindControllerTests<ShipperController, Shipper>
    {
    }
}
