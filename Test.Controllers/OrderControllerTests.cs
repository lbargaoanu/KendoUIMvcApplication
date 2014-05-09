using System.Linq;
using FluentAssertions;
using KendoUIMvcApplication;
using KendoUIMvcApplication.Controllers;
using Xunit;
using Xunit.Extensions;

namespace Test.Controllers.Integration
{
    public class OrderControllerTests : ControllerTests<OrderController, Order>
    {
        [Theory, MyAutoData]
        public override void ShouldDelete(Order newEntity, ProductServiceContext createContext, ProductServiceContext readContext)
        {
            base.ShouldDelete(newEntity, createContext, readContext);
            foreach(var detail in newEntity.OrderDetails)
            {
                Assert.Null(readContext.OrderDetails.Find(detail.Id));
            }
        }

        [Theory, MyAutoData]
        public void ShouldAddWithDetails(Order newEntity, OrderDetail[] details, ProductServiceContext readContext)
        {
            newEntity.OrderDetails.Add(details);
            
            base.ShouldAdd(newEntity, readContext);

            CheckDetails(newEntity, details, readContext);
        }

        private static void CheckDetails(Order newEntity, OrderDetail[] details, ProductServiceContext readContext)
        {
            var found = readContext.Orders.GetWithInclude(newEntity.Id, o => o.OrderDetails);
            found.OrderDetails.ShouldAllBeEquivalentTo(details);
        }

        [Theory, MyAutoData]
        public void ShouldModifyWithDetails(Order newEntity, OrderDetail[] newDetails, Order modified, OrderDetail[] modifiedDetails, ProductServiceContext createContext, ProductServiceContext readContext)
        {
            newEntity.OrderDetails.Add(newDetails);
            modified.OrderDetails.Add(modifiedDetails);

            base.ShouldModify(newEntity, modified, createContext, readContext);

            CheckDetails(newEntity, modifiedDetails, readContext);
        }

        protected override void Map(Order source, Order destination)
        {
            base.Map(source, destination);
            destination.OrderDetails = source.OrderDetails;
        }

        [Theory, MyAutoData]
        public void ShouldModifyJustDetails(Order newEntity, OrderDetail[] newDetails, Order modified, OrderDetail[] modifiedDetails, ProductServiceContext createContext, ProductServiceContext readContext)
        {
            // arrange
            newEntity.OrderDetails.Add(newDetails);
            createContext.AddAndSave(newEntity);
            foreach(var detail in modifiedDetails)
            {
                detail.OrderID = newEntity.Id;
            }
            var detailsCount = createContext.OrderDetails.Count();
            modified.Id = newEntity.Id;
            modified.RowVersion = newEntity.RowVersion;
            modified.OrderDetails.Add(modifiedDetails);
            for(int index = 0; index < modifiedDetails.Length; index++)
            {
                modifiedDetails[index].Id = newDetails[index].Id;
            }
            Map(modified, newEntity);
            // act
            var response = new OrderController().PutAndSave(newEntity);
            // assert
            response.AssertIsOk(newEntity);

            createContext.OrderDetails.Count().Should().Be(detailsCount, "nothing should be inserted in FK tables");
            var found = readContext.Orders.GetWithInclude(newEntity.Id, o => o.OrderDetails);
            found.OrderDetails.ShouldHaveTheSameIdsAs(modifiedDetails); 
            found.OrderDetails.ShouldAllBeQuasiEquivalentTo(modifiedDetails);
        }
    }
}