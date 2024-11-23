using Aliyun.OSS;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files.Storage.Abstractions;
using Microsoft.Extensions.Configuration;

namespace FCloud3.Services.Etc.Split
{
    public class SplitService(
        WikiItemRepo wikiItemRepo,
        WikiToDirRepo wikiToDirRepo,
        FileDirRepo fileDirRepo,
        UserRepo userRepo,
        FileItemRepo fileItemRepo,
        MaterialRepo materialRepo,
        //IStorage storage,
        IConfiguration config)
    {
        public List<AllWikiInDirReportItem> AllWikiInDirReport(int dirId)
        {
            List<WikiToDir> wikiToDirs = wikiToDirRepo.Existing.ToList();
            var allWInfo = AllWikiInDir(wikiToDirs, dirId);
            var wIdsList = allWInfo.wIds.ToList();
            var us = userRepo.Existing.Select(x => new { x.Id, x.Name }).ToList();
            var ws = wikiItemRepo
                .GetRangeByIds(wIdsList)
                .Select(x => new { x.Id, x.Title, x.OwnerUserId }).ToList();
            var res = ws.ConvertAll(w =>
            {
                var title = w.Title ?? "??";
                var owner = us.Find(u => u.Id == w.OwnerUserId)?.Name ?? "??";
                var existsInOtherDir = wikiToDirs
                    .Where(x => x.WikiId == w.Id)
                    .Select(x => x.DirId)
                    .Except(allWInfo.dirIds).Any();
                return new AllWikiInDirReportItem(w.Id, title, owner, existsInOtherDir);
            });
            return res;
        }
        private (HashSet<int> wIds, HashSet<int> dirIds) AllWikiInDir(List<WikiToDir> wikiToDirs, int dirId)
        {
            List<SimpleDir> dirs = fileDirRepo.Existing
                .Select(x => new SimpleDir(x.Id, x.ParentDir)).ToList();
            HashSet<int> wikiIds = [];
            HashSet<int> dirIds = [];
            AllWikiInDir(wikiIds, dirIds, wikiToDirs, dirs, dirId);
            return (wikiIds, dirIds);
        }
        private void AllWikiInDir(HashSet<int> wIds, HashSet<int> dIds,
            List<WikiToDir> wikiToDirs, List<SimpleDir> dirs, int dirId)
        {
            var wHere = wikiToDirs
                .Where(x => x.DirId == dirId)
                .Select(x => x.WikiId);
            foreach (var w in wHere)
                wIds.Add(w);
            dIds.Add(dirId);
            
            var subdirIds = dirs
                .Where(x => x.ParentId == dirId)
                .Select(x => x.Id);
            foreach (var subdirId in subdirIds)
            {
                AllWikiInDir(wIds, dIds, wikiToDirs, dirs, subdirId);
            }
        }
        public List<int> DirDescendants(int dirId)
        {
            List<WikiToDir> wikiToDirs = wikiToDirRepo.Existing.ToList();
            var (_, dirIds) = AllWikiInDir(wikiToDirs, dirId);
            return dirIds.ToList();
        }
        private readonly struct SimpleDir(int id, int parentId)
        {
            public int Id { get; } = id;
            public int ParentId { get; } = parentId;
        }

        public int CopyAllFilesToAnotherBucket()
        {
            var distBucketName = config["FileMoveToOss:BucketName"];
            var configSec = config.GetSection("FileStorage:Oss");
            var endPoint = configSec["EndPoint"];
            var bucketName = configSec["BucketName"];
            var accessKeyId = configSec["AccessKeyId"];
            var accessKeySecret = configSec["AccessKeySecret"];
            var client = new OssClient(endPoint, accessKeyId, accessKeySecret);
            var allFileFiles = fileItemRepo.Existing.Select(x => x.StorePathName).ToList();
            var allMatFiles = materialRepo.Existing.Select(x => x.StorePathName).ToList();
            var allKeys = allFileFiles.Union(allMatFiles).ToList();
            var existInDist = GetAllFilesInBucket(client, distBucketName!);
            int copied = 0;
            foreach (var key in allKeys)
            {
                if (key is null || existInDist.Contains(key))
                    continue;
                var copyReq = new CopyObjectRequest(bucketName, key, distBucketName, key);
                try
                {
                    client.CopyObject(copyReq);
                    copied++;
                }
                catch { }
            }
            return allKeys.Count;
        }
        public List<string> GetAllFilesInBucket(OssClient client, string bucketName)
        {
            List<string> keys = [];
            ObjectListing result;
            string nextMarker = string.Empty;
            do
            {
                // 每页列举的文件个数通过MaxKeys指定，超出指定数量的文件将分页显示。
                var listObjectsRequest = new ListObjectsRequest(bucketName)
                {
                    Marker = nextMarker,
                    MaxKeys = 100
                };
                result = client.ListObjects(listObjectsRequest);
                foreach (var summary in result.ObjectSummaries)
                {
                    keys.Add(summary.Key);
                }
                nextMarker = result.NextMarker;
            } while (result.IsTruncated);
            return keys;
        }
    }

    public class AllWikiInDirReportItem(int id, string title, string owner, bool existsInOtherDir)
    {
        public int Id { get; } = id;
        public string Title { get; set; } = title;
        public string Owner { get; set; } = owner;
        public bool ExistsInOtherDir { get; set; } = existsInOtherDir;
    }
}