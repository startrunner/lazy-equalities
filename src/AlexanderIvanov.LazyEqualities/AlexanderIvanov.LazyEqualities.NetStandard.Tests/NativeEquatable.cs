using System;
using System.Collections.Generic;
using System.Text;

namespace AlexanderIvanov.LazyEqualities.NetStandard.Tests
{
    class NativeEquatable : IEquatable<NativeEquatable>
    {
        private string str;

        public static NativeEquatable Parse(string str)
        {
            if (str is null) { return null; }
            return new NativeEquatable(str);
        }

        private NativeEquatable(string str) => this.str = str;

        public bool Equals(NativeEquatable other) => str.Equals(other.str);
    }
}
