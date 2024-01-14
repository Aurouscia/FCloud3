namespace FCloud3.Entities.Wiki.Paragraph
{
    public interface IWikiParaObject
    {
        public WikiParaDisplay ToDisplay(WikiPara para);
        public WikiParaDisplay ToDisplaySimple(WikiPara para);
    }
}
