using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc;
using Microsoft.EntityFrameworkCore;
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

        public IQueryable<WikiPara> GetParasByWikiId(int wikiId)
        {
            return Existing.Where(x => x.WikiItemId == wikiId);
        }
        
        public List<WikiPara> ParaContainingThem(List<(WikiParaType type, int objId)> info)
        {
            var objIds = info.ConvertAll(x => x.objId);
            var candidates = Existing.Where(x => objIds.Contains(x.ObjectId)).ToList();
            return candidates.FindAll(c =>
                info.Any(i => i.type == c.Type && i.objId == c.ObjectId));
        }

        public bool SetInfo(int id, string? nameOverride, out string? errmsg)
        {
            if(nameOverride is not null && nameOverride.Length > WikiPara.nameOverrideMaxLength)
            {
                errmsg = "名称过长";
                return false;
            }
            int affected = Existing.Where(x => x.Id == id)
                .ExecuteUpdate(x => x.SetProperty(p => p.NameOverride, nameOverride));
            if (affected == 0)
            {
                errmsg = "找不到指定段落";
                return false;
            }
            errmsg = null;
            return true;
        }
    }
}
