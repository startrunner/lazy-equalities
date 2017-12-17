using System;

namespace AlexanderIvanov.LazyEqualities
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class EqualityIncludeAttribute : Attribute { }
}
