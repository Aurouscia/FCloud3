using FCloud3.Entities.Sys;
using FCloud3.Repos.Etc;
using FCloud3.Repos.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Test.Base.FakeImplementation
{
    internal class SomeClassRepo(
        FCloudContextWithSomeClass contextWithSomeClass,
        LastUpdateRepo lastUpdateRepo,
        ICommitingUserIdProvider userIdProvider) 
        : RepoBaseCache<SomeClass, SomeClassCacheModel>(contextWithSomeClass, lastUpdateRepo, userIdProvider)
    {
        public new void Add(SomeClass item) => base.Add(item);
        public new void Update(SomeClass item) => base.Update(item);
        public new void Remove(int id) => base.Remove(id); 
        protected override IQueryable<SomeClassCacheModel> ConvertToCacheModel(IQueryable<SomeClass> q)
        {
            return q.Select(x => new SomeClassCacheModel(x.Id, x.Updated, x.Number1));
        }
        protected override LastUpdateType GetLastUpdateType()
        {
            return (LastUpdateType)100;
        }
    }

    internal class SomeClassCacheModel(int id, DateTime updated, int num1)
        : CacheModelBase<SomeClass>(id, updated)
    {
        public int Number1 { get; set; } = num1;  
    }
}
