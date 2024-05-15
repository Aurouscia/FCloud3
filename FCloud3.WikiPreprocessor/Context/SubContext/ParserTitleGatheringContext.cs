using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Context.SubContext
{
    public class ParserTitleGatheringContext
    {
        private readonly Random _random;
        public ParserTitleGatheringContext() 
        {
            _random = new();
        }
        public int GenerateTitleId()
        {
            return _random.Next(1000000,9999999);
        }
    }
    public class ParserTitleTreeNode
    {
        public string Text { get; }
        public int Id { get; }
        public int Level { get; }
        public List<ParserTitleTreeNode>? Subs { get; set; }
        public ParserTitleTreeNode(int level, string text, int id)
        {
            Level = level;
            Text = text;
            Id = id;
        }
        public void Insert(ParserTitleTreeNode newNode)
        {
            if (newNode.Level <= this.Level)
                return;
            this.Subs ??= new();
            var last = this.Subs.LastOrDefault();
            if(last is null || last.Level == newNode.Level)
            {
                this.Subs.Add(newNode);
            }
            else
            {
                last.Insert(newNode);
            }
        }
    }
}
