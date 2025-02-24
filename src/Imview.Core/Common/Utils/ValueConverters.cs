/*
BSD 3-Clause License

Copyright (c) 2024, Jooty

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.
*/

using System;
using System.Collections.Generic;
using Imcodec.IO;
using Imcodec.Types;
using Imview.Core.Common.Extensions;

namespace Imview.Core.Common.Utils;

/// <summary>
/// Utility class for converting between different value types.
/// </summary>
public static class ValueConverters {

    private static readonly Dictionary<Type, Func<string, object>> s_converters = new() {
        { typeof(byte), s => byte.Parse(s) },
        { typeof(sbyte), s => sbyte.Parse(s) },
        { typeof(short), s => short.Parse(s) },
        { typeof(ushort), s => ushort.Parse(s) },
        { typeof(int), s => int.Parse(s) },
        { typeof(uint), s => uint.Parse(s) },
        { typeof(long), s => long.Parse(s) },
        { typeof(ulong), s => ulong.Parse(s) },
        { typeof(float), s => float.Parse(s) },
        { typeof(double), s => double.Parse(s) },
        { typeof(bool), s => bool.Parse(s) },
        { typeof(string), s => s },
        { typeof(ByteString), s => new ByteString(s) },
        { typeof(WideByteString), s => new WideByteString(s) },
        { typeof(GID), s => new GID(ulong.Parse(s)) }
    };

    /// <summary>
    /// Converts a string value to the specified type.
    /// </summary>
    public static object? ConvertValue(string value, Type targetType) {
        if (string.IsNullOrEmpty(value)) {
            return null;
        }

        if (targetType.IsEnum) {
            return Enum.Parse(targetType, value);
        }

        if (s_converters.TryGetValue(targetType, out var converter)) {
            return converter(value);
        }

        if (targetType.IsCollectionType()) {
            var elementType = targetType.GetCollectionElementType();
            var items = value.SplitCommaSeparated();
            if (elementType == null) {
                throw new ArgumentException("Element type cannot be null.");
            }
            var list = (IList<object>?) Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) 
                       ?? throw new InvalidOperationException("Failed to create list instance.");

            foreach (var item in items) {
                var convertedItem = ConvertValue(item, elementType);
                if (convertedItem != null) {
                    list.Add(convertedItem);
                }
            }

            return list;
        }

        throw new ArgumentException($"Unsupported type: {targetType.Name}");
    }

    /// <summary>
    /// Gets type-specific validation hints.
    /// </summary>
    public static string GetTypeValidationHint(Type type) 
        => type.Name switch {
            nameof(Byte) => $"{byte.MinValue} to {byte.MaxValue}",
            nameof(SByte) => $"{sbyte.MinValue} to {sbyte.MaxValue}",
            nameof(Int16) => $"{short.MinValue} to {short.MaxValue}",
            nameof(UInt16) => $"{ushort.MinValue} to {ushort.MaxValue}",
            nameof(Int32) => $"{int.MinValue} to {int.MaxValue}",
            nameof(UInt32) => $"{uint.MinValue} to {uint.MaxValue}",
            nameof(Int64) => $"{long.MinValue} to {long.MaxValue}",
            nameof(UInt64) => $"{ulong.MinValue} to {ulong.MaxValue}",
            nameof(Single) => "Enter a decimal number (e.g., 3.14)",
            nameof(Double) => "Enter a decimal number with up to 15-17 significant digits",
            nameof(String) or nameof(ByteString) or nameof(WideByteString) => "Enter text",
            nameof(GID) => "Template ID number",
            _ => type.IsCollectionType() ? "Enter values separated by commas" : string.Empty
    };

}