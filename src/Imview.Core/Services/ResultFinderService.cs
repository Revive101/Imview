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
using Imcodec.ObjectProperty.TypeCache;

namespace Imview.Core.Services;

/// <summary>
/// Utility class for discovering and managing Result types in the application.
/// </summary>
public static class ResultFinderService {

    private static readonly Dictionary<string, System.Type> s_resultTypes = [];
    private static bool s_isInitialized;

    /// <summary>
    /// Gets all available Result types.
    /// </summary>
    public static IReadOnlyDictionary<string, System.Type> ResultTypes {
        get {
            EnsureInitialized();
            return s_resultTypes;
        }
    }

    /// <summary>
    /// Initialize the Result type cache. This method is thread-safe and ensures
    /// initialization happens only once.
    /// </summary>
    public static void Initialize() {
        if (s_isInitialized) {
            return;
        }

        lock (s_resultTypes) {
            if (s_isInitialized) {
                return;
            }

            DiscoverResultTypes();
            s_isInitialized = true;
        }
    }
    
    /// <summary>
    /// Gets a Result type by its name.
    /// </summary>
    /// <param name="typeName">The name of the Result type</param>
    /// <returns>The Type of the Result, or null if not found</returns>
    public static System.Type? GetResultType(string typeName) {
        EnsureInitialized();

        s_resultTypes.TryGetValue(typeName, out var resultType);

        return resultType;
    }

    private static void EnsureInitialized() {
        if (!s_isInitialized) {
            Initialize();
        }
    }

    private static void DiscoverResultTypes() {
        s_resultTypes.Clear();

        // Get all types from the current assembly and any referenced assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location));

        foreach (var assembly in assemblies) {
            try {
                var types = assembly.GetTypes()
                    .Where(t => typeof(Result).IsAssignableFrom(t) &&
                               !t.IsAbstract &&
                               t.Namespace == typeof(Result).Namespace);

                foreach (var type in types) {
                    s_resultTypes[type.Name] = type;
                }
            }
            catch (ReflectionTypeLoadException ex) {
                Console.WriteLine($"Error loading types from assembly {assembly.FullName}: {ex.Message}");
                // Log the loader exceptions if needed
                foreach (var loaderException in ex.LoaderExceptions) {
                    Console.WriteLine($"Loader Exception: {loaderException?.Message}");
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error processing assembly {assembly.FullName}: {ex.Message}");
            }
        }
    }

}