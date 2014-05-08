using System.Data.Entity;
using System.Linq;
using AutoMapper;

namespace KendoUIMvcApplication
{
    public class ModifyOrder : ICommand
    {
        public Order Order { get; set; }
    }

    public class ModifyOrderHandler : ProductsCommandHandler<ModifyOrder>
    {
        public override void Handle(ModifyOrder command)
        {
            var order = command.Order;
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