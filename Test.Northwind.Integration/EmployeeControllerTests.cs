using System;
using Moq;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using KendoUIMvcApplication;
using Northwind.Controllers;
using Northwind;
using Xunit;
using Xunit.Extensions;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Infrastructure.Test;
using Infrastructure.Web;
using System.Web.Http.Metadata;

namespace Test.Northwind.Integration
{
    public class EmployeeControllerTests : NorthwindControllerTests<EmployeeController, Employee>
    {
        [ContextAutoData, Theory]
        public void FillNomenclatorCollection(Employee newEntity, ProductServiceContext context)
        {
            var ids = Enumerable.Range(1, Extensions.CollectionCount).ToArray();
            newEntity.Territories.Set(ids);
            newEntity.Territories = newEntity.Territories.ToList();

            var metadataProvider = new Mock<ModelMetadataProvider>();
            var properties = typeof(Employee).GetProperties().Select(p => new ModelMetadata(metadataProvider.Object, typeof(Employee), () => p.GetValue(newEntity), p.PropertyType, p.Name));
            metadataProvider.Setup(m => m.GetMetadataForProperties(newEntity, typeof(Employee))).Returns(properties);

            Utils.Set(newEntity, typeof(Employee), context, metadataProvider.Object);

            newEntity.Territories.ShouldAllBeEquivalentTo(context.Territories.Where(e => ids.Contains(e.Id)));
        }

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