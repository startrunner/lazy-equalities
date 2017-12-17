using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AlexanderIvanov.LazyEqualities.NetStandard.Tests
{
    public class LazyTests
    {
        public LazyTests()
        {
            Assembly
                .GetAssembly(typeof(LazyTests))
                .GetTypes()
                .SelectMany(x => new Type[] {
                    x,
                    typeof(IReadOnlyList<>).MakeGenericType(x),
                    typeof(List<>).MakeGenericType(x),
                    typeof(LinkedList<>).MakeGenericType(x),
                    typeof(IReadOnlyList<>).MakeGenericType(x)
                })
                .Select(x => LazyEquality.EnsureInitialized(x));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("1337", "1337", true)]
        [InlineData("", " ", false)]
        [InlineData(null, "", false)]
        [InlineData("", null, false)]
        [InlineData(null, null, true)]
        public void TestSimpeList(string xs, string ys, bool equal)
        {
            List<char> x = xs?.ToList();
            List<char> y = ys?.ToList();

            Assert.Equal(LazyEqualities.LazyEquality.Equals(x, y), equal);
        }

        [Theory]
        [InlineData("ivan", "ivan")]
        [InlineData("dragan", "petkan")]
        [InlineData("pesho", "pesho")]
        [InlineData("gosho", "tosho")]
        public void TestEquatableClass(string tx, string ty)
        {
            NativeEquatableWrapper x = new NativeEquatableWrapper(tx);
            NativeEquatableWrapper y = new NativeEquatableWrapper(ty);

            Assert.Equal(tx == ty, LazyEqualities.LazyEquality.Equals(x, y));
        }

        [Theory]
        [InlineData(new[] { 4, 8, 15, 16, 23, 42 }, new[] { 4, 8, 15, 16, 23, 42 })]
        [InlineData(new[] { 4, 8, 15, 16, 23, 42 }, new[] { 4, 8, 15, 16, 23, 43 })]
        [InlineData(new[] { 1, 3, 3, 7 }, new[] { 1, 3, 3, 7 })]
        [InlineData(new[] { 1, 2, 3 }, new int[] { })]
        [InlineData(new[] { 4, 5, 6 }, new[] { 7, 8, 9 })]
        public void TestMemberwiseLists(int[] seedsX, int[] seedsY)
        {
            var x = new LinkedList<ComplexClassWithInts>(
                seedsX.Select(z => new ComplexClassWithInts(new Random(z)))
            );

            var y = new LinkedList<ComplexClassWithInts>(
                seedsY.Select(z => new ComplexClassWithInts(new Random(z)))
            );

            Assert.Equal(x.SequenceEqual(y), LazyEquality.Equals(x, y));
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1337, 1337)]
        [InlineData(481516, 2342)]
        [InlineData(12, 13)]
        [InlineData(5, 6)]
        public void TestMemberwiseClass(int seedX, int seedY)
        {
            ComplexClassWithInts x = new ComplexClassWithInts(new Random(seedX));
            ComplexClassWithInts y = new ComplexClassWithInts(new Random(seedY));
            Assert.Equal(x.Equals(y), LazyEqualities.LazyEquality.Equals(x, y));
        }

        [Theory]
        [InlineData(SomeEnum.Alpha, SomeEnum.Beta)]
        [InlineData(SomeEnum.Gamma, SomeEnum.Gamma)]
        public void TestEnum(SomeEnum x, SomeEnum y)
        {
            Assert.Equal(x == y, LazyEquality.Equals(x, y));
        }

        [Theory]
        [InlineData(1, 3)]
        [InlineData(2, 5)]
        [InlineData(101, 101)]
        public void TestVeryComplex(int seedX, int seedY)
        {
            VeryComplexClass x = new VeryComplexClass(seedX);
            VeryComplexClass y = new VeryComplexClass(seedY);

            Assert.Equal(x.Equals(y), LazyEquality.Equals(x, y));
        }

        [Theory]
        [InlineData("ivan", "ivan")]
        [InlineData("Ivan", "not Ivan")]
        [InlineData("ivan", "ivan + ivan = 2*ivan")]
        [InlineData("krokodil", "krokodil")]
        [InlineData("123", "123")]
        public void TestDifferentEnumerables(string xs, string ys)
        {
            IEnumerable<char> x = xs.ToList();
            IEnumerable<char> y = new LinkedList<char>(ys);

            Assert.Equal(xs == ys, LazyEquality.Equals(x, y));
        }

        [Theory]
        [InlineData("pesho", "gosho")]
        [InlineData("321", "123")]
        [InlineData("krokodil", "krokodil")]
        public void TestNotEqualToHashSet(string xs, string ys)
        {
            IEnumerable<char> x = xs;
            IEnumerable<char> y = new HashSet<char>(ys);

            Assert.False(LazyEquality.Equals(x, y));
        }

        [Theory]
        [InlineData(1337, 1337)]
        [InlineData(1, 3)]
        public void TestInterfaced(int x, int y)
        {
            Assert.Equal(x == y, LazyEquality.Equals(new LazyIEquatableClass(x), new LazyIEquatableClass(y)));
        }
    }
}
