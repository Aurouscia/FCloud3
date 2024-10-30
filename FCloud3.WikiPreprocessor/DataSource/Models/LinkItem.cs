using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.DataSource.Models
{
    public class LinkItem(string text, string url)
    {
        public string Text { get; } = text;
        public string Url { get; } = url;
    }
}
