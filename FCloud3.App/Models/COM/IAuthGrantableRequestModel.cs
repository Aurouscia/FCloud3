using FCloud3.Entities.Identities;

namespace FCloud3.App.Models.COM
{
    public interface IAuthGrantableRequestModel
    {
        public int AuthGrantOnId { get; }
    }
    public interface IAuthGrantableRequstModelWithOn: IAuthGrantableRequestModel
    {
        public AuthGrantOn AuthGrantOnType { get; }
    }
}
