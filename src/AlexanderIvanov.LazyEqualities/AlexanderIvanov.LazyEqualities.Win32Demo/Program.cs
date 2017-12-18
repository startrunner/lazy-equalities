using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AlexanderIvanov.LazyEqualities.Win32Demo
{
    class AnotherClass
    {
        static int Called = 0;
        IEnumerable<int> SomeList { get; }

        public AnotherClass(int[] items)
        {
            if ((Called++) % 2 == 0)
            {
                SomeList = items.ToList();
            }
            else
            {
                SomeList = new LinkedList<int>(items);
            }
        }
    }

    class SomeClass
    {
        public int X { get; }
        public int Y { get; }
        public AnotherClass Z { get; }

        public SomeClass(int x, int y, params int[] items)
        {
            X = x;
            Y = y;
            Z = new AnotherClass(items);
        }

        public static bool operator ==(SomeClass x, SomeClass y) => LazyEquality.Equals(x, y);
        public static bool operator !=(SomeClass x, SomeClass y) => LazyEquality.NotEquals(x, y);
        public override bool Equals(object obj) => LazyEquality.Equals(this, obj as SomeClass);
        public override int GetHashCode() => base.GetHashCode();
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(new SomeClass(1, 2) == new SomeClass(1, 2));
            Console.WriteLine(new SomeClass(1, 2) == new SomeClass(1, 3));
            Console.WriteLine(new SomeClass(1, 2, 3, 3) == new SomeClass(1, 2, 3, 4));
            Console.WriteLine(new SomeClass(1, 2, 3, 3) == new SomeClass(1, 2, 3, 3));
            Console.WriteLine(new SomeClass(1, 2, 5, 5) == new SomeClass(1, 3, 5, 5));

            Console.WriteLine(LazyEquality.Equals<IEnumerable>(new ArrayList(new[] { 1, 2, 3 }), (new[] { 1, 2, 3 })));
            Console.WriteLine(LazyEquality.Equals(new ArrayList(new[] { 1, 2, 3 }), new ArrayList(new[] { 1, 2, 4 })));
        }
    }
}
