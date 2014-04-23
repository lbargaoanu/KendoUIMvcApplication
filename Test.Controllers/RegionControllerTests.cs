using System;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using KendoUIMvcApplication;
using KendoUIMvcApplication.Controllers;
using Xunit;
using Xunit.Extensions;

namespace Test.Controllers.Integration
{
    public class RegionControllerTests : ControllerTests<RegionController, Region>
    {
    }
}
