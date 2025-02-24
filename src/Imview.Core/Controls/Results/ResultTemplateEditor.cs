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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Imcodec.IO;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Types;
using Imview.Core.Common.Constants;
using Imview.Core.Common.Extensions;
using Imview.Core.Common.Utils;
using Imview.Core.Controls.Base;
using Imview.Core.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Imview.Core.Controls.Results;

/// <summary>
/// Editor for Result templates used in quest goals.
/// </summary>
public class ResultTemplateEditor : EditorWindowBase<Result> {

    // Core properties.
    private readonly Result _template;
    private readonly Dictionary<string, Control> _propertyEditors = [];
    
    // UI Controls.
    private readonly ComboBox _typeSelector;
    private readonly StackPanel _propertyPanel;
    private string _selectedResultType;
    
    // Services.
    private readonly IReadOnlyDictionary<string, System.Type> _resultTypes;

    public ResultTemplateEditor(Result? template = null, IReadOnlyDictionary<string, System.Type>? resultTypes = null) 
        : base(template is not null ? "Edit Result" : "Add Result") {
        
        _template = template ?? new Result();
        _resultTypes = resultTypes ?? ResultFinderService.ResultTypes;
        _selectedResultType = string.Empty;
        
        Width = EditorConstants.SMALL_WINDOW_WIDTH;
        Height = EditorConstants.SMALL_WINDOW_HEIGHT;
        Background = EditorConstants.DEFAULT_WINDOW_BACKGROUND;
        
        _typeSelector = new ComboBox {
            Width = EditorConstants.DEFAULT_SELECTOR_WIDTH,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        // ? Bugfix: Virtualization causes the ComboBox to grow/shrink as items are shown/hidden.
        // https://github.com/AvaloniaUI/Avalonia/issues/11018#issuecomment-1510803095
        _typeSelector.ItemsPanel = new FuncTemplate<Panel?>(new(() => new StackPanel()));
        
        _propertyPanel = new StackPanel {
            Spacing = EditorConstants.DEFAULT_CONTROL_SPACING
        };
        
        InitializeComponent();
    }
    
    private void InitializeComponent() {
        // Populate type selector with result types.
        _typeSelector.ItemsSource = GetResultTypeNames();

        // If editing existing result, select its type.
        if (_template != null && _template.GetType() != typeof(Result)) {
            var typeName = _template.GetType().Name;
            _typeSelector.SelectedItem = typeName;
            _selectedResultType = typeName;
            _typeSelector.IsEnabled = false; // Don't allow type changes for existing results.
        }
        
        _typeSelector.SelectionChanged += TypeSelector_SelectionChanged;
        
        var typeSelectorContent = new StackPanel {
            Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
            Children = {
                new TextBlock { Text = "Select Result Type:" },
                _typeSelector
            }
        };
        
        MainPanel.Children.Add(CreateGroupBox("Result Type", typeSelectorContent));
        
        // Property panel.
        var propertyScroller = new ScrollViewer {
            Content = _propertyPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        
        MainPanel.Children.Add(CreateGroupBox("Properties", propertyScroller));
        MainPanel.Children.Add(CreateActionButtons(Save, Cancel));
        
        // If editing existing result, initialize properties.
        if (_template != null && _template.GetType() != typeof(Result)) {
            PopulatePropertyEditors(_template.GetType());
            InitializeValues();
        }
    }

    private IEnumerable<string> GetResultTypeNames() 
        => _resultTypes.Keys
            .Where(typeName => typeName.StartsWith("Res") && !typeName.StartsWith("Result"))
            .Select(typeName => typeName[3..])
            .OrderBy(t => t);

    private void TypeSelector_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (_typeSelector.SelectedItem is string typeName) {
            _selectedResultType = "Res" + typeName;
            var type = _resultTypes.TryGetValue(_selectedResultType, out var t) ? t : null;
            
            if (type != null) {
                PopulatePropertyEditors(type);
            }
        }
    }
    
    private void PopulatePropertyEditors(System.Type resultType) {
        _propertyPanel.Children.Clear();
        _propertyEditors.Clear();
        
        var properties = resultType.GetEditableProperties()
            .OrderBy(p => p.Name);
        
        foreach (var property in properties) {
            var editor = CreateEditorForProperty(property);
            _propertyPanel.Children.Add(editor);
        }
    }
    
    private Control CreateEditorForProperty(PropertyInfo property) {
        var displayName = property.GetDisplayName();
        var propertyType = property.PropertyType;
        
        Control? editor = null;
        
        // Create appropriate editor based on property type.
        if (propertyType == typeof(bool)) {
            editor = new CheckBox();
            _propertyEditors[property.Name] = editor;
        }
        else if (propertyType.IsEnum) {
            var comboBox = new ComboBox {
                ItemsSource = Enum.GetValues(propertyType)
            };
            _propertyEditors[property.Name] = comboBox;
            editor = comboBox;
        }
        else if (propertyType.IsCollectionType()) {
            var textBox = new TextBox {
                Watermark = "Comma-separated values"
            };
            _propertyEditors[property.Name] = textBox;
            editor = textBox;
        }
        else if (propertyType == typeof(string) || 
                 propertyType == typeof(ByteString) ||
                 propertyType == typeof(WideByteString) ||
                 propertyType == typeof(GID) ||
                 propertyType.IsPrimitive) {
            
            var textBox = new TextBox();
            _propertyEditors[property.Name] = textBox;
            editor = textBox;
        }
        else {
            // Complex type case
            var button = new Avalonia.Controls.Button {
                Content = $"Edit {displayName}",
                Command = ReactiveCommand.Create(() => EditComplexProperty(property))
            };
            _propertyEditors[property.Name] = button;
            editor = button;
        }
        
        // Add validation hint for numeric and other types.
        var hint = ValueConverters.GetTypeValidationHint(propertyType);
        
        var container = new StackPanel {
            Spacing = 5,
            Margin = new Thickness(0, 0, 0, 5)
        };
        
        container.Children.Add(new TextBlock { Text = displayName });
        
        if (editor != null) {
            container.Children.Add(editor);
        }
        
        if (!string.IsNullOrEmpty(hint)) {
            container.Children.Add(new TextBlock {
                Text = hint,
                FontSize = 12,
                Foreground = Brushes.Gray,
                FontStyle = FontStyle.Italic
            });
        }
        
        return container;
    }
    
    private async void EditComplexProperty(PropertyInfo property) {
        // For complex properties, we'd typically launch a nested editor.
        // This is a placeholder for complex property editing.
        // In a complete implementation, you would create specialized editors for each complex type.
        MessageService
            .Info("Complex editor is not yet implemented.")
            .WithDuration(TimeSpan.FromSeconds(5))
            .Send();
    }
    
    private void InitializeValues() {
        if (_template == null) {
            return;
        }

        foreach (var property in _template.GetType().GetEditableProperties()) {
            if (_propertyEditors.TryGetValue(property.Name, out var control)) {
                var value = property.GetValue(_template);
                
                switch (control) {
                    case CheckBox checkbox:
                        checkbox.IsChecked = value as bool? ?? false;
                        break;
                        
                    case ComboBox comboBox:
                        comboBox.SelectedItem = value;
                        break;
                        
                    case TextBox textBox:
                        if (property.PropertyType.IsCollectionType()) {
                            textBox.Text = value is IEnumerable<object> collection
                                ? string.Join(", ", collection)
                                : string.Empty;
                        }
                        else {
                            textBox.Text = value?.ToString() ?? string.Empty;
                        }
                        break;
                }
            }
        }
    }
    
    private void Save() {
        try {
            if (string.IsNullOrEmpty(_selectedResultType)) {
                MessageService
                    .Error("Please select a result type.")
                    .Send();

                return;
            }
            
            // Get the type and create an instance if needed
            if (!_resultTypes.TryGetValue(_selectedResultType, out var resultType)) {
                MessageService
                    .Error("Invalid result type selected.")
                    .Send();

                return;
            }
            
            // Create or use existing template
            var result = _template?.GetType() == resultType 
                ? _template 
                : (Result)Activator.CreateInstance(resultType)!;
            
            // Update all properties
            foreach (var property in resultType.GetEditableProperties()) {
                if (_propertyEditors.TryGetValue(property.Name, out var editor)) {
                    object? value = null;
                    
                    switch (editor) {
                        case CheckBox checkbox:
                            value = checkbox.IsChecked ?? false;
                            break;
                            
                        case ComboBox comboBox:
                            value = comboBox.SelectedItem;
                            break;
                            
                        case TextBox textBox:
                            try {
                                value = ValueConverters.ConvertValue(textBox.Text ?? string.Empty, property.PropertyType);
                            }
                            catch (Exception ex) {
                                MessageService
                                    .Error($"Invalid value for {property.Name}: {ex.Message}")
                                    .Send();

                                return;
                            }
                            break;
                    }
                    
                    if (value != null || Nullable.GetUnderlyingType(property.PropertyType) != null) {
                        property.SetValue(result, value);
                    }
                }
            }
            
            ResultSource.SetResult(result);
            Close();
        }
        catch (Exception ex) {
            MessageService
                .Error($"Failed to save result: {ex.Message}")
                .Send();
        }
    }

}