using Aurouscia.TableEditor.Core;
using Aurouscia.TableEditor.Core.Utils;
using FCloud3.DbContexts;
using FCloud3.Entities.Table;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FCloud3.Repos.Table
{
    public class FreeTableRepo:RepoBase<FreeTable>
    {
        private const int briefCellMaxStrLength = 12;
        private const string newTableDefaultName = "新建表格";
        public FreeTableRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }

        public List<FreeTableMeta> GetMetaRangeByIds(List<int> ids)
        {
            if (ids.Count == 0)
                return new();
            return base.GetRangeByIds(ids).GetMetaData().ToList();
        }
        public bool TryEditInfo(int id, string name, out string? errmsg)
        {
            if (!NameCheck(name, out errmsg)) {
                return false;
            }
            int affected = Existing.Where(x => x.Id == id)
                .ExecuteUpdate(x => x
                    .SetProperty(t => t.Name, name)
                    .SetProperty(t => t.Updated, DateTime.Now)
                );
            return affected == 1;
        }

        public bool TryEditContent(FreeTable model, AuTable data, string dataRaw, out string? errmsg)
        {
            var brief = Brief(data.Cells);
            string briefJson = brief.ToJsonStr();
            if (model is null)
            {
                errmsg = "找不到指定表格";
                return false;
            }
            model.Updated = DateTime.Now;
            model.Brief = briefJson;
            model.Data = dataRaw;
            _context.Update(model);
            _context.SaveChanges();
            errmsg = null;
            return true;
        }
        private FreeTableBrief Brief(List<List<string?>?>? cells)
        {
            FreeTableBrief brief = new();
            if(cells is null)
            {
                return DefaultBrief();
            }
            for (int r = 0; r < cells.Count && r < 3; r++)
            {
                var row = cells[r];
                if (row is null) continue;
                var briefRow = new List<string>();
                brief.Add(briefRow);
                for (int c = 0; c < row.Count && r < 4; c++)
                {
                    string cellVal = row[c] ?? "";
                    if (cellVal.Length > briefCellMaxStrLength)
                        cellVal = string.Concat(cellVal.AsSpan(0, briefCellMaxStrLength - 1), "...");
                    briefRow.Add(cellVal);
                }
            }
            return brief;
        }
        private static List<List<string?>?> DefaultCells()
        {
            return new List<List<string?>?>()
                {
                    new List<string?>(){"",""},
                    new List<string?>(){"",""}
                };
        }
        private static FreeTableBrief DefaultBrief()
        {
            return new()
                {
                    new List<string>(){"",""},
                    new List<string>(){"",""}
                };
        }

        public override int TryCreateDefaultAndGetId(out string? errmsg)
        {
            AuTable table = new()
            {
                Name = newTableDefaultName,
                Cells = DefaultCells(),
                Merges = new()
            };
            var brief = DefaultBrief();
            FreeTable creating = new();
            creating.SetData(table);
            creating.SetBrief(brief);
            creating.Name = newTableDefaultName;
            return TryAddAndGetId(creating, out errmsg);
        }


        private static bool NameCheck(string name, out string? errmsg)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                errmsg = "表格名不能为空";
                return false;
            }
            if (name.Length > 25)
            {
                errmsg = "表格名称过长，请缩短";
                return false;
            }
            errmsg = null;
            return true;
        }
    }


    public class FreeTableMeta
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Brief { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
    public static class FreeTableMetaQuerier
    {
        public static IQueryable<FreeTableMeta> GetMetaData(this IQueryable<FreeTable> freeTables)
        {
            return freeTables.Select(data => new FreeTableMeta()
            {
                Id = data.Id,
                Name = data.Name,
                Brief = data.Brief,
                CreatorUserId = data.CreatorUserId,
                Created = data.Created,
                Updated = data.Updated,
                Deleted = data.Deleted,
            });
        }
    }

    public class FreeTableBrief: List<List<string>>
    {
    }

    public static class FreeTableDataConvert
    {
        public static AuTable Deserialize(string? data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return new();
            return JsonConvert.DeserializeObject<AuTable>(data, jsonSettings) ?? new();
        }
        public static AuTable GetData(this FreeTable table) 
        {
            return Deserialize(table.Data);
        }
        public static void SetData(this FreeTable table, AuTable tableData)
        {
            string dataStr = JsonConvert.SerializeObject(tableData, jsonSettings);
            table.Data = dataStr;
        }
        public static void SetBrief(this FreeTable table, FreeTableBrief briefData)
        {
            string briefStr = briefData.ToJsonStr();
            table.Brief = briefStr;
        }
        public static string ToJsonStr(this FreeTableBrief briefData)
        {
            return JsonConvert.SerializeObject(briefData, jsonSettings);
        }

        private static JsonSerializerSettings jsonSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };
    }
}
