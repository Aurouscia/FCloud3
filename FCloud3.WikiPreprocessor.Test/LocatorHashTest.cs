using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Util;
using System.Security.Cryptography;
using System.Text;
using FCloud3.WikiPreprocessor.Test.Support;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class LocatorHashTest
    {
        public static IEnumerable<object[]> ParagraphTestData()
        {
            yield return new object[] {
                "你好，很高兴认识你",
                "4c38b20edd380a3584fb704ce66a4e2c"
            };
            yield return new object[] {
                "你好，很高兴认识你\n他这么说到",
                "4c38b20edd380a3584fb704ce66a4e2c;91768cb2f78f2e57b76d3b71a3329d7f"
            };
            yield return new object[] {
                "> 你好，很高兴认识你\r\n他这么说到",
                "ec2a33b330dcefed0ef6a4b2d4c1e5af;91768cb2f78f2e57b76d3b71a3329d7f"
            };
            yield return new object[] {
                "> 你好，{{greeting}param1\nparam2\nparam3}\n他这么说到",
                "0a1ed23087feee50cedbed32bcfde740;91768cb2f78f2e57b76d3b71a3329d7f"
            };
        }

        [TestMethod]
        [DynamicData(nameof(ParagraphTestData))]
        public void Paragraph(string input,string answer)
        {
            var shouldAppear = answer.Split(';');
            var parser = new ParserBuilder()
                .UseLocatorHash(new LocatorHash())
                .BuildParser();
            var res = parser.RunToPlain(input);
            Assert.IsTrue( shouldAppear.All(x=>res.Contains(x)));
        }

        public static IEnumerable<object[]> TitleTestData()
        {
            yield return new object[] {
                "# 你好，很高兴认识你\r\n他这么说到",
                "4dcf93fa310b469c089421c509325a79;91768cb2f78f2e57b76d3b71a3329d7f"
            };
        }

        [TestMethod]
        [DynamicData(nameof(TitleTestData))]
        public void Title(string input,string answer)
        {
            var shouldAppear = answer.Split(';');
            var parser = new ParserBuilder()
                .UseLocatorHash(new LocatorHash())
                .BuildParser();
            var res = parser.RunToPlain(input);
            Assert.IsTrue(shouldAppear.All(x => res.Contains(x)));
        }


        public class LocatorHash:ILocatorHash
        {
            public string? Hash(string? input)
            {
                if (input is null)
                    return null;
                byte[] data = MD5.HashData(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            } 
        }
    }
}
