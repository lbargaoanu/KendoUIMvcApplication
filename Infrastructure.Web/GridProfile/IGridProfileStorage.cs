namespace Infrastructure.Web.GridProfile
{
    public interface IGridProfileStorage
    {
        void SaveProfile(string gridId, string profile);
        string LoadProfile(string gridId);
    }
}
