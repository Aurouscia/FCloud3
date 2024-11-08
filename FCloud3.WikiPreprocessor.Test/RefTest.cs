using FCloud3.WikiPreprocessor.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class RefTest
    {
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Refs(bool keep)
        {
            var pb = new ParserBuilder();
            pb = keep ? pb.KeepRefBeforeCalling() : pb;
            var p = pb.BuildParser();
            var text1 = "ABC[DE]，123[456](789 )，X{Y}Z，{{M}2233}";
            _ = p.RunToParserResult(text1);
            var refs1 = p.Context.Ref.Refs.ToList();
            CollectionAssert.AreEquivalent(
                new List<string>() { "DE", "789", "Y", "M" },
                refs1);

            var text2 = "ABC[KK]，123[456](789)";
            _ = p.RunToParserResult(text2);
            var refs2 = p.Context.Ref.Refs.ToList();
            if (keep)
                CollectionAssert.AreEquivalent(
                    new List<string>() { "DE", "789", "Y", "M", "KK" },
                    refs2);
            else
                CollectionAssert.AreEquivalent(
                    new List<string>() { "KK", "789" },
                    refs2
                    );
        }
    }
}
