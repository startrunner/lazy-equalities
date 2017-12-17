using System;

namespace AlexanderIvanov.LazyEqualities
{
    public static class LazyEquality
    {
        public static bool Equals<T>(T x, T y) => !NotEquals(x, y);
        public static bool NotEquals<T>(T x, T y) => AlexanderIvanov.LazyEqualities.NotEquals<T>.CompareNotEqualsManual(x, y);

        public static bool EnsureInitialized(Type type)
        {
            typeof(NotEquals<>).MakeGenericType(type).TypeInitializer.Invoke(Array.Empty<object>());
            return true;
        }

        [Obsolete("Generic parameters needed for '" + nameof(Equals) + "<T>(T x, T y)' Method", error: true)]
        public static new bool Equals(object x, object y) => throw new NotImplementedException();
    }
}
