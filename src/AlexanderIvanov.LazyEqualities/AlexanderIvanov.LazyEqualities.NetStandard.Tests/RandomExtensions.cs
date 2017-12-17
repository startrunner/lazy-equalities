using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlexanderIvanov.LazyEqualities.NetStandard.Tests
{
    enum E
    {

    }

    public static class RandomExtensions
    {
        public static T NextEnum<T>(this Random random) where T : struct
        {
            Type type = typeof(T);
            if (!type.IsEnum) { throw new ArgumentException("Type is not enum!"); }

            return (T)(object)random.Next(maxValue: Enum.GetValues(type).Length);
        }

        public static IEnumerable<T> ToRandomEnumerable<T>(this IEnumerable<T> enumerable, Random rand)
        {
            switch(rand.Next()%4)
            {
                case 0:
                    return enumerable.ToList();
                case 1:
                    return enumerable.ToArray();
                default:
                    return new LinkedList<T>(enumerable);
            }
        }
    }
}
