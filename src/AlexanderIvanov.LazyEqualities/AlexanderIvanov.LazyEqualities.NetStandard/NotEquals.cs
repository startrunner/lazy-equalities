using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace AlexanderIvanov.LazyEqualities
{
    internal static class NotEquals<T>
    {
        static int CompareNotEqualManuallyCalledFlag = 0;
        static readonly EqualityFunction<T> NotEqualsFunction = GenerateNotEqualsFunction(typeof(T));

        public static bool CompareNotEqualsManual(T x, T y)
        {
            bool firstCall = Interlocked.CompareExchange(ref CompareNotEqualManuallyCalledFlag, 1, 0) == 0;
#if DEBUG
            firstCall = true;
#endif

            if (firstCall) { CheckForIEquatableFuckup(typeof(T)); }
            return CompareNotEquals(x, y);
        }

        private static void CheckForIEquatableFuckup(Type type)
        {
            Type equatableInterface = typeof(IEquatable<>).MakeGenericType(type);
            bool hasInterface = type.GetInterfaces().Any(x => x == equatableInterface);
            bool hasAttribute = type.GetCustomAttributes<EquatableDependsOnLazyComparison>().Any();

            if (!hasInterface && hasAttribute)
            {
                throw new InvalidOperationException(
                    $"Type {type} has attribute {typeof(EquatableDependsOnLazyComparison)}" +
                    $" without implementing the {equatableInterface} interface."
                );
            }

            if (hasInterface && !hasAttribute)
            {
                throw new InvalidOperationException(
                    $"Type {type} cannot make use of lazy comparison for the implementation of " +
                    $"{equatableInterface} without having the {typeof(EquatableDependsOnLazyComparison)} attribute."
                );
            }
        }

        public static bool CompareNotEquals(T x, T y)
        {
            if (ReferenceEquals(x, y)) { return false; }
            if (ReferenceEquals(x, null) != ReferenceEquals(y, null)) { return true; }//one is null
            bool result = NotEqualsFunction.Invoke(x, y);
#if DEBUG
            Debug.WriteLine("not equals of");
            Debug.IndentLevel++;
            Debug.WriteLine(x.ToString());
            Debug.WriteLine(y.ToString());
            Debug.WriteLine($"is {result}.");
            Debug.IndentLevel--;
#endif
            return result;
        }

        private static EqualityFunction<T> GenerateNotEqualsFunction(Type type)
        {
            EqualityFunction<T> result = null;

            if (!(
                TryGenerateForPrimitive(type, ref result) ||
                TryGenerateForIEquatable(type, ref result) ||
                TryGenerateForEnumerable(type, ref result) ||
                TryGenerateForOldEnumerable(type, ref result) ||
                TryGenerateMemberwise(type, ref result)
            ))
            {
                throw new NotImplementedException("Could not handle lazy comparison.");
            }

            return result;
        }

        private static bool TryGenerateForOldEnumerable(Type type, ref EqualityFunction<T> result)
        {
            if (type.CustomAttributes.OfType<NotEnumerableComparisonAttribute>().Any()) { return false; }

            Type enumerableInterface =
                type.GetInterfaces()
                .Concat(new[] { type })//this one handles the case where type is IEnumerable itself
                .Where(x => x == typeof(IEnumerable))
                .FirstOrDefault();

            if (enumerableInterface == null) { return false; }

            ParameterExpression left = Expression.Parameter(type, "x");
            ParameterExpression right = Expression.Parameter(type, "y");

            result = Expression.Lambda<EqualityFunction<T>>(
                    Expression.Call(
                    instance: null,
                    method: NotEquals.GetSequenceNotEqualMethod(type),
                    arguments: new[] { left, right }
                ),
                parameters: new[] { left, right }
            ).Compile();
            return true;
        }

        private static bool TryGenerateForIEquatable(Type type, ref EqualityFunction<T> result)
        {

            Type equatableInterface = typeof(IEquatable<>).MakeGenericType(type);
            bool implements = type.GetInterfaces().Any(x => x == equatableInterface);
            if (!implements) { return false; }

            if (type.GetCustomAttributes<EquatableDependsOnLazyComparison>().Any()) { return false; }

            ParameterExpression left = Expression.Parameter(type, "x");
            ParameterExpression right = Expression.Parameter(type, "y");

            MethodInfo compareMethod = NotEquals.GetEquatableCompareMethod(type);

            result = Expression.Lambda<EqualityFunction<T>>(
                Expression.Call(
                    instance: null,
                    method: NotEquals.GetEquatableCompareMethod(type),
                    arguments: new[] { left, right }
                ),
                parameters: new[] { left, right }
            ).Compile();

            return true;
        }

        private static bool TryGenerateForPrimitive(Type type, ref EqualityFunction<T> result)
        {
            result = DefaultNotEquals;
            return type.IsPrimitive || type.IsEnum;
        }

        private static bool TryGenerateMemberwise(Type type, ref EqualityFunction<T> result)
        {
            PropertyInfo[] properties = type.GetNonEqualityExcludedProperties();
            FieldInfo[] fields = type.GetEqualityIncludedFields();

            ParameterExpression left = Expression.Parameter(type, nameof(left));
            ParameterExpression right = Expression.Parameter(type, nameof(right));

            result = Expression.Lambda<EqualityFunction<T>>(
                properties.Aggregate(
                    fields.Aggregate(
                        Expression.Constant(false) as Expression,
                        (expression, field) =>
                            Expression.OrElse(
                                expression,
                                Expression.Call(
                                    instance: null,
                                    method: GetNotEqualMethod(field.FieldType),
                                    arguments: new[] {
                                        Expression.Field(left, field),
                                        Expression.Field(right, field)
                                    }
                                )
                            )
                    ),
                    (expression, property) =>
                        Expression.OrElse(
                        expression,
                        Expression.Call(
                            instance: null,
                            method: GetNotEqualMethod(property.PropertyType),
                            arguments: new[] {
                                Expression.Property(left, property),
                                Expression.Property(right, property)
                            }
                        )
                    )
                ),
                parameters: new[] { left, right }
            ).Compile();

            return true;
        }

        private static bool TryGenerateForEnumerable(Type type, ref EqualityFunction<T> result)
        {
            if (type.CustomAttributes.OfType<NotEnumerableComparisonAttribute>().Any()) { return false; }

            Type enumerableInterface =
                type.GetInterfaces()
                .Concat(new[] { type })//this one handles the case where type is IEnumerable itself
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .FirstOrDefault();

            if (enumerableInterface == null) { return false; }

            Type itemType = enumerableInterface.GetGenericArguments().Single();

            ParameterExpression left = Expression.Parameter(type, "x");
            ParameterExpression right = Expression.Parameter(type, "y");

            result = Expression.Lambda<EqualityFunction<T>>(
                Expression.Call(
                    instance: null,
                    method: NotEquals.GetSequenceNotEqualMethod(itemType, type),
                    arguments: new[] { left, right }
                ),
                left,
                right
            ).Compile();
            return true;
        }

        private static bool DefaultNotEquals(T x, T y) => !x.Equals(y);

        private static MethodInfo GetNotEqualMethod(Type GenericParameter) =>
            typeof(NotEquals<>).MakeGenericType(GenericParameter).GetMethod(nameof(CompareNotEquals));
    }

    internal static class NotEquals
    {
        const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

        public static MethodInfo GetSequenceNotEqualMethod(Type tItem, Type tCollection) =>
            typeof(NotEquals).GetMethod(nameof(SequenceNotEqual), PublicStatic).MakeGenericMethod(tItem, tCollection);

        public static MethodInfo GetEquatableCompareMethod(this Type type) =>
            typeof(NotEquals).GetMethod(nameof(EquatableCompareNotEquals), PublicStatic).MakeGenericMethod(type);

        public static MethodInfo GetSequenceNotEqualMethod(Type tCollection) =>
            typeof(NotEquals).GetMethod(nameof(ObjectSequenceNotEqual), PublicStatic).MakeGenericMethod(tCollection);

        public static bool EquatableCompareNotEquals<T>(T x, T y) where T : IEquatable<T>
        {
            return !(x as IEquatable<T>).Equals(y);
        }

        public static bool ObjectSequenceNotEqual<TCollection>(TCollection x, TCollection y) where TCollection : IEnumerable
        {
            if (ReferenceEquals(x, y)) { return false; }
            if (ReferenceEquals(x, null) != ReferenceEquals(y, null)) { return true; }

            int xLen = (x as ICollection)?.Count ?? 0;
            int yLen = (x as ICollection)?.Count ?? 0;

            if (xLen != yLen) { return true; }

            IEnumerator xEnu = x.GetEnumerator();
            IEnumerator yEnu = y.GetEnumerator();

            for (; ; )
            {
                bool moveX = xEnu.MoveNext();
                bool moveY = yEnu.MoveNext();

                if (moveX != moveY) { return true; }
                if (!moveX) { return false; }
                //if (NotEquals<TItem>.CompareNotEquals(xEnu.Current, yEnu.Current)) { return true; }
                if (!xEnu.Current.Equals(yEnu.Current)) { return true; }
            }
        }


        public static bool SequenceNotEqual<TItem, TCollection>(TCollection x, TCollection y) where TCollection : IEnumerable<TItem>
        {
            if (ReferenceEquals(x, y)) { return false; }
            if (ReferenceEquals(x, null) != ReferenceEquals(y, null)) { return true; }

            int xLen = (x as IReadOnlyCollection<TItem>)?.Count ?? 0;
            int yLen = (x as IReadOnlyCollection<TItem>)?.Count ?? 0;

            if (xLen != yLen) { return true; }

            IEnumerator<TItem> xEnu = x.GetEnumerator();
            IEnumerator<TItem> yEnu = y.GetEnumerator();

            for (; ; )
            {
                bool moveX = xEnu.MoveNext();
                bool moveY = yEnu.MoveNext();

                if (moveX != moveY) { return true; }
                if (!moveX) { return false; }
                if (NotEquals<TItem>.CompareNotEquals(xEnu.Current, yEnu.Current)) { return true; }
            }
        }

        internal static FieldInfo[] GetEqualityIncludedFields(this Type type)
        {
            return
                type
                .GetRuntimeFields()
                .Where(x =>
                    x.GetCustomAttributes<EqualityIncludeAttribute>().Any()
                )
                .ToArray();
        }

        internal static PropertyInfo[] GetNonEqualityExcludedProperties(this Type type)
        {
            return
                type
                .GetRuntimeProperties()
                .Where(x =>
                    x.CanRead &&
                    !x.GetCustomAttributes<EqualityExcludeAttribute>().Any()
                )
                .ToArray();
        }
    }
}
