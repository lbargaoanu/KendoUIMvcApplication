using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Http;
using AutoMapper;
using System.Diagnostics;
using Infrastructure.Web;

namespace Northwind.Controllers
{
    public class EmployeeController : NorthwindController<Employee>
    {
        protected override void Modify(Employee entity)
        {
            var existingEntity = Context.Employees.GetWithInclude(entity.Id, e => e.Territories);
            SetRowVersion(entity, existingEntity);
            Mapper.Map(entity, existingEntity);
            existingEntity.Territories = entity.Territories;
        }
    }
}