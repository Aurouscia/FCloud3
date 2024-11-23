using FCloud3.Services.Test.TestSupport;
using FCloud3.Services.TextSec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Test.TextSec
{
    [TestClass]
    public class TextSectionServiceTest
    {
        private readonly TextSectionService _textSectionService;
        public TextSectionServiceTest() 
        {
            var provider = new TestingServiceProvider();
            _textSectionService = provider.Get<TextSectionService>();
        } 
        [TestMethod]
        [DataRow(101, "你好，很高兴认识你", "你好，很高兴认识你", 30, 100)]
        [DataRow(102, "你好，很**高兴**认识你", "你好，很高兴认识你", 30, 100)]
        [DataRow(103, "你好，很高兴认识你", "你好，很高...", 8, 100)]
        [DataRow(104, "你好，很**高兴**认识你", "你好，很高...", 8, 100)]
        [DataRow(105, "你好 \n 很高兴认识你", "你好 很高兴认识你", 30, 100)]
        [DataRow(106, "你好 \n 很高兴认识你", "你好 很高...", 8, 100)]
        public void Briefing(int tId, string content, string expectBrief, int briefLength, int parseLength)
        {
            var res = _textSectionService.Brief(tId, content, briefLength, parseLength);
            Assert.AreEqual(expectBrief, res);
        }
    }
}
