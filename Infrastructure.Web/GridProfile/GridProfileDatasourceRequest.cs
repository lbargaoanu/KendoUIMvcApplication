using Kendo.Mvc.UI;

namespace Infrastructure.Web.GridProfile
{
    public class GridProfileDataSourceRequest : DataSourceRequest
    {
        public bool IncludeProfile { get; set; }
        public string GridId { get; set; }
    }
}
