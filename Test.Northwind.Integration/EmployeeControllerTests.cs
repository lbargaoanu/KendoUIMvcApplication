using System.Data.Entity;
using System.Linq;
using FluentAssertions;
using Infrastructure.Test;
using Infrastructure.Web;
using Northwind;
using Northwind.Controllers;
using Xunit.Extensions;

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

            var metadataProvider = new TestChildCollectionsModelMetadataProvider(newEntity);

            Utils.SetNomCollectionsUnchanged(newEntity, typeof(Employee), context, metadataProvider);

            newEntity.Territories.Select(t => context.Entry(t).State).Count(s => s == EntityState.Unchanged).Should().Be(Extensions.CollectionCount);
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