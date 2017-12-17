using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlexanderIvanov.LazyEqualities.NetStandard.Tests
{
    class VeryComplexClass
    {
        static Random StaticRandom = new Random(1337);

        public IReadOnlyList<ComplexClassWithInts> Yes1 { get; }

        [EqualityExclude]
        public List<ComplexClassWithInts> No1 { get; }

        public ComplexClassWithInts Yes2 { get; }
        public ComplexClassWithInts Yes3 { get; }
        protected int Yes4 { get; }

        private double Yes5 { get; }
        public SomeEnum Yes6 { get; }

        public IEnumerable<ComplexClassWithInts> Yes7 { get; }

        [EqualityExclude]
        public SomeEnum No2 { get; }

        [EqualityInclude]
        public int yeah1;
        public int nope1;

        [EqualityInclude]
        private double yeah2;

        [EqualityInclude]
        protected SomeEnum yeah3;

        [EqualityInclude]
        public ComplexClassWithInts yeah4;
        private int seed;

        public VeryComplexClass(int randomSeed) : this(new Random(randomSeed)) { this.seed = randomSeed; }

        private VeryComplexClass(Random random)
        {
            Yes1 =
                Enumerable
                .Repeat(0, random.Next(1000, 1999))
                .Select(x => new ComplexClassWithInts(random.Next()))
                .ToArray();
            Yes2 = new ComplexClassWithInts(random.Next());
            Yes3 = new ComplexClassWithInts(random.Next());
            Yes4 = random.Next();
            Yes5 = random.NextDouble();
            Yes6 = random.NextEnum<SomeEnum>();
            Yes7 = Enumerable
                .Repeat(0, random.Next(1000, 1999))
                .Select(x => new ComplexClassWithInts(random.Next()))
                .ToRandomEnumerable(StaticRandom);
            yeah1 = random.Next();
            yeah2 = random.NextDouble();
            yeah3 = random.NextEnum<SomeEnum>();
            yeah4 = new ComplexClassWithInts(random.Next());

            nope1 = StaticRandom.Next();
            No1 = Enumerable.Repeat(0, StaticRandom.Next(3, 15)).Select(x => new ComplexClassWithInts(StaticRandom.Next())).ToList();
            No2 = StaticRandom.NextEnum<SomeEnum>();
        }

        public override bool Equals(object obj)
        {
            var objCast = obj as VeryComplexClass;
            if (ReferenceEquals(this, objCast)) { return true; }
            if (objCast is null) { return false; }
            return 
                seed == objCast.seed && 
                Yes7.SequenceEqual(objCast.Yes7)&&
                Yes1.SequenceEqual(objCast.Yes1);
        }

        public override int GetHashCode() => throw new NotImplementedException();
    }
}
