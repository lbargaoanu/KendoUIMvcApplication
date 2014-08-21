using Kendo.Mvc.UI;

namespace Infrastructure.Web.GridProfile
{
    public class GridProfileDataSourceRequest : DataSourceRequest
    {
        public GridProfileDataSourceRequest(){}

        public GridProfileDataSourceRequest(DataSourceRequest request)
        {
            this.Aggregates = request.Aggregates;
            this.Filters = request.Filters;
            this.Groups = request.Groups;
            this.Page = request.Page;
            this.PageSize = request.PageSize;
            this.Sorts = request.Sorts;
        }

        public bool IncludeProfile { get; set; }
        public string GridId { get; set; }
    }
}
