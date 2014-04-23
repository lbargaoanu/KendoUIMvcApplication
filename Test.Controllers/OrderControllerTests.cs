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
    public class OrderControllerTests : ControllerTests<OrderController, Order>
    {
        [RepeatTheory(1), MyAutoData]
        public void ShouldAddWithDetails(Order newEntity, OrderDetail[] details, ProductServiceContext readContext)
        {
            newEntity.OrderDetails.Add(details);
            
            base.ShouldAdd(newEntity, readContext);

            var found = readContext.Orders.GetWithInclude(newEntity.Id, o => o.OrderDetails);
            found.OrderDetails.ShouldBeTheSameAs(details);
        }

        [RepeatTheory(1), MyAutoData]
        public void ShouldModifyWithDetails(Order newEntity, OrderDetail[] newDetails, Order modified, OrderDetail[] modifiedDetails, ProductServiceContext createContext, ProductServiceContext readContext)
        {
            newEntity.OrderDetails.Add(newDetails);
            modified.OrderDetails.Add(modifiedDetails);

            base.ShouldModify(newEntity, modified, createContext, readContext);

            foreach(var detail in modifiedDetails)
            {
                detail.OrderID = newEntity.Id;
            }
            var found = readContext.Orders.GetWithInclude(newEntity.Id, o => o.OrderDetails);
            found.OrderDetails.ShouldAllBeQuasiEquivalentTo(modifiedDetails);
        }
    }
}