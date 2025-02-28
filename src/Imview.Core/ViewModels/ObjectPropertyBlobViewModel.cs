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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Imcodec.ObjectProperty;
using Imview.Core.Services;
using ReactiveUI;

namespace Imview.Core.ViewModels;

public class ObjectPropertyBlobViewModel : ViewModelBase {
    
    private readonly MainWindowViewModel _mainViewModel;
    private string _hexBlob = string.Empty;
    private string _deserializedResult = string.Empty;
    private bool _isDeserializing;

    public ICommand DeserializeBlobCommand { get; }
    public ICommand BackToSplashCommand { get; }

    public string HexBlob {
        get => _hexBlob;
        set => this.RaiseAndSetIfChanged(ref _hexBlob, value);
    }

    public string DeserializedResult {
        get => _deserializedResult;
        set => this.RaiseAndSetIfChanged(ref _deserializedResult, value);
    }

    public bool IsDeserializing {
        get => _isDeserializing;
        set => this.RaiseAndSetIfChanged(ref _isDeserializing, value);
    }

    public ObjectPropertyBlobViewModel(MainWindowViewModel mainViewModel) {
        _mainViewModel = mainViewModel;
        DeserializeBlobCommand = ReactiveCommand.CreateFromTask(DeserializeBlob);
        BackToSplashCommand = ReactiveCommand.Create(BackToSplash);
    }

    private async Task DeserializeBlob() {
        try {
            IsDeserializing = true;
            DeserializedResult = string.Empty;
            var cleanedHexBlob = HexBlob
                .Replace(" ", "")
                .Replace("\n", "")
                .Replace("\r", "");

            var bytes = Convert.FromHexString(cleanedHexBlob);

            var bindSerializer = new BindSerializer();
            if (bindSerializer.Deserialize<PropertyClass>(bytes, out var propertyClass)) {
                // Create wrapper object with metadata.
                var deserializedObject = new {
                    _flags = (uint)bindSerializer.SerializerFlags,
                    _className = propertyClass.GetType().Name,
                    _hash = propertyClass.GetHash(),
                    _deserializedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    _imcodecVersion = typeof(ObjectPropertyBlobViewModel).Assembly.GetName()?.Version?.ToString() ?? "Unknown",
                    _object = propertyClass
                };

                DeserializedResult = JsonSerializer.Serialize(deserializedObject, new JsonSerializerOptions {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                MessageService.Info("Blob deserialized successfully!")
                    .WithDuration(TimeSpan.FromSeconds(3))
                    .Send();
            }
            else {
                MessageService.Error("Failed to deserialize the blob.")
                    .WithDuration(TimeSpan.FromSeconds(3))
                    .Send();
            }
        }
        catch (Exception ex) {
            MessageService.Error($"Error during deserialization: {ex.Message}")
                .WithDuration(TimeSpan.FromSeconds(5))
                .Send();
        }
        finally {
            IsDeserializing = false;
        }
    }

    private void BackToSplash() 
        => _mainViewModel.ReturnToSplash();

}