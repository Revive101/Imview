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
using System.Text.Json.Nodes;
using System.Linq;
using System.Text.Json;
using System.Reflection;
using Imcodec.IO;

namespace Imview.PacketReader.Services;

/// <summary>
/// Service for reading and parsing packet capture files with generic mapping.
/// </summary>
public class PacketReaderService {
    
    /// <summary>
    /// Mapping attribute to specify how a property should be extracted from a JSON node.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PacketFieldAttribute(string fieldName, ExtractMethod method = ExtractMethod.Default) : Attribute {

        public string FieldName { get; } = fieldName;
        public ExtractMethod Method { get; } = method;

    }

    public enum ExtractMethod {
        Default,
        ASCII,
        Hex,
        Gid,
        Ubyte,
        Int,
    }

    /// <summary>
    /// Extracts packets of a specific type from a JSON file.
    /// </summary>
    /// <typeparam name="T">The type of packet to extract</typeparam>
    /// <param name="filePath">Path to the JSON packet capture file</param>
    /// <param name="packetName">The name of the packet to filter</param>
    /// <returns>A list of extracted packets</returns>
    public static async Task<List<T>> ExtractPacketsAsync<T>(
        string filePath, 
        string packetName) where T : class, new() {
        
        if (string.IsNullOrEmpty(filePath)) {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");
        }

        if (!File.Exists(filePath)) {
            throw new FileNotFoundException("Packet capture file not found.", filePath);
        }

        try {
            var jsonText = await File.ReadAllTextAsync(filePath);
            var jsonNode = JsonNode.Parse(jsonText);
            
            if (jsonNode == null) {
                return [];
            }

            var packets = jsonNode is JsonArray 
                ? jsonNode.AsArray() 
                : new JsonArray(jsonNode);

            return [.. packets
                .Where(packet => IsMatchingPacket(packet, packetName))
                .Select(MapPacket<T>)];
        }
        catch (JsonException ex) {
            throw new InvalidOperationException("Invalid JSON format in packet capture file.", ex);
        }
        catch (Exception ex) {
            throw new InvalidOperationException("Error reading packet capture file.", ex);
        }
    }

    private static bool IsMatchingPacket(JsonNode? packetNode, string packetName) 
        => packetNode?["data"]?["name"]?.GetValue<string>() == packetName;

    private static T MapPacket<T>(JsonNode? packetNode) where T : class, new() {
        if (packetNode == null) {
            throw new ArgumentNullException(nameof(packetNode), "Packet node cannot be null.");
        }

        var fields = packetNode["data"]?["fields"] ?? throw new InvalidOperationException("Invalid packet structure.");
        var result = new T();

        // Get all properties with PacketFieldAttribute.
        var properties = typeof(T).GetProperties()
            .Where(p => p.GetCustomAttribute<PacketFieldAttribute>() != null);

        foreach (var prop in properties) {
            var attribute = prop.GetCustomAttribute<PacketFieldAttribute>();
            if (attribute == null) {
                continue;
            }

            var fieldNode = fields[attribute.FieldName];
            var value = attribute.Method switch {
                ExtractMethod.ASCII => GetStringValue(fieldNode),
                ExtractMethod.Gid => GetGidValue(fieldNode),
                ExtractMethod.Hex => GetHexValue(fieldNode),
                ExtractMethod.Ubyte => GetUbyteValue(fieldNode),
                ExtractMethod.Int => GetIntValue(fieldNode),
                _ => GetDefaultValue(fieldNode, prop.PropertyType)
            };

            prop.SetValue(result, value);
        }

        // Special handling for timestamp.
        var timestampProp = typeof(T).GetProperties()
            .FirstOrDefault(p => p.Name.Equals("Timestamp", StringComparison.OrdinalIgnoreCase));
        
        timestampProp?.SetValue(result, 
            packetNode["timestamp"]?.GetValue<DateTime>() ?? DateTime.MinValue);

        return result;
    }

    private static object? GetDefaultValue(JsonNode? node, Type targetType) {
        if (node == null) {
            return GetDefaultValue(targetType);
        }

        try {
            var value = node["value"];
            
            // Handle nullable types.
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            return value?.GetValue<object>() != null 
                ? Convert.ChangeType(value.GetValue<object>(), underlyingType) 
                : GetDefaultValue(targetType);
        }
        catch {
            return GetDefaultValue(targetType);
        }
    }

    private static object? GetDefaultValue(Type type) 
        => type.IsValueType ? Activator.CreateInstance(type) : null;

    private static string GetStringValue(JsonNode? node) 
        => node?["value"]?.GetValue<string>() ?? string.Empty;

    private static ulong GetGidValue(JsonNode? node) 
        => node?["value"]?.GetValue<ulong>() ?? 0;

    private static string GetHexValue(JsonNode? node) 
        => node?["value"]?.GetValue<string>() ?? string.Empty;

    private static byte GetUbyteValue(JsonNode? node) 
        => node?["value"]?.GetValue<byte>() ?? 0;

    private static int GetIntValue(JsonNode? node) 
        => node?["value"]?.GetValue<int>() ?? 0;

}