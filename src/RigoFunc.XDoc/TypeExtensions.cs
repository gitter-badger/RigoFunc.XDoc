// Copyright (c) RigoFunc (xuyingting). All rights reserved.

using System;
using System.Reflection;

namespace RigoFunc.XDoc
{
    /// <summary>
    /// The extension methods for <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the unwrap nullalble type if the the <paramref name="type"/> is nullable type or the type self.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type UnwrapNullableType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;

        /// <summary>
        /// Determines the specified type is primitive type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsPrimitive(this Type type) => type.IsInteger() || type.IsNonIntegerPrimitive();

        /// <summary>
        /// Determines the specified type is integer type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsInteger(this Type type)
        {
            type = type.UnwrapNullableType();

            return (type == typeof(int))
                   || (type == typeof(long))
                   || (type == typeof(short))
                   || (type == typeof(byte))
                   || (type == typeof(uint))
                   || (type == typeof(ulong))
                   || (type == typeof(ushort))
                   || (type == typeof(sbyte))
                   || (type == typeof(char));
        }

        private static bool IsNonIntegerPrimitive(this Type type)
        {
            type = type.UnwrapNullableType();

            return (type == typeof(bool))
                   || (type == typeof(byte[]))
                   || (type == typeof(DateTime))
                   || (type == typeof(DateTimeOffset))
                   || (type == typeof(decimal))
                   || (type == typeof(double))
                   || (type == typeof(float))
                   || (type == typeof(Guid))
                   || (type == typeof(string))
                   || (type == typeof(TimeSpan))
                   || type.GetTypeInfo().IsEnum;
        }
    }
}
