using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGenTest
{
    [TestClass]
    public class TitleGatherTest
    {
        [TestMethod]
        public void Gather()
        {
            ParserBuilder builder = new();
            builder.TitleGathering.Enable();
            Parser p = builder.BuildParser();
            string input = "#1 \n xxx \n ##1-1 \n xxx \n #2\n xx \n#";
            var res = p.RunToPlain(input);

            var root = p.Context.TitleGathering.Root;
            Assert.IsNotNull(root.Subs);
            Assert.AreEqual(2, root.Subs.Count);

            var t0 = root.Subs[0];
            Assert.AreEqual("1", t0.Text);
            Assert.IsNotNull(t0.Subs);
            Assert.AreEqual(1, t0.Subs.Count);
            Assert.IsTrue(res.Contains(t0.Id.ToString()));

            var t00 = t0.Subs[0];
            Assert.AreEqual("1-1", t00.Text);
            Assert.IsNull(t00.Subs);
            Assert.IsTrue(res.Contains(t00.Id.ToString()));

            var t1 = root.Subs[1];
            Assert.AreEqual("2", t1.Text);
            Assert.IsNull(t1.Subs);
            Assert.IsTrue(res.Contains(t1.Id.ToString()));
        }
    }
}
