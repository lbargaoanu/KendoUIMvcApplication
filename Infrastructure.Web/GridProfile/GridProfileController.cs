using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Web.Http;

namespace Infrastructure.Web.GridProfile
{
    public class GridProfileController : ApiController, IContextController
    {
        private readonly DbContext context = (DbContext)Activator.CreateInstance(Type.GetType("Northwind.ProductServiceContext, Northwind"));
        private readonly IGridProfileStorage gridProfileStorage;

        public GridProfileController(IGridProfileStorage gridProfileStorage)
        {
            this.gridProfileStorage = gridProfileStorage;
        }

        public IHttpActionResult Get(string id)
        {
            return Ok(gridProfileStorage.LoadProfile(id));
        }

        public IHttpActionResult Post(GridProfile profile)
        {
            gridProfileStorage.SaveProfile(profile.GridId, profile.State);
            return CreatedAtRoute("DefaultApi", new { id = profile.GridId }, profile);
        }

        public System.Data.Entity.DbContext Context
        {
            get { return context; }
        }
    }

    public class Child : NomEntity, IValidatableObject
    {
        [StringLength(100, MinimumLength=10)]
        public string Name { get; set; }

        public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new ValidationResult[0];
        }
    }

    public class GridProfile : Entity, IValidatableObject
    {
        [Required]
        public string GridId { get; set; }
        public string State { get; set; }

        public ICollection<Child> Children { get; set; }

        public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new ValidationResult[0];
        }
    }
}
