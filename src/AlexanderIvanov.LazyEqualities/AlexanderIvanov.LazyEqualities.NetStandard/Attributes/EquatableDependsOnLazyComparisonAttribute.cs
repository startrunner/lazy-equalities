using System;

namespace AlexanderIvanov.LazyEqualities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class EquatableDependsOnLazyComparisonAttribute : Attribute { }
}

