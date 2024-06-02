using FCloud3.DbContexts;
using FCloud3.Repos.Etc.Caching.Abstraction;
using Microsoft.Extensions.Logging;

namespace FCloud3.Repos.Test.Etc.Caching.Abstractions.FakeImplementation
{
    [TestClass]
    public class SomeCaching : CachingBase<SomeCachingModel, SomeDbModel>
    {
        public SomeCaching(
            IQueryable<SomeDbModel> source,
            ILogger<CachingBase<SomeCachingModel, SomeDbModel>> logger) 
            : base(source, logger)
        {
        }

        public void Insert(SomeDbModel model) => base.Insert(model);
        public void InsertRange(List<SomeDbModel> models) => base.InsertRange(models);
        protected override IQueryable<SomeCachingModel> GetFromDbModel(IQueryable<SomeDbModel> dbModels)
        {
            return dbModels.Select(x => new SomeCachingModel()
            {
                Id = x.Id,
                Name = x.Name,
                SomeProp = x.SomeProp
            });
        }
        protected override SomeCachingModel GetFromDbModel(SomeDbModel model)
        {
            return new SomeCachingModel()
            {
                Id = model.Id,
                Name = model.Name,
                SomeProp = model.SomeProp
            };
        }
        protected override void MutateByDbModel(SomeCachingModel target, SomeDbModel from)
        {
            target.Id = from.Id;
            target.Name = from.Name;
            target.SomeProp = from.SomeProp;
        }
        public List<SomeCachingModel> GetDataList() => base.TestingOnlyGetDataList();
    }
}