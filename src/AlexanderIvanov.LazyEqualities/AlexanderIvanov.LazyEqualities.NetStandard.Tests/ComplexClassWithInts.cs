using System;
using System.Diagnostics;
using AlexanderIvanov.LazyEqualities;
using Newtonsoft.Json;

namespace AlexanderIvanov.LazyEqualities.NetStandard.Tests
{
    class ComplexClassWithInts
    {
        static Random RandomStatic = new Random(1337);

        readonly int hashCode = RandomStatic.Next();

        public int a = RandomStatic.Next();
        public int b = RandomStatic.Next();

        [EqualityInclude]
        public int c;

        public int X { get; }
        public int Y { get; }

        [EqualityExclude]
        public int Z { get; } = RandomStatic.Next();

        public override bool Equals(object obj)
        {
            var objCast = obj as ComplexClassWithInts;
            if (ReferenceEquals(this, objCast)) { return true; }
            if (obj is null) { return false; }

            bool result =
                c == objCast.c &&
                X == objCast.X &&
                Y == objCast.Y;

            Debug.WriteLine("operator comparison of ");
            Debug.IndentLevel++;
            Debug.WriteLine(this.ToString());
            Debug.WriteLine(objCast.ToString());
            Debug.WriteLine($"is {result}.");
            Debug.IndentLevel--;

            return result;
        }

        public override int GetHashCode() => hashCode;

        public ComplexClassWithInts(int randomSeed) : this(new Random(randomSeed)) { }

        public ComplexClassWithInts(Random rand)
        {
            c = rand.Next();
            X = rand.Next();
            Y = rand.Next();
        }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
