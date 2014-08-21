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

    public class GridProfile
    {
        public string GridId { get; set; }
        public string State { get; set; }
    }
}
