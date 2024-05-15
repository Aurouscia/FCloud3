using FCloud3.App.Utils;
using FCloud3.WikiPreprocessor.Util;

namespace FCloud3.App.Services.Utils
{
    public class LocatorHash : ILocatorHash
    {
        public string? Hash(string? input)
        {
            return (input ?? "").GetMD5();
        }
    }
}
