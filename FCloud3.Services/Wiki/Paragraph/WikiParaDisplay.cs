using FCloud3.Entities.Wiki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Wiki.Paragraph
{
    public class WikiParaDisplay
    {
        /// <summary>
        /// 指该段落Id
        /// </summary>
        public int ParaId { get; set; }
        /// <summary>
        /// 指该段落代表的文本段/文件/表格的Id
        /// </summary>
        public int UnderlyingId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? NameOverride { get; set; }
        public int Order { get; set; }
        public WikiParaType Type { get; set; }
        public int Bytes { get; set; }
        public WikiParaDisplay(WikiPara para, int underlyingId, string? title, string? content, string? nameOverride, WikiParaType type, int bytes)
        {
            ParaId = para.Id;
            Order = para.Order;
            UnderlyingId = underlyingId;
            Title = title;
            Content = content;
            NameOverride = nameOverride;
            Type = type;
            Bytes = bytes;
        }
    }
    public class WikiParaPlaceholder
    {
        public WikiParaType Type { get; set; }
        private string _title;
        public WikiParaPlaceholder(WikiParaType type)
        {
            Type = type;
            if (type == WikiParaType.Text)
                _title = "空文本段落";
            else if (type == WikiParaType.File)
                _title = "空文件段落";
            else if (type == WikiParaType.Table)
                _title = "空表格段落";
            else
                _title = "???";
        }

        public WikiParaDisplay ToDisplay(WikiPara para)
        {
            return new WikiParaDisplay(para, 0, _title,"" ,null , Type, 0);
        }
    }
}
