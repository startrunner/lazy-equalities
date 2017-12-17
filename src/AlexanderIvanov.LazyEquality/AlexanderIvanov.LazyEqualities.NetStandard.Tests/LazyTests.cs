using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlexanderIvanov.LazyEqualities;
using Xunit;

namespace AlexanderIvanov.LazyEqualityTests
{
    public class LazyTests
    {
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
        [InlineData("gosho", "thosho")]
        public void TestEquatableClass(string tx, string ty)
        {
            TestEquatable x = TestEquatable.Parse(tx);
            TestEquatable y = TestEquatable.Parse(ty);

            Assert.Equal(x.Equals(other: y), LazyEqualities.LazyEquality.Equals<TestEquatable>(x, y));
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
            Assert.Equal(x.Equals(y), LazyEqualities.LazyEquality.Equals<ComplexClassWithInts>(x, y));
        }
    }
}
