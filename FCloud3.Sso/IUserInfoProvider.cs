namespace FCloud3.Sso
{
    public interface IUserInfoProvider
    {
        int GetUserId();
        string GetUserName();
        byte GetUserLevel();
    }
}
