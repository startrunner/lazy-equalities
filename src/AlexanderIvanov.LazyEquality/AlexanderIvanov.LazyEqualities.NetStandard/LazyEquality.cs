using System;
using System.Collections.Generic;
using System.Text;

namespace AlexanderIvanov.LazyEqualities
{
    public static class LazyEquality
    {
        public static bool Equals<T>(T x, T y) => !NotEquals(x, y);
        public static bool NotEquals<T>(T x, T y) => AlexanderIvanov.LazyEqualities.NotEquals<T>.CompareNotEqual(x, y);
    }
}
