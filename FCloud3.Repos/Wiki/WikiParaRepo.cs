﻿using FCloud3.DbContexts;
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

        public new int AddAndGetId(WikiPara para) => base.AddAndGetId(para);
        public void Update(WikiPara para) => base.Update(para);
        public void UpdateRange(List<WikiPara> paras) => base.UpdateRange(paras);
        public void Remove(WikiPara para) => base.Remove(para);

        public bool SetParaObjId(int paraId, WikiParaType shouldBeType, int objId, out string? errmsg)
        {
            var p = Existing.Where(x => x.Id == paraId).FirstOrDefault();
            if (p is null)
            {
                errmsg = "找不到指定段落";
                return false;
            }
            if (p.Type != shouldBeType)
            {
                errmsg = "未知错误，段落类型检查出错";
                return false;
            }
            p.ObjectId = objId;
            base.Update(p);
            errmsg = null;
            return true;
        }

        public IQueryable<int> WikiContainingIt(WikiParaType type, int objId)
        {
            return Existing.Where(x => x.Type == type && x.ObjectId == objId).Select(x => x.WikiItemId);
        }
        public HashSet<int> WikisContainingThem(List<(WikiParaType Type, int ObjectId)> paras)
        {
            var allObjIds = paras.Select(x => x.ObjectId);
            var suspicious = Existing
                .Where(x => allObjIds.Contains(x.ObjectId))
                .Select(x => new {x.ObjectId, x.Type, x.WikiItemId})
                .ToList();
            var res = new HashSet<int>();
            foreach(var p in paras)
            {
                var wId = suspicious.Find(x => x.Type == p.Type && x.ObjectId == p.ObjectId)?.WikiItemId;
                if(wId is int wikiId)
                {
                    res.Add(wikiId);
                }
            }
            return res;
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
                .ExecuteUpdate(x => x
                    .SetProperty(p => p.NameOverride, nameOverride)
                    .SetProperty(p => p.Updated, DateTime.Now));
            AfterDataChange();
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
