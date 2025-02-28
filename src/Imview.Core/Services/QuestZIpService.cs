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
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imview.Core.Services;

namespace Imview.Core.Services;

/// <summary>
/// Service for saving multiple quest templates to a zip file.
/// </summary>
public static class QuestZipService {

    /// <summary>
    /// Saves multiple quest templates to a zip file.
    /// </summary>
    /// <param name="templates">The collection of templates to save</param>
    /// <param name="parentWindow">The parent window for the save dialog</param>
    /// <returns>A task that completes when the save operation is done, with a bool indicating success</returns>
    public static async Task<bool> SaveQuestsToZipFileAsync(
        IEnumerable<QuestTemplate> templates, 
        Avalonia.Controls.Window parentWindow) {

        ArgumentNullException.ThrowIfNull(templates);

        // Create temporary directory for .view files.
        var tempDir = Path.Combine(Path.GetTempPath(), $"Imview_Export_{Guid.NewGuid()}");
        var tempFiles = new List<string>();

        try {
            // Create save file dialog for zip file.
            var options = new FilePickerSaveOptions {
                Title = "Save All Quest Templates",
                SuggestedFileName = $"Quest_Export_{DateTime.Now:yyyyMMdd}.zip",
                FileTypeChoices = [
                    new("Zip Files") {
                        Patterns = ["*.zip"]
                    }
                ]
            };

            var zipResult = await parentWindow.StorageProvider.SaveFilePickerAsync(options);
            if (zipResult == null) {
                // User cancelled.
                return false;
            }

            var zipPath = zipResult.Path.LocalPath;

            _ = Directory.CreateDirectory(tempDir);

            // Serialize each template to a .view file in the temp directory.
            var serializer = new ObjectSerializer(false);
            var questCounter = 0;
            
            foreach (var template in templates) {
                questCounter++;
                var questName = SanitizeFileName(template.m_questName?.ToString() ?? 
                                                template.m_questTitle?.ToString() ?? 
                                                $"Quest_{questCounter}");
                
                var tempFilePath = Path.Combine(tempDir, $"{questName}.view");
                tempFiles.Add(tempFilePath);
                
                // Serialize the template.
                if (!serializer.Serialize(template, 1, out var data)) {
                    MessageService.Error($"Failed to serialize template: {questName}")
                        .WithDuration(TimeSpan.FromSeconds(3))
                        .Send();

                    continue;
                }
                
                if (data is null) {
                    MessageService.Error($"Serialized data is null for template: {questName}")
                        .WithDuration(TimeSpan.FromSeconds(3))
                        .Send();

                    continue;
                }
                
                // Write the serialized data to the temp file.
                await File.WriteAllBytesAsync(tempFilePath, data);
            }
            
            // Create the zip file.
            if (File.Exists(zipPath)) {
                File.Delete(zipPath);
            }
            
            ZipFile.CreateFromDirectory(tempDir, zipPath);
            
            return true;
        }
        catch (Exception ex) {
            MessageService.Error($"Failed to save templates to zip file: {ex.Message}")
                .WithDuration(TimeSpan.FromSeconds(5))
                .Send();
            return false;
        }
        finally {
            // Clean up temp files.
            foreach (var file in tempFiles) {
                try {
                    if (File.Exists(file)) {
                        File.Delete(file);
                    }
                }
                catch {
                    // Ignore clean-up errors.
                }
            }
            
            // Clean up temp directory.
            try {
                if (Directory.Exists(tempDir)) {
                    Directory.Delete(tempDir, true);
                }
            }
            catch {
                // Ignore clean-up errors.
            }
        }
    }
    
    /// <summary>
    /// Sanitizes a file name by removing invalid characters.
    /// </summary>
    private static string SanitizeFileName(string fileName) {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries))
            .Replace(" ", "_");
    }
    
}