using System;
using System.Collections.Generic;
using System.Text;

namespace AlexanderIvanov.LazyEqualityTests
{
    class TestEquatable : IEquatable<TestEquatable>
    {
        private string str;

        public static TestEquatable Parse(string str)
        {
            if (str is null) { return null; }
            return new TestEquatable(str);
        }

        private TestEquatable(string str) => this.str = str;

        public bool Equals(TestEquatable other) => str.Equals(other.str);
    }
}
