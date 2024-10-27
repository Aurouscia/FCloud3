using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc;
using Microsoft.Extensions.Configuration;

namespace FCloud3.Repos.Wiki
{
    public class WikiSelectedRepo(
        FCloudContext context,
        ICommitingUserIdProvider userIdProvider,
        IConfiguration config) 
        :RepoBase<WikiSelected>(context, userIdProvider)
    {
        private int MaxCount => MaxCountConfig.Value;
        private Lazy<int> MaxCountConfig = new(() =>
            {
                var configVal = config["WikiSelected:MaxCount"];
                if (int.TryParse(configVal, out int val))
                    return val;
                return 8;
            });
        
        public bool Insert(int beforeOrder,
            int wikiItemId, string? intro, int dropAfterHr, out string? errmsg)
        {
            var all = Existing.ToList();
            var model = new WikiSelected()
            {
                WikiItemId = wikiItemId,
                Intro = intro,
                DropAfterHr = dropAfterHr
            };
            TryAdd(model, out errmsg);
            if (errmsg is { })
                return false;
            var lessOrder = all.FindAll(x => x.Order < beforeOrder);
            var biggerOrder = all.Except(lessOrder);
            List<WikiSelected> ordered = [..lessOrder];
            ordered.Add(model);
            ordered.AddRange(biggerOrder);
            var needUpdate = new List<WikiSelected>();
            
            if (ordered.Count > MaxCount)
            {
                int exceeded = ordered.Count - MaxCount;
                List<(double overHr, WikiSelected item)> removeList = [];
                var now = DateTime.Now;
                foreach(var w in ordered)
                {
                    var lasted = (now - w.Created).TotalHours;
                    if (lasted > w.DropAfterHr)
                    {
                        double overHr = lasted - w.DropAfterHr;
                        removeList.Add((overHr, w));
                        if (removeList.Count >= exceeded)
                            break;
                    }
                }
                var removeItems = removeList
                    .OrderByDescending(x => x.overHr)
                    .Select(x => x.item)
                    .Take(exceeded)
                    .ToList();
                if (removeList.Count > 0)
                {
                    ordered.RemoveAll(removeItems.Contains);
                    base.RemoveRange(removeItems);
                }
            }
            for (int i = 0; i < ordered.Count; i++)
            {
                ordered[i].Order = i;
            }
            base.UpdateRange(ordered);
            return true;
        }

        public bool TryEdit(int id, string? intro, int dropAfterHr, out string? errmsg)
        {
            var model = GetById(id);
            if (model is null)
            {
                errmsg = "找不到指定数据";
                return false;
            }
            model.Intro = intro;
            model.DropAfterHr = dropAfterHr;
            errmsg = ModelCheck(model);
            if (errmsg is not null)
                return false;
            base.Update(model);
            return true;
        }
        public bool TryRemove(int id, out string? errmsg)
        {
            var model = GetById(id);
            if (model is null)
            {
                errmsg = "找不到指定数据";
                return false;
            }
            base.Remove(model);
            errmsg = null;
            return true;
        }

        public bool TryAdd(WikiSelected item, out string? errmsg)
        {
            errmsg = ModelCheck(item);
            if (errmsg is not null)
                return false;
            if (Existing.Any(x => x.WikiItemId == item.WikiItemId))
            {
                errmsg = "已添加过该词条";
                return false;
            }
            base.Add(item);
            return true;
        }

        private string? ModelCheck(WikiSelected model)
        {
            if (model.WikiItemId == 0)
                return "请选择词条";
            if (string.IsNullOrWhiteSpace(model.Intro))
                return "简介不能为空";
            if (model.Intro.Length > WikiSelected.introMaxLength)
                return $"简介长度不能超过{WikiSelected.introMaxLength}";
            return null;
        }
    }
}