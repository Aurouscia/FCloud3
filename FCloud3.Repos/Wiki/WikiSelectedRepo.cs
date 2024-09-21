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
                    removeItems.ForEach(w =>
                    {
                        w.Deleted = true;
                        _context.Update(w);
                    });
                }
            }
            for (int i = 0; i < ordered.Count; i++)
            {
                ordered[i].Order = i;
                _context.Update(ordered[i]);
            }
            _context.SaveChanges();
            return true;
        }

        public bool Edit(int id, string? intro, int dropAfterHr, out string? errmsg)
        {
            var model = GetById(id);
            if (model is null)
            {
                errmsg = "找不到指定数据";
                return false;
            }
            model.Intro = intro;
            model.DropAfterHr = dropAfterHr;
            return TryEdit(model, out errmsg);
        }
        public bool Remove(int id, out string? errmsg)
        {
            var model = GetById(id);
            if (model is null)
            {
                errmsg = "找不到指定数据";
                return false;
            }
            return TryRemove(model, out errmsg);
        }

        public override bool TryAddCheck(WikiSelected item, out string? errmsg)
        {
            errmsg = ModelCheck(item);
            if (errmsg is not null)
                return false;
            if (Existing.Any(x => x.WikiItemId == item.WikiItemId))
            {
                errmsg = "已添加过该词条";
                return false;
            }
            return true;
        }

        public override bool TryEditCheck(WikiSelected item, out string? errmsg)
        {
            errmsg = ModelCheck(item);
            return errmsg is null;
        }

        public string? ModelCheck(WikiSelected model)
        {
            if (string.IsNullOrWhiteSpace(model.Intro))
                return "简介不能为空";
            if (model.Intro.Length > WikiSelected.introMaxLength)
                return $"简介长度不能超过{WikiSelected.introMaxLength}";
            return null;
        }
    }
}