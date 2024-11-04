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
        public void Refs()
        {
            var p = new ParserBuilder().BuildParser();
            var text = "ABC[DE]，123[456](789 )，X{Y}Z，{{M}2233}";
            _ = p.RunToParserResult(text);
            var refs = p.Context.Ref.GetRefs().ToList();
            CollectionAssert.AreEquivalent(
                new List<string>() { "DE", "789", "Y", "M" },
                refs);
        }
    }
}
