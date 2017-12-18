# Lazy Equalities

**Lazy Equalities** is a simple to use utility library designed to ease .NET developers in implementing object comparison functionality: `Equals(object obj)`, `==` and `!=`. Since doing so error-freely for many types is a tedious task of writing lines upon lines of generic boilerplate code, **Lazy Equalities** aims to do (part of) the job for you.

## Some Examples

### An example of C# comparison boilerplate

```c#
class SomeClass
{
    public int X { get; }
    public int Y { get; }
    public AnotherClass Z { get; }
    public int NewlyAddedPropertyYouForgotAbout { get; }

  	public override int GetHashCode() => base.GetHashCode();
  	public override bool Equals(object obj) => this == obj as SomeClass;
    public static bool operator ==(SomeClass x, SomeClass y) => !(x != y);
    public static bool operator !=(SomeClass x, SomeClass y)
    {
        /*Non-equality is more easily provable than equality, hence the inversed dependency :)*/
        if(ReferenceEquals(x, y)) { return false; }
        if(x is null != y is null) { return true; }

        return
            x.X != y.X ||
            x.Y != y.Y ||
            x.Z != y.Z;
    }
}
//...And it used to be even uglier before C# 7.0
```
### The same comparison using Lazy Equalities

    using AlexanderIvanov.LazyEqualities;
    
    class SomeClass
    {
        public int X { get; }
        public int Y { get; }
        public AnotherClass Z { get; }
        public int NewlyAddedProperty { get; }
    
    	public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj) => this == obj as SomeClass;
        public static bool operator ==(SomeClass x, SomeClass y) => LazyEquality.Equals(x, y);
        public static bool operator !=(SomeClass x, SomeClass y) => LazyEquality.NotEquals(x, y);
    }
    //Neater, isn't it?


## Usage in details

### Comparison Rules

- All definitions required for the usage of this library are located in the `AlexanderIvanov.LazyEqualities` namespace.

- The `Equals` and `NotEquals` methods having a generic interface like this:  
  `public static bool LazyEquality.Equals<T> (T x, T y)` and 
  will be referred to as '**the two comparison methods**'.

- Comparison of `x` and `y` will be performed according to the first matched rule:

  - The value of `x==y` or `x!=y` will be returned if the generic parameter is a **[primitive type, string](https://msdn.microsoft.com/en-us/library/ms228360(v=vs.90).aspx) or an enum type**.

  - If T implements the `System.IEquatable<T>` [interface](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1?view=netstandard-2.0):

    - If the defining class of `T` has an `EquatableDependsOnLazyComparisonAttrobute` attribute,  the `IEquatable<T>` interface of T will NOT be considered.

    - Otherwise, the system will consider the absence of such attribute:

      - If this is NOT a direct invocation of one of the two comparison methods, the value of `(x as IEquatable<T>).Equals(y)` or its opposite will be returned. 
      - If this is a direct invocation of one of the two comparison methods, an exception will be thrown;

      ```pla
      System.InvalidOperationException : Type AlexanderIvanov.LazyEqualities.NetStandard.Tests.NativeEquatableWrapper cannot make use of lazy comparison for the implementation of System.IEquatable`1[AlexanderIvanov.LazyEqualities.NetStandard.Tests.NativeEquatableWrapper] without having the AlexanderIvanov.LazyEqualities.EquatableDependsOnLazyComparisonAttribute attribute.
      ```

      This attribute implies that the implementation of `IEquatable<T>` depends on Lazy Equalities. Its lack thereof means that the implementation is either a native boilerplate or uses another library. The above exception is in place in order to prevent a stack overflow when `LazyEquality.Equals<T>(T, T)` is called and in turn it calls itself.

  - If `T` implements a `System.Collections.Generic.IEnumerable<TItem>` interface or is such interface itself:

    - A sequential equality check will be performed, each item being compared (Recursively, following the same ruleset)
    - Two collections with a different item count will **never** be considered equal.
    - If T implements `IEnumerable<TItem>` for more than one different `TItem`, only the first one will be considered.

  - If T only implements the non-generic `System.Collections.IEnumerable` interface:

    - A sequential equality check will be performed, each item being compared with the default `x.CompareTo(object obj)` method (**not recursively**), whether or not it's been overridden and implemented somehow (using lazy equalities, boilerplate code or otherwise);
    - Two collections with a different item count will **never** be considered equal.

  - In all other cases, a recursive memberwise comparison will be performed:

    - The values of all non-static properties with a getter that **do not have** an `EqualityExcludeAttribute` will be considered.
    - The values of all non-static fields that **do have** an `EqualityIncludeAttribute` will be considered

### Some Examples (Constructors omitted)

```c#
using AlexanderIvanov.LazyEqualities;
using System.Threading;
using System;
using ...;

public class SomeClass
{
	//This field will not be considered because it's static
	private static IDCounter = 0;
	
	//...And neither will this static property
	static string Identifier{ get=>nameof(SomeClass); set=>throw new Exception("WtfuDo?"); }

	//this field will NOT be considered (implicitly, because it's a field)
	internal int objectID= Interlocked.Increment(ref IDCounter);
	
	//but this one will (explicitly, using an attribute)
	[EqualityInclude]
	private int x=12;

	//These three properties will be considered (implicitly)
    protected internal int X { get; internal set; }
    private int Y { get; set; }
    public AnotherClass Z { get; } //The value of this one will be compared recursively
    
    //This property will not be considered (explicitly, using an attribute)
    [EqualityExclude]
    public int NewlyAddedProperty { get; protected set; }
    //This property will not be considered because it doesn't have a getter.
    internal ISomeInterface SetterInjection{ set=> Console.WriteLine("Please don't do this."); } 

  	//It's good practice for non-internal classes to implement these for more seamless 
  	//comparisons.
	public override int GetHashCode() => base.GetHashCode();
    public override bool Equals(object obj) => this == obj as SomeClass;
    public static bool operator ==(SomeClass x, SomeClass y) => LazyEquality.Equals(x, y);
    public static bool operator !=(SomeClass x, SomeClass y) => LazyEquality.NotEquals(x, y);
}
```

```C#
//This class doesn't have an EquatableDependsOnLazyComparisonAttribute
//Because the Equals method doesn't use the lazy comparison library.
internal class NativeEquatable : IEquatable<NativeEquatable>
{
        private string str;
        public bool Equals(NativeEquatable other) => str.Equals(other.str);
}

//But this one does because it...does!
[EquatableDependsOnLazyComparisonAttribute]
internal class NativeEquatableWrapper : IEquatable<NativeEquatableWrapper>
{
	//this one will be compared using ==
    public string StringValue { get; }
    
    //this one will be compared using IEquatable<NativeEquatable>.Equals(NativeEquatable).
    public NativeEquatable Native { get; }
												
    public bool Equals(NativeEquatableWrapper other) => 
    	LazyEquality.Equals(this, other);//<---RIGHT HERE
}
```



## How it works

On every invocation of a comparison method, a corresponding `NotEquals<T>.Compare(T x, T y)` method will be called. Note that the `internal static NotEquals<T>`  is generic which allows per-type delegate storage using a static field initialized upon first usage.

Said delegates are compiled using [reflection](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/reflection) and [expression trees](https://www.codeproject.com/Articles/235860/Expression-Tree-Basics) (once per type) . Reflection is not used upon further comparisons for the same type. This delegate caching technique is often used by [dependency injection (DI) frameworks](https://en.wikipedia.org/wiki/Dependency_injection). It is to be expected that ~~performance~~ it always hurts the first time but subsequent invocations are seamless. 

The `LazyEquality` class also exposes the `EnsureInitialized(Type type)` method which will ensure the comparator delegate for `type` has been initialized.

This library is written under [.Net Standard](https://blogs.msdn.microsoft.com/dotnet/2016/09/26/introducing-net-standard/) 2.0 and should work across .Net Core and recent versions of the Windows-only .Net.

## Links

[GitHub repo](https://github.com/startrunner/lazy-equalities)

[NuGet Package](https://www.nuget.org/packages/AlexanderIvanov.LazyEqualities/)





© Alexander "startrunner" Ivanov 2017 - 2018