using FCloud3.Entities.Files;
using FCloud3.Repos;
using FCloud3.Repos.Files;

namespace FCloud3.Services.Files
{
    public class FileDirService
    {
        private readonly FileDirRepo _fileDirRepo;

        public FileDirService(FileDirRepo fileDirRepo)
        {
            _fileDirRepo = fileDirRepo;
        }

        public FileDirIndexResult? GetListByPath(IndexQuery q, string[] path, out string? errmsg)
        {
            var children = _fileDirRepo.GetChildrenByPath(path,out int thisDirId ,out errmsg);
            if(children is null)
                return null;
            var dirs = _fileDirRepo.IndexFilterOrder(children,q);
            var res = dirs.TakePage(q,x=>new SubDir()
            {
                Id = x.Id,
                Name = x.Name,
                Update = x.Updated.ToString("yy/MM/dd HH:mm")
            });
            return new() { 
                Data = res,
                ThisDirId = thisDirId
            };
        }

        public FileDirTakeContentResult TakeContent(int dirId)
        {
            var q = _fileDirRepo.Existing
                .Where(x => x.ParentDir == dirId)
                .Select(x => new { x.Id, x.Name })
                .OrderBy(x => x.Name).ToList();
            FileDirTakeContentResult res = new();
            q.ForEach(x =>
            {
                res.SubDirs.Add(new() { Id = x.Id, Name = x.Name });
            });
            return res;
        }
    }

    public class SubDir
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Update { get; set; }
        public int OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public int ByteCount { get; set; }
        public int FileNumber { get; set; }
    }
    public class FileDirIndexResult
    {
        public IndexResult<SubDir>? Data { get; set; }
        public int ThisDirId { get; set; }
    }
    public class FileDirTakeContentResult
    {
        public List<TakeContentResSubDir> SubDirs { get; set; }
        public class TakeContentResSubDir
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }
        public FileDirTakeContentResult()
        {
            SubDirs = new();
        }
    }
}
