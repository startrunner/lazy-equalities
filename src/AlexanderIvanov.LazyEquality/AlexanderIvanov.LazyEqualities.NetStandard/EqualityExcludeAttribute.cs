using System;

namespace AlexanderIvanov.LazyEqualities
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class EqualityExcludeAttribute : Attribute { }
}
