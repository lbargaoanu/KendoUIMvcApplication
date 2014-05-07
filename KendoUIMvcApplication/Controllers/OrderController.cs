using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Http;
using AutoMapper;

namespace KendoUIMvcApplication.Controllers
{
    public class OrderController : NorthwindController<Order>
    {
        protected override void Modify(Order order)
        {
            var existingOrder = Context.Orders.GetWithInclude(order.Id, e => e.OrderDetails);
            SetRowVersion(order, existingOrder);
            foreach(var existingDetail in existingOrder.OrderDetails.ToArray())
            {
                var detail = order.OrderDetails.Find(existingDetail.Id);
                if(detail == null)
                {
                    Context.OrderDetails.Remove(existingDetail);
                }
                else
                {
                    Mapper.Map(detail, existingDetail);
                }
            }
            foreach(var detail in order.OrderDetails)
            {
                if(!detail.Exists || existingOrder.OrderDetails.Find(detail.Id) == null)
                {
                    existingOrder.OrderDetails.Add(detail);
                }
            }
            Mapper.Map(order, existingOrder);
        }
    }
}
