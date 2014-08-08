using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Web;

namespace Common
{
    public class NotifyCustomer : ICommand
    {
        public int ProductId { get; set; }
    }
}
