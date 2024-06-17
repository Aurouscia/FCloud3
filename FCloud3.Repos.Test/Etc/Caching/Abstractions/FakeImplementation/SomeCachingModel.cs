using FCloud3.Entities;
using FCloud3.Repos.Etc.Caching.Abstraction;

namespace FCloud3.Repos.Test.Etc.Caching.Abstractions.FakeImplementation
{
    public class SomeCachingModel : CachingModelBase<SomeDbModel>
    {
        public string Name { get; set; }
        public int SomeProp { get; set; }
        public SomeCachingModel()
        {
            Name = "";
        }
        public SomeCachingModel(SomeDbModel model)
        {
            Id = model.Id;
            Name = model.Name;
            SomeProp = model.SomeProp;
        }
    }

    public class SomeCachingModelEqualityComparer : IEqualityComparer<SomeCachingModel>
    {
        public bool Equals(SomeCachingModel? x, SomeCachingModel? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id && x.Name == y.Name && x.SomeProp == y.SomeProp;
        }
        public int GetHashCode(SomeCachingModel obj)
        {
            return HashCode.Combine(obj.Id, obj.Name, obj.SomeProp);
        }
    }
    public class SomeDbModel:IDbModel
    {
        public SomeDbModel(int id, string name, int someProp, int creatorUserId)
        {
            Id = id;
            Name = name;
            SomeProp = someProp;
            CreatorUserId = creatorUserId;
            Created = DateTime.Now;
            Updated = DateTime.Now;
            Deleted = false;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int SomeProp { get; set; }
        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
}