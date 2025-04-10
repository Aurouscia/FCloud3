using Aurouscia.TableEditor.Core.Excel;

namespace FCloud3.Services
{
    public static class CommonOptions
    {
        public static readonly AuTableExcelConverterOptions excelConvert = new() 
        { 
            RemoveTrailingEmptyColumns = true,
            RemoveTrailingEmptyRows = true,
            RowCountLimit = 1000,
            ColumnCountLimit = 30,
            CharCountLimit = 60000
        };
    }
}
