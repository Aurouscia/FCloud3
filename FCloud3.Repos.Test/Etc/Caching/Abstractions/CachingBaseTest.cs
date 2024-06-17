using FCloud3.Repos.Etc.Caching.Abstraction;
using FCloud3.Repos.Test.Etc.Caching.Abstractions.FakeImplementation;
using FCloud3.Repos.Test.TestSupport;
using Newtonsoft.Json;

namespace FCloud3.Repos.Test.Etc.Caching.Abstractions
{
    [TestClass]
    public class CachingBaseTest
    {
        private readonly SomeCaching _caching;
        private readonly SomeCaching _anotherCaching;
        private readonly List<SomeDbModel> _fakeDb;
        private readonly SomeCachingModelEqualityComparer _comparer;
        private readonly SomeDbModel _item6 = new(6, "FFF", 43, 2);
        private readonly SomeDbModel _item7 = new(7, "GGG", 15, 2);
        public CachingBaseTest()
        {
            _fakeDb =
            [
                new SomeDbModel(1, "AAA", 99, 1),
                new SomeDbModel(2, "BBB", 72, 1),
                new SomeDbModel(3, "CCC", 64, 1),
                new SomeDbModel(4, "DDD", 58, 2),
                new SomeDbModel(5, "EEE", 21, 2),
            ];
            var fakeSource = _fakeDb.AsQueryable();
            var fakeLogger = new FakeLogger<SomeCaching>();
            _caching = new SomeCaching(fakeSource, fakeLogger);
            _anotherCaching = new SomeCaching(fakeSource, fakeLogger);
            _caching.Clear();
            _comparer = new SomeCachingModelEqualityComparer();
        }
        private void IsSameList(List<int> ids, List<SomeCachingModel> res)
        {
            var expectedModels = _fakeDb
                .FindAll(x => ids.Contains(x.Id))
                .ConvertAll(x => new SomeCachingModel(x));
            expectedModels.Sort((x,y) => x.Id - y.Id);
            var expectedJson = JsonConvert.SerializeObject(expectedModels);
            res.Sort((x,y) => x.Id - y.Id);
            var actualJson = JsonConvert.SerializeObject(res);
            Assert.AreEqual(expectedJson, actualJson);
        }

        [TestMethod]
        [DataRow(1)][DataRow(2)][DataRow(3)]
        public void QuerySingle(int id)
        {
            var expectedModel = new SomeCachingModel(_fakeDb.Find(x => x.Id == id)!);
            var res1 = _caching.Get(id);
            Assert.AreEqual(1, _caching.QueriedRows);
            Assert.AreEqual(1, _caching.QueriedTimes);
            Assert.AreEqual(expectedModel, res1, _comparer);
            var res2 = _caching.Get(id);
            Assert.AreEqual(1, _caching.QueriedRows);
            Assert.AreEqual(1, _caching.QueriedTimes);
            Assert.AreEqual(expectedModel, res2, _comparer);
        }
        
        [TestMethod]
        [DataRow("1,2,3")]
        [DataRow("2,3,4")]
        public void QueryRange(string idsStr)
        {
            var ids = TestStrParse.IntList(idsStr);
            
            var res1 = _caching.GetRange(ids);
            Assert.AreEqual(3, _caching.QueriedRows);
            Assert.AreEqual(1, _caching.QueriedTimes);
            IsSameList(ids, res1);
            
            var res2 = _caching.GetRange(ids);
            Assert.AreEqual(3, _caching.QueriedRows);
            Assert.AreEqual(1, _caching.QueriedTimes);
            IsSameList(ids, res2);
        }

        [TestMethod]
        [DataRow("1,2,3", 3, 1, "2,3", 0, 0, "1,2,3")]
        [DataRow("1,2,3", 3, 1, "3,4", 1, 1, "1,2,3,4")]
        [DataRow("1,2,3", 3, 1, "4,5", 2, 1, "1,2,3,4,5")]
        public void QueryMutipleTimes(
            string q1Ids, int q1Rows, int q1Times,
            string q2Ids, int q2Rows, int q2Times, string finalIds)
        {
            var ids = TestStrParse.IntList(q1Ids);
            var res1 = _caching.GetRange(ids);
            Assert.AreEqual(q1Rows, _caching.QueriedRows);
            Assert.AreEqual(q1Times, _caching.QueriedTimes);
            IsSameList(ids, res1);
            ids = TestStrParse.IntList(q2Ids);
            var res2 = _anotherCaching.GetRange(ids);
            Assert.AreEqual(q2Rows, _anotherCaching.QueriedRows);
            Assert.AreEqual(q2Times, _anotherCaching.QueriedTimes);
            IsSameList(ids, res2);

            var final = TestStrParse.IntList(finalIds);
            var data = _caching.GetDataList();
            var actualFinal = data.ConvertAll(x => x.Id);
            Assert.AreEqual(final.Count, data.Count);
            CollectionAssert.AllItemsAreNotNull(data);
            CollectionAssert.AreEquivalent(final, actualFinal);
        }
        
        [TestMethod]
        [DataRow("1,2,3", "2,3", 3, "1,2,3")]
        [DataRow("1,2,3", "3,4", 4, "1,2,3,4")]
        [DataRow("1,2,3", "4,5", 5, "1,2,3,4,5")]
        public void QuerySimultaneously(
            string q1Ids, string q2Ids, int qRows, string finalIds)
        {
            var ids1 = TestStrParse.IntList(q1Ids);
            var ids2 = TestStrParse.IntList(q2Ids);

            var tsk1 = Task.Run(() =>
            {
                var res = _caching.GetRange(ids1);
                IsSameList(ids1, res);
            });
            var tsk2 = Task.Run(() =>
            {
                var res = _anotherCaching.GetRange(ids2);
                IsSameList(ids2, res);
            });
            Task.WaitAll(tsk1, tsk2);
            Assert.AreEqual(qRows, _caching.QueriedRows + _anotherCaching.QueriedRows);
            
            var final = TestStrParse.IntList(finalIds);
            var data = _caching.GetDataList();
            var actualFinal = data.ConvertAll(x => x.Id);
            Assert.AreEqual(final.Count, data.Count);
            CollectionAssert.AllItemsAreNotNull(data);
            CollectionAssert.AreEquivalent(final, actualFinal);
            Assert.IsFalse(_anotherCaching.HoldingAll);
        }
        
        [TestMethod]
        [DataRow("", 5, 1)]
        [DataRow("1", 4, 1)]
        [DataRow("1,2", 3, 1)]
        [DataRow("1,2,3,4", 1, 1)]
        [DataRow("1,2  3,4", 1, 1)]
        [DataRow("1,2,3  3,4", 1, 1)]
        [DataRow("1,2,3  2,3,4", 1, 1)]
        [DataRow("1,2,3,4,5", 0, 1)]
        [DataRow("1,2,3  4,5", 0, 1)]
        [DataRow("1,2,3  3,4,5", 0, 1)]
        [DataRow("1,2,3,4,5  3,4,5", 0, 1)]
        public void QueryAll(
            string q1Ids, int q2Rows, int q2Times)
        {
            var idss = TestStrParse.IntListList(q1Ids);
            foreach (var ids in idss)
            {
                var res = _caching.GetRange(ids);
                IsSameList(ids, res);   
            }
            Assert.IsFalse(_anotherCaching.HoldingAll);

            var res2 = _anotherCaching.GetAll();
            Assert.AreEqual(q2Rows, _anotherCaching.QueriedRows);
            Assert.AreEqual(q2Times, _anotherCaching.QueriedTimes);
            IsSameList([1,2,3,4,5], res2);
            Assert.IsTrue(_anotherCaching.HoldingAll);

            var data = _caching.GetDataList();
            var actualFinal = data.ConvertAll(x => x.Id);
            Assert.AreEqual(5, data.Count);
            CollectionAssert.AllItemsAreNotNull(data);
            CollectionAssert.AreEquivalent(new List<int>{1,2,3,4,5}, actualFinal);
            
            //没有增删的情况下，再次查询，查询次数不会变多
            var res3 = _anotherCaching.GetAll();
            Assert.AreEqual(q2Rows, _anotherCaching.QueriedRows);
            Assert.AreEqual(q2Times, _anotherCaching.QueriedTimes);
            IsSameList([1,2,3,4,5], res3);
            Assert.IsTrue(_anotherCaching.HoldingAll);
        }

        [TestMethod]
        [DataRow("BBB", "CCC")]
        [DataRow("DDD", "EEE")]
        public void QueryOtherProp(string name1, string name2)
        {
            Func<SomeCachingModel, string, bool> match = (x,y) => x.Name == y;
            Func<IQueryable<SomeDbModel>, string, IQueryable<SomeDbModel>> matchDb
                = (x, y) => x.Where(m => m.Name == y);
            var tsk1 = Task.Run(() =>
            {
                var res1 = _caching.GetCustom(
                    x => match(x, name1),
                    x=>matchDb(x, name1))!;
                Assert.AreEqual(res1.Name, name1);
                var res2 = _caching.GetCustom(
                    x => match(x, name2),
                    x=>matchDb(x, name2))!;
                Assert.AreEqual(res2.Name, name2);
            });
            var tsk2 = Task.Run(() =>
            {
                var res = _anotherCaching.GetCustom(
                    x => match(x, name2),
                    x=>matchDb(x, name2))!;
                Assert.AreEqual(res.Name, name2);
            });
            Task.WaitAll(tsk1, tsk2);
            Assert.AreEqual(2, _caching.QueriedTimes + _anotherCaching.QueriedTimes);
            Assert.AreEqual(2, _caching.QueriedRows + _anotherCaching.QueriedRows);
        }

        [TestMethod]
        [DataRow(true, 0, 1)]
        [DataRow(false, 5, 1)]
        public void Insert(bool queryAllFirst, int expectRows, int expectTimes)
        {
            if (queryAllFirst)
            {
                var res1 = _caching.GetAll().ConvertAll(x => x.Id);
                CollectionAssert.AreEquivalent(new List<int>() { 1, 2, 3, 4, 5 }, res1);
                Assert.AreEqual(5, _caching.QueriedRows);
                Assert.AreEqual(1, _caching.QueriedTimes);
            }
            _fakeDb.Add(_item6);
            _caching.Insert(_item6);
            var res2 = _anotherCaching.GetAll().ConvertAll(x=>x.Id);
            CollectionAssert.AreEquivalent(new List<int>(){1,2,3,4,5,6}, res2);
            Assert.AreEqual(expectRows, _anotherCaching.QueriedRows);
            Assert.AreEqual(expectTimes, _anotherCaching.QueriedTimes);
        }
        [TestMethod]
        [DataRow(true, 0, 1)]
        [DataRow(false, 5, 1)]
        public void InsertRange(bool queryAllFirst, int expectRows, int expectTimes)
        {
            if (queryAllFirst)
            {
                var res1 = _caching.GetAll().ConvertAll(x => x.Id);
                CollectionAssert.AreEquivalent(new List<int>() { 1, 2, 3, 4, 5 }, res1);
                Assert.AreEqual(5, _caching.QueriedRows);
                Assert.AreEqual(1, _caching.QueriedTimes);
            }
            _fakeDb.AddRange([_item6, _item7]);
            _caching.InsertRange([_item6, _item7]);
            var res2 = _anotherCaching.GetAll().ConvertAll(x=>x.Id);
            CollectionAssert.AreEquivalent(new List<int>(){1,2,3,4,5,6,7}, res2);
            Assert.AreEqual(expectRows, _anotherCaching.QueriedRows);
            Assert.AreEqual(expectTimes, _anotherCaching.QueriedTimes);
        }

        [TestMethod]
        public void Update()
        {
            var dModel = _fakeDb.Find(x => x.Id == 1)!;
            dModel.Name = "XXX";
            _caching.UpdateByDbModel(dModel);
            Assert.AreEqual(1, _caching.QueriedRows);
            Assert.AreEqual(1, _caching.QueriedTimes);
            Assert.AreEqual(1, _caching.GetDataList().Count);
            Assert.AreEqual("XXX", _caching.GetDataList()[0].Name);
        }
        [TestMethod]
        public void UpdateRange()
        {
            var dModel1 = _fakeDb.Find(x => x.Id == 1)!;
            var dModel2 = _fakeDb.Find(x => x.Id == 2)!;
            dModel1.Name = "XXX";
            dModel2.Name = "YYY";
            _caching.UpdateRangeByDbModel([dModel1, dModel2]);
            Assert.AreEqual(2, _caching.QueriedRows);
            Assert.AreEqual(1, _caching.QueriedTimes);
            Assert.AreEqual(2, _caching.GetDataList().Count);
            var cModel1 = _anotherCaching.Get(1)!;
            var cModel2 = _anotherCaching.Get(2)!;
            Assert.AreEqual(0, _anotherCaching.QueriedRows);
            Assert.AreEqual(0, _anotherCaching.QueriedRows);
            Assert.AreEqual("XXX", cModel1.Name);
            Assert.AreEqual("YYY", cModel2.Name);
        }

        [TestMethod]
        [DataRow("1")]
        [DataRow("1,2,3")]
        [DataRow("1,2,3,4,5")]
        public void Remove(string remove)
        {
            _caching.GetAll();
            var removeIds = TestStrParse.IntList(remove);
            int expectedQueryTimes = 0;
            foreach (var id in removeIds)
            {
                expectedQueryTimes++;
                _fakeDb.RemoveAll(x => id == x.Id);
                _caching.Remove(id);   
                _anotherCaching.GetAll();
                Assert.AreEqual(expectedQueryTimes, _anotherCaching.QueriedTimes);
            }
            Assert.AreEqual(1, _caching.QueriedTimes);
        }
        [TestMethod]
        [DataRow("1")]
        [DataRow("1,2,3")]
        [DataRow("1,2,3,4,5")]
        public void RemoveRange(string remove)
        {
            _caching.GetAll();
            var removeIds = TestStrParse.IntList(remove);
            _fakeDb.RemoveAll(x => removeIds.Contains(x.Id));
            _caching.RemoveRange(removeIds);
            _anotherCaching.GetAll();
            Assert.AreEqual(1, _anotherCaching.QueriedTimes);
            Assert.AreEqual(1, _caching.QueriedTimes);
        }

        [TestMethod]
        public void ParallelOps1()
        {
            _fakeDb.Add(_item6);
            _fakeDb.Add(_item7);
            Task tsk1 = Task.Run(() =>
            {
                _caching.Insert(_item6);
            });
            Task tsk2 = Task.Run(() =>
            {
                _anotherCaching.Insert(_item7);
            });
            Task.WaitAll(tsk1, tsk2);
            var list = _caching.GetDataList();
            CollectionAssert.AllItemsAreNotNull(list);
            CollectionAssert.AreEquivalent(new List<int>(){6,7}, list.ConvertAll(x=>x.Id));
            _caching.Get(7);
            _anotherCaching.Get(6);
            Assert.IsFalse(_caching.HoldingAll);
            Assert.AreEqual(0, _caching.QueriedTimes + _anotherCaching.QueriedTimes);

            _fakeDb.Remove(_item6);
            _fakeDb.Remove(_item7);
            Task tsk3 = Task.Run(() =>
            {
                _caching.Remove(6);
            });
            Task tsk4 = Task.Run(() =>
            {
                _anotherCaching.Remove(7);
            });
            Task.WaitAll(tsk3, tsk4);
            CollectionAssert.AllItemsAreNotNull(list);
            CollectionAssert.AreEquivalent(new List<int>(), list.ConvertAll(x=>x.Id));
            Assert.IsFalse(_caching.HoldingAll);
            Assert.AreEqual(0, _caching.QueriedTimes + _anotherCaching.QueriedTimes);
        }
        [TestMethod]
        public void ParallelOps2()
        {
            _caching.GetAll();
            Assert.IsTrue(_caching.HoldingAll);
            
            _fakeDb.Add(_item6);
            _fakeDb.Add(_item7);
            Task tsk1 = Task.Run(() =>
            {
                _caching.Insert(_item6);
            });
            Task tsk2 = Task.Run(() =>
            {
                _anotherCaching.Insert(_item7);
            });
            Task.WaitAll(tsk1, tsk2);
            var list = _caching.GetDataList();
            CollectionAssert.AllItemsAreNotNull(list);
            CollectionAssert.AreEquivalent(new List<int>(){1,2,3,4,5,6,7}, list.ConvertAll(x=>x.Id));
            _caching.Get(7);
            _anotherCaching.Get(6);
            Assert.IsFalse(_caching.HoldingAll);
            Assert.AreEqual(1, _caching.QueriedTimes + _anotherCaching.QueriedTimes);

            _caching.GetAll();
            Assert.IsTrue(_caching.HoldingAll);
            
            _fakeDb.Remove(_item6);
            _fakeDb.Remove(_item7);
            Task tsk3 = Task.Run(() =>
            {
                _caching.Remove(6);
            });
            Task tsk4 = Task.Run(() =>
            {
                _anotherCaching.Remove(7);
            });
            Task.WaitAll(tsk3, tsk4);
            CollectionAssert.AllItemsAreNotNull(list);
            CollectionAssert.AreEquivalent(new List<int>(){1,2,3,4,5}, list.ConvertAll(x=>x.Id));
            Assert.IsFalse(_caching.HoldingAll);
            Assert.AreEqual(2, _caching.QueriedTimes + _anotherCaching.QueriedTimes);
        }
        
        [TestMethod]
        public void ParallelOps3()
        {
            var m1 = _fakeDb.Find(x => x.Id == 1)!;
            m1.Name = "XXX";
            var m2 = _fakeDb.Find(x => x.Id == 2)!;
            m2.Name = "YYY";
            Task tsk1 = Task.Run(() =>
            {
                _caching.UpdateByDbModel(m1);
            });
            Task tsk2 = Task.Run(() =>
            {
                _anotherCaching.UpdateByDbModel(m2);
            });
            Task.WaitAll(tsk1, tsk2);
            var list = _caching.GetDataList();
            CollectionAssert.AllItemsAreNotNull(list);
            CollectionAssert.AreEquivalent(new List<string>(){"XXX","YYY"}, list.ConvertAll(x=>x.Name));
            Assert.IsFalse(_caching.HoldingAll);
            Assert.AreEqual(2, _caching.QueriedTimes + _anotherCaching.QueriedTimes);
        }
        [TestMethod]
        public void ParallelOps4()
        {
            _caching.GetAll();
            
            var m1 = _fakeDb.Find(x => x.Id == 1)!;
            m1.Name = "XXX";
            var m2 = _fakeDb.Find(x => x.Id == 2)!;
            m2.Name = "YYY";
            Task tsk1 = Task.Run(() =>
            {
                _caching.UpdateByDbModel(m1);
            });
            Task tsk2 = Task.Run(() =>
            {
                _anotherCaching.UpdateByDbModel(m2);
            });
            Task.WaitAll(tsk1, tsk2);
            var list = _caching.GetDataList();
            CollectionAssert.AllItemsAreNotNull(list);
            CollectionAssert.AreEquivalent(new List<string>(){"XXX","YYY","CCC","DDD","EEE"}, list.ConvertAll(x=>x.Name));
            Assert.IsTrue(_caching.HoldingAll);
            Assert.AreEqual(1, _caching.QueriedTimes + _anotherCaching.QueriedTimes);
        }
    }
}