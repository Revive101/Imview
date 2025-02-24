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

using System.Reflection;
using System.ComponentModel;
using System;
using Imview.Core.Common.Utils;

namespace Imview.Core.Common.Extensions;

/// <summary>
/// Extension methods for working with PropertyInfo objects.
/// </summary>
public static class PropertyInfoExtensions {
    
    /// <summary>
    /// Gets the display name for a property, either from its DisplayName attribute or by humanizing the property name.
    /// </summary>
    public static string GetDisplayName(this PropertyInfo property) {
        var attribute = property.GetCustomAttribute<DisplayNameAttribute>();
        if (attribute != null) {
            return attribute.DisplayName;
        }

        return property.Name.ToHumanizedName();
    }

    /// <summary>
    /// Determines if a property should be displayed in the editor.
    /// </summary>
    public static bool ShouldDisplay(this PropertyInfo property) 
        => property.CanWrite &&
            !property.Name.StartsWith("_") &&
             property.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false;

    /// <summary>
    /// Gets the type-specific editor for a property, if one exists.
    /// </summary>
    public static Type? GetEditorType(this PropertyInfo property) {
        var attribute = property.GetCustomAttribute<EditorAttribute>();
        return attribute != null ? Type.GetType(attribute.EditorTypeName) : null;
    }

}