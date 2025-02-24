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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Imview.Core.Common.Utils;

/// <summary>
/// Utilities for converting between human-readable and system names.
/// </summary>
public static partial class HumanizeUtils {

    private static readonly Regex s_propertyPrefixRegex = PropertyPrefixRegex();
    private static readonly Regex s_capitalLettersRegex = CapitalLettersRegex();

    [GeneratedRegex(@"^m_", RegexOptions.Compiled)]
    private static partial Regex PropertyPrefixRegex();
    
    [GeneratedRegex(@"([A-Z])", RegexOptions.Compiled)]
    private static partial Regex CapitalLettersRegex();

    /// <summary>
    /// Converts a property name to a human-readable format.
    /// </summary>
    public static string ToHumanizedName(this string propertyName) {
        if (string.IsNullOrEmpty(propertyName)) {
            return string.Empty;
        }

        // Remove the "m_" prefix if it exists
        propertyName = s_propertyPrefixRegex.Replace(propertyName, "");

        // Capitalize the first letter
        propertyName = char.ToUpper(propertyName[0]) + propertyName[1..];

        // Add spaces between capital letters
        return s_capitalLettersRegex.Replace(propertyName, " $1").Trim();
    }

    /// <summary>
    /// Converts a human-readable name back to a property name format.
    /// </summary>
    public static string ToPropertyName(this string humanizedName) {
        if (string.IsNullOrEmpty(humanizedName)) {
            return string.Empty;
        }

        // Remove spaces and ensure proper casing
        var propertyName = string.Concat(
            humanizedName.Split(' ')
                        .Select(part => char.ToUpper(part[0]) + part[1..].ToLower())
        );

        // Convert first character to lowercase and add "m_" prefix
        return "m_" + char.ToLower(propertyName[0]) + propertyName[1..];
    }

    /// <summary>
    /// Splits a comma-separated string into a list, handling whitespace.
    /// </summary>
    public static List<string> SplitCommaSeparated(this string text) {
        if (string.IsNullOrEmpty(text)) {
            return [];
        }

        return text.Split(',')
                  .Select(s => s.Trim())
                  .Where(s => !string.IsNullOrEmpty(s))
                  .ToList();
    }

    /// <summary>
    /// Joins a list of items into a comma-separated string.
    /// </summary>
    public static string ToCommaSeparatedString<T>(this IEnumerable<T> items) 
        => items == null ? string.Empty : string.Join(", ", items);

}
