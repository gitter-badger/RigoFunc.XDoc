// Copyright (c) RigoFunc (xuyingting). All rights reserved.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Newtonsoft.Json.Linq;

namespace RigoFunc.XDoc {
    /// <summary>
    /// Provides extension methods for reading XML comments from reflected members.
    /// </summary>
    public static class DocExtension {
        /// <summary>
        /// Gets the specified type specified property's XML documentation.
        /// </summary>
        /// <typeparam name="T">The type declares the property.</typeparam>
        /// <typeparam name="P">The property expression to get XML documentation.</typeparam>
        /// <param name="type">The specified type instance.</param>
        /// <param name="property">The specified property.</param>
        /// <returns>The XML comments.</returns>
        public static string GetPropXDoc<T, P>(this T type, Expression<Func<T, P>> property) where T : class => (property.Body as MemberExpression)?.Member?.XmlDoc();

        /// <summary>
        /// Gets the specified enum value's XML documentation.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <param name="withRawValue"><c>True</c> if need to get the enum underlying raw constant value.</param>
        /// <returns>The XML comments or Json style string if <paramref name="withRawValue"/> set to <c>true</c>.</returns>
        public static string GetEnumXDoc(this Enum value, bool withRawValue = false) {
            var typeInfo = value.GetType().UnwrapNullableType().GetTypeInfo();
            if (!typeInfo.IsEnum) {
                throw new ArgumentException($"{nameof(value)} must be a Enum", nameof(value));
            }

            var field = typeInfo.GetField(value.ToString());
            if(field == null) {
                return value.ToString();
            }

            var xml = field.XmlDoc() ?? field.Name;

            if (withRawValue) {
                var json = new JObject();

                json.Add(field.GetRawConstantValue().ToString(), xml);

                return json.ToString();
            }

            return xml;
        }

        /// <summary>
        /// Gets the specified XML comments.
        /// </summary>
        /// <param name="type">The instance of the specified type.</param>
        /// <returns>The XML comments represents by Json object.</returns>
        public static JObject GetXDoc(this Type type) {
            type = type.UnwrapNullableType();

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsEnum) {
                return type.GetEnumXDoc();
            }

            var json = new JObject();
            var properties = typeInfo.DeclaredProperties;
            foreach (var prop in properties) {
                var xml = prop.XmlDoc() ?? prop.Name;

                if (prop.PropertyType.IsPrimitive()) {
                    var propertyType = prop.PropertyType.UnwrapNullableType();
                    //var isNullable = propertyType != prop.PropertyType;
                    //if (isNullable) {
                    //    json.WriteComments("optional parameter");
                    //    json.WriteLine(IndentStyle.Same);
                    //}

                    if (propertyType.GetTypeInfo().IsEnum) {
                        json.Add(prop.Name, propertyType.GetEnumXDoc());
                    }
                    else {
                        json.Add(prop.Name, xml);
                    }
                }
                else if (prop.PropertyType.GetTypeInfo().IsGenericType) {
                    var jarray = new JArray();
                    var elementType = prop.PropertyType.GenericTypeArguments[0];
                    if (elementType.IsPrimitive()) {
                        var inner = new JObject();
                        inner.Add(prop.Name, xml);
                        jarray.Add(inner);
                    }
                    else {
                        jarray.Add(elementType.GetXDoc());
                        json.Add(prop.Name, jarray);
                    }
                }
                else if (prop.PropertyType.GetTypeInfo().IsClass) {
                    json.Add(prop.Name, prop.PropertyType.GetXDoc());
                }
            }

            return json;
        }

        /// <summary>
        /// Gets the specified enum type XML comments.
        /// </summary>
        /// <param name="enumType">The type of the specified enum</param>
        /// <returns>The XML comments represents by a Json object.</returns>
        public static JObject GetEnumXDoc(this Type enumType) {
            var typeInfo = enumType.UnwrapNullableType().GetTypeInfo();
            if (!typeInfo.IsEnum) {
                throw new ArgumentException($"{nameof(enumType)} must be a Enum", nameof(enumType));
            }

            var json = new JObject();

            // why: remove the "value__"
            var fields = typeInfo.DeclaredFields.Where(f => !f.IsSpecialName).ToArray();
            foreach (var field in fields) {
                var xml = field.XmlDoc() ?? field.Name;
                json.Add(field.GetRawConstantValue().ToString(), xml);
            }

            return json;
        }

        /// <summary>
        /// Gets the enum value by the specified XML comments.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="xDoc">The XML comments</param>
        /// <returns>The enum value which's XML comments is <paramref name="xDoc"/>.</returns>
        public static T GetEnumFromXDoc<T>(this string xDoc) {
            if (string.IsNullOrEmpty(xDoc)) {
                throw new ArgumentException($"The {nameof(xDoc)} cannot be null or enmty", nameof(xDoc));
            }

            var typeInfo = typeof(T).UnwrapNullableType().GetTypeInfo();
            if (!typeInfo.IsEnum) {
                throw new InvalidOperationException($"The type of T must be Enum");
            }

            foreach (var field in typeInfo.GetFields()) {
                var xml = field.XmlDoc();
                if (!string.IsNullOrEmpty(xml)) {
                    if (xml == xDoc) {
                        return (T)field.GetValue(null);
                    }
                }
                else {
                    if (field.Name == xDoc) {
                        return (T)field.GetValue(null);
                    }
                }
            }

            throw new ArgumentException($"cannot find the value from {xDoc}", nameof(xDoc));
        }

        /// <summary>
        /// Gets the enum value by the specified XML comments
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="xDoc">The XML comments</param>
        /// <param name="defaultValue">The default value if not found.</param>
        /// <returns>The enum value which's XML comments is <paramref name="xDoc"/>.</returns>
        public static T GetEnumFromXDoc<T>(this string xDoc, T defaultValue) {
            if (string.IsNullOrEmpty(xDoc)) {
                return defaultValue;
            }

            var typeInfo = typeof(T).UnwrapNullableType().GetTypeInfo();
            if (!typeInfo.IsEnum) {
                throw new InvalidOperationException($"The type of T must be Enum");
            }

            foreach (var field in typeInfo.GetFields()) {
                var xml = field.XmlDoc();
                if (!string.IsNullOrEmpty(xml)) {
                    if (xml == xDoc) {
                        return (T)field.GetValue(null);
                    }
                }
                else {
                    if (field.Name == xDoc) {
                        return (T)field.GetValue(null);
                    }
                }
            }

            return defaultValue;
        }
    }
}
