using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Http;
using AutoMapper;

namespace Northwind.Controllers
{
    public class OrderController : NorthwindController<Order>
    {
        protected override void Modify(Order order)
        {
            Send(new ModifyOrder { Order = order });
        }
    }
}