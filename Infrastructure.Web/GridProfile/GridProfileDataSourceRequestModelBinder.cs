using Kendo.Mvc.UI;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Infrastructure.Web.GridProfile
{
    public class GridProfileDataSourceRequestModelBinder : DataSourceRequestModelBinder
    {
        public override bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (base.BindModel(actionContext, bindingContext))
            {
                var request = new GridProfileDataSourceRequest((DataSourceRequest)bindingContext.Model);

                string gridId;
                bool includeProfile;

                if (TryGetValue(bindingContext, "gridId", out gridId))
                {
                    request.GridId = gridId;
                }

                if (TryGetValue(bindingContext, "includeProfile", out includeProfile))
                {
                    request.IncludeProfile = includeProfile;
                }

                bindingContext.Model = request;
                return true;
            }
            return false;
        }
    }
}
