using System;
using System.Collections.Generic;
using System.Text;

namespace AlexanderIvanov.LazyEqualities.NetStandard.Tests
{
    [EquatableDependsOnLazyComparisonAttribute]
    class LazyIEquatableClass : IEquatable<LazyIEquatableClass>
    {
        public int X { get; }

        public LazyIEquatableClass(int x)
        {
            X = x;
        }

        public bool Equals(LazyIEquatableClass other) => LazyEquality.Equals(this, other);

        public override int GetHashCode() => base.GetHashCode();
    }
}
