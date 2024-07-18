using FCloud3.Repos.Files;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Repos.Etc.Caching;

namespace FCloud3.Services.Etc
{
    public class LatestWorkService(
        WikiItemRepo wikiItemRepo,
        FileItemRepo fileItemRepo,
        UserCaching userCaching,
        IStorage storage)
    {
        private readonly WikiItemRepo _wikiItemRepo = wikiItemRepo;
        private readonly FileItemRepo _fileItemRepo = fileItemRepo;
        private readonly UserCaching _userCaching = userCaching;
        private readonly IStorage _storage = storage;

        public List<LatestWorkViewItem> Get(int uid = -1)
        {
            var wikis = _wikiItemRepo.OwnedByUser(uid).OrderByDescending(x => x.Updated).Take(20).ToList();
            var files = _fileItemRepo.OwnedByUser(uid).OrderByDescending(x => x.Created).Take(20).ToList();
            var relatedUserIds =
                wikis.ConvertAll(x => x.OwnerUserId)
                .Union(files.ConvertAll(x => x.CreatorUserId)).ToList();
            var users = _userCaching.GetRange(relatedUserIds);
            string uname(int uid) => users.Find(x => x.Id == uid)?.Name ?? "??";

            List<LatestWorkViewItem> res = [];
            wikis.ForEach(w =>
            {
                res.Add(new(
                    type: LatestWorkType.Wiki, 
                    title: w.Title ?? "??",
                    jumpParam: w.UrlPathName ?? "??", 
                    objId: w.Id,
                    userId: w.OwnerUserId,
                    userName: uname(w.OwnerUserId),
                    time: w.Updated));
            });
            files.ForEach(f =>
            {
                res.Add(new(
                    type: LatestWorkType.File,
                    title: f.DisplayName ?? "??",
                    jumpParam: "",
                    objId: f.Id,
                    userId: f.CreatorUserId,
                    userName: uname(f.CreatorUserId),
                    time: f.Created));
            });
            res.Sort((x, y) => DateTime.Compare(y.GetTime(), x.GetTime()));
            return res;
        }

        public class LatestWorkViewItem(
            LatestWorkType type,
            string title,
            string jumpParam,
            int objId,
            int userId,
            string userName,
            DateTime time)
        {
            public LatestWorkType Type { get; set; } = type;
            public string Title { get; set; } = title;
            public string JumpParam { get; set; } = jumpParam;
            public int ObjId { get; set; } = objId;
            public int UserId { get; set; } = userId;
            public string UserName { get; set; } = userName;
            public string Time { get; set; } = time.ToString("MM-dd HH:mm");

            private readonly DateTime time = time;
            public DateTime GetTime() => time;
        }
        public enum LatestWorkType
        {
            None = 0,
            Wiki = 1,
            File = 2
        }
    }
}
