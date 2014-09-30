using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;

namespace Infrastructure.Web.GridProfile
{
    public class GridProfileController : ApiController
    {
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
    }

    public class Child : NomEntity, IValidatableObject
    {
        [StringLength(100)]
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
