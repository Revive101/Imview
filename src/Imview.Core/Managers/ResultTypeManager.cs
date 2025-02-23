using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Imcodec.ObjectProperty.TypeCache;

namespace Imview.Core.Managers;

/// <summary>
/// Utility class for discovering and managing Result types in the application.
/// </summary>
public static class ResultTypeManager {

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
    /// Creates a new instance of a Result type by its name.
    /// </summary>
    /// <param name="typeName">The name of the Result type to create</param>
    /// <returns>A new instance of the specified Result type, or null if not found</returns>
    public static Result CreateResult(string typeName) {
        EnsureInitialized();

        if (s_resultTypes.TryGetValue(typeName, out var resultType)) {
            return (Result) Activator.CreateInstance(resultType);
        }

        return null;
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