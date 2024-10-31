using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCloud3.WikiPreprocessor.Test.Support;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class RuleGatherTest
    {
        [TestMethod]
        [DataRow(
            "123*456*789",
            "*")]
        [DataRow(
            "12**34*5*67**89",
            "**;*")]
        [DataRow(
            "> 123*456*789\n123**456**789",
            "**;*")]
        [DataRow(
            "|12*34*56|78\\bd90\\bd00|\n",
            "*;\\bd")]
        public void Inline(string input,string answerStr)
        {
            var parser = new ParserBuilder()
                .BuildParser();
            var element = parser.RunToObject(input);
            var rules = element.ContainRules()??new();
            var inlineRules = rules.ConvertAll(x=>
            {
                if(x is IInlineRule ir)
                    return ir.MarkLeft;
                return null;
            });
            inlineRules.RemoveAll(x => x is null);
            var answers = answerStr.Split(';').ToList();
            CollectionAssert.AreEquivalent(answers,inlineRules);
        }
    }
}
