using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Wiki
{
    public class WikiParaRepo : RepoBase<WikiPara>
    {
        public WikiParaRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }

        public IQueryable<WikiPara> WithType(WikiParaType type) => Existing.Where(x => x.Type == type);

        public bool SetFileParaFileId(int paraId, int fileId, out string? errmsg)
        {
            var p = Existing.Where(x => x.Id == paraId).FirstOrDefault();
            if (p is null)
            {
                errmsg = "找不到指定段落";
                return false;
            }
            if (p.Type != WikiParaType.File)
            {
                errmsg = "段落类型异常(不是文件)";
                return false;
            }
            p.ObjectId = fileId;
            return TryEdit(p, out errmsg);
        }

        public IQueryable<int> WikiContainingIt(WikiParaType type, int objId)
        {
            return Existing.Where(x => x.Type == type && x.ObjectId == objId).Select(x => x.WikiItemId);
        }
    }
}
