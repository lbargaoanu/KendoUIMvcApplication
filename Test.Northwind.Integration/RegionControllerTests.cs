using System;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using KendoUIMvcApplication;
using Northwind.Controllers;
using Northwind;
using Xunit;
using Xunit.Extensions;

namespace Test.Northwind.Integration
{
    public class RegionControllerTests : NorthwindControllerTests<RegionController, Region>
    {
    }
}
