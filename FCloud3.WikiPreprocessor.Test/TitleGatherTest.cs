using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCloud3.WikiPreprocessor.Test.Support;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class TitleGatherTest
    {
        [TestMethod]
        public void Gather()
        {
            ParserBuilder builder = new();
            builder.TitleGathering.Enable();
            builder.Cache.UseCacheInstance(CacheInstance.Get());
            Parser p = builder.BuildParser();
            string input = "#1 \n xxx \n ##1-1 \n xxx \n #2\n xx \n#";

            for (int i = 0; i < 2; i++)
            {
                var res = p.RunToParserResultRaw(input);

                var root = res.Titles;
                Assert.IsNotNull(root);
                Assert.AreEqual(2, root.Count);

                var t0 = root[0];
                Assert.AreEqual("1", t0.Text);
                Assert.IsNotNull(t0.Subs);
                Assert.AreEqual(1, t0.Subs.Count);
                Assert.IsTrue(res.Content.Contains(t0.Id.ToString()));

                var t00 = t0.Subs[0];
                Assert.AreEqual("1-1", t00.Text);
                Assert.IsNull(t00.Subs);
                Assert.IsTrue(res.Content.Contains(t00.Id.ToString()));

                var t1 = root[1];
                Assert.AreEqual("2", t1.Text);
                Assert.IsNull(t1.Subs);
                Assert.IsTrue(res.Content.Contains(t1.Id.ToString()));
            }
        }
    }
}
