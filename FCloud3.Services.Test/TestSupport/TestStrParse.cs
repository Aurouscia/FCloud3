using FCloud3.Entities.Identities;

namespace FCloud3.Services.Test.TestSupport
{
    public class TestStrParse
    {
        public static List<int> IntList(string str, char sep=',')
        {
            if (string.IsNullOrEmpty(str))
                return [];
            return str.Split(sep).ToList().ConvertAll(int.Parse);
        }
        public static List<AuthGrant> AuthGrants(string str, AuthGrantOn on, int onId)
        {
            if (string.IsNullOrWhiteSpace(str))
                return [];
            var build = (AuthGrantTo to, int toId, bool isReject) => new AuthGrant()
            {
                To = to, ToId = toId, On = on, OnId = onId, IsReject = isReject
            };
            return str.Split("  ").ToList().ConvertAll(x =>
            {
                var t = x[0];
                var i = 0;
                if (x.ElementAtOrDefault(1) is char ic)
                    int.TryParse(ic.ToString(), out i);
                bool r = x.Last() == '!';
                if (t == 'e')
                    return build(AuthGrantTo.EveryOne, 0, r);
                if (t == 'u')
                    return build(AuthGrantTo.User, int.Parse(i.ToString()), r);
                if (t == 'g')
                    return build(AuthGrantTo.UserGroup, int.Parse(i.ToString()), r);
                return new();
            });
        }
    }
}