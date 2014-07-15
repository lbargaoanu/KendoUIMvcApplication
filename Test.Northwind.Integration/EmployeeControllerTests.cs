using System;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using KendoUIMvcApplication;
using KendoUIMvcApplication.Controllers;
using Xunit;
using Xunit.Extensions;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Infrastructure.Test;
using Infrastructure.Web;

namespace Test.Northwind.Integration
{
    public class EmployeeControllerTests : NorthwindControllerTests<EmployeeController, Employee>
    {
        [Theory, ContextAutoData]
        public override void ShouldAdd(Employee newEntity, ProductServiceContext readContext)
        {
            var territoryCount = readContext.Territories.Count();
            newEntity.Territories.Set(1, 2, 3, 4, 5);
            
            base.ShouldAdd(newEntity, readContext);

            AssertTerritories(newEntity.Id, newEntity, readContext, territoryCount);
        }

        [Theory, ContextAutoData]
        public override void ShouldModify(Employee newEntity, Employee modified, ProductServiceContext createContext, ProductServiceContext readContext)
        {
            var territoryCount = readContext.Territories.Count();
            newEntity.Territories.Set(createContext, 1, 2, 3, 4, 5);
            modified.Territories.Set(3, 4, 5, 6, 7, 8);
            
            base.ShouldModify(newEntity, modified, createContext, readContext);

            AssertTerritories(newEntity.Id, modified, readContext, territoryCount);
        }

        protected override void Map(Employee source, Employee destination)
        {
            base.Map(source, destination);
            destination.Territories = source.Territories;
        }

        private static void AssertTerritories(int id, Employee expectation, ProductServiceContext readContext, int territoryCount)
        {
            var found = readContext.Employees.GetWithInclude(id, e => e.Territories);
            found.Territories.ShouldHaveTheSameIdsAs(expectation.Territories);
            readContext.Territories.Count().Should().Be(territoryCount, "nothing should be inserted in FK tables");
        }
    }
}