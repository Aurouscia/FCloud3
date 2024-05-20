using FCloud3.Diff.Object;
using Newtonsoft.Json;

namespace FCloud3.Diff.Test.Object
{
    [TestClass]
    public class ObjectDiffRevertTest
    {
        private TestClass Data { get; set; }
        public ObjectDiffRevertTest()
        {
            Data = new TestClass()
            {
                A = 666,
                B = "Hello",
                C = null,
                D = [1,2,3,4]
            };
        }

        [TestMethod]
        public void Simple()
        {
            var mutated = Data.DeepCloned();
            mutated.B = "HelloWorld";
            var diffs = ObjectDiffSearch.Run(Data, mutated);
            Assert.AreEqual(1,diffs.Count);
            Assert.AreEqual(5, diffs[0].New);

            var originalReadable = ObjectDiff.ObjectReadable(Data);
            var mutatedReadable = ObjectDiff.ObjectReadable(mutated);
            var reverted = diffs.RevertAll(mutatedReadable);
            Assert.AreEqual(originalReadable, reverted);
        }

        [TestMethod]
        public void Mutiple()
        {
            var mutated = Data.DeepCloned();
            mutated.B = "Hello\nWorld";
            mutated.D![2] = 999;
            var diffs = ObjectDiffSearch.Run(Data, mutated);
            Assert.AreEqual(2, diffs.Count);

            var originalReadable = ObjectDiff.ObjectReadable(Data);
            var mutatedReadable = ObjectDiff.ObjectReadable(mutated);
            var reverted = diffs.RevertAll(mutatedReadable);
            Assert.AreEqual(originalReadable, reverted);
        }

        [TestMethod]
        public void Inserted()
        {
            var mutated = Data.DeepCloned();
            mutated.C = new() { A = 3, B = "3", C = null, D = [] };
            var diffs = ObjectDiffSearch.Run(Data, mutated);

            var originalReadable = ObjectDiff.ObjectReadable(Data);
            var mutatedReadable = ObjectDiff.ObjectReadable(mutated);
            var reverted = diffs.RevertAll(mutatedReadable);
            Assert.AreEqual(originalReadable, reverted);
        }

        public class TestClass
        {
            public int A { get; set; }
            public string? B { get; set; }
            public TestClass? C { get; set; }
            public List<int>? D { get; set; }

            public TestClass DeepCloned()
            {
                string json = JsonConvert.SerializeObject(this);
                return JsonConvert.DeserializeObject<TestClass>(json)!;
            }
        }
    }
}
