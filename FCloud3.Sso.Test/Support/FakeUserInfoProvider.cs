namespace FCloud3.Sso.Test.Support
{
    public sealed class FakeUserInfoProvider : IUserInfoProvider
    {
        private readonly int _id;
        private readonly string _name;
        private readonly byte _level;

        public FakeUserInfoProvider(int id, string name, byte level)
        {
            _id = id;
            _name = name;
            _level = level;
        }

        public int GetUserId() => _id;
        public string GetUserName() => _name;
        public byte GetUserLevel() => _level;
    }
}
