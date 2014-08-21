using Infrastructure.Web.GridProfile;

namespace KendoUIMvcApplication.Infrastructure
{
    public class GridProfileStorage:IGridProfileStorage
    {
        private static string _profile;

        public void SaveProfile(string gridId, string profile)
        {
            _profile = profile;
        }

        public string LoadProfile(string gridId)
        {
            return _profile;
        }
    }
}