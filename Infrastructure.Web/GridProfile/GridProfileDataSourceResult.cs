using Kendo.Mvc.UI;

namespace Infrastructure.Web.GridProfile
{
    public class GridProfileDataSourceResult : DataSourceResult
    {
        public GridProfileDataSourceResult(){}

        public GridProfileDataSourceResult(DataSourceResult result)
        {
            this.AggregateResults = result.AggregateResults;
            this.Data = result.Data;
            this.Errors = result.Errors;
            this.Total = result.Total;
        }
        public string Profile { get; set; }
    }
}
