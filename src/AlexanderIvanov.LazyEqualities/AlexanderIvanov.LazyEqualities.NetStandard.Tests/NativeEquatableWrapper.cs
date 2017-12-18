using System;
using System.Collections.Generic;
using System.Text;

namespace AlexanderIvanov.LazyEqualities.NetStandard.Tests
{
    [EquatableDependsOnLazyComparisonAttribute]
    class NativeEquatableWrapper : IEquatable<NativeEquatableWrapper>
    {
        public string StringValue { get; }
        public NativeEquatable Native { get; }
        public NativeEquatableWrapper(string str)
        {
            StringValue = str;
            Native = NativeEquatable.Parse(str);
        }

        public bool Equals(NativeEquatableWrapper other) => LazyEquality.Equals(this, other);
    }
}
