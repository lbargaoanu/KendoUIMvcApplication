using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Http;
using AutoMapper;

namespace KendoUIMvcApplication.Controllers
{
    public class OrderController : NorthwindController<Order>
    {
        protected override void Modify(Order entity)
        {
            var existingEntity = Context.Orders.GetWithInclude(entity.Id, e => e.OrderDetails);
            SetRowVersion(entity, existingEntity);
            foreach(var existingDetail in existingEntity.OrderDetails.ToArray())
            {
                var newDetail = entity.OrderDetails.SingleOrDefault(d => d.Id == existingDetail.Id);
                if(newDetail == null)
                {
                    Context.OrderDetails.Remove(existingDetail);
                }
                else
                {
                    Mapper.Map(newDetail, existingDetail);
                }
            }
            //Context.OrderDetails.RemoveRange(existingEntity.OrderDetails.Where(d=>!entity.OrderDetails.Any(od=>od.Id==d.Id)));
            Mapper.Map(entity, existingEntity);
            existingEntity.OrderDetails.Set(Context);
        }
    }
}
