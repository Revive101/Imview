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
using System.Linq;
using System.Reflection;

namespace Imview.Core.Common.Extensions;

/// <summary>
/// Extension methods for working with Type objects.
/// </summary>
public static class TypeExtensions {

    /// <summary>
    /// Gets all editable properties for a type.
    /// </summary>
    public static IEnumerable<PropertyInfo> GetEditableProperties(this Type type) 
        => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                  .Where(p => p.ShouldDisplay());

    /// <summary>
    /// Checks if a type is a collection type.
    /// </summary>
    public static bool IsCollectionType(this Type type) 
        => type.IsGenericType &&
              (typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()) ||
               typeof(ICollection<>).IsAssignableFrom(type.GetGenericTypeDefinition()));

    /// <summary>
    /// Gets the element type if this is a collection type.
    /// </summary>
    public static Type? GetCollectionElementType(this Type type) 
        => type.IsGenericType ? type.GetGenericArguments()[0] : null;

}