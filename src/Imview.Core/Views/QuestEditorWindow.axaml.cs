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
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Imcodec.ObjectProperty.TypeCache;
using Imview.Core.Services;
using System;
using System.Threading.Tasks;

namespace Imview.Core.Views;

public partial class QuestEditorWindow : Avalonia.Controls.Window {

    private readonly QuestTemplate _originalTemplate;
    private bool _hasChanges;

    public QuestTemplate EditedTemplate { get; private set; }

    public QuestEditorWindow() {
        InitializeComponent();
    }

    public QuestEditorWindow(QuestTemplate template) {
        _originalTemplate = template;
        EditedTemplate = template;
        
        InitializeComponent();
        InitializeEditor();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeEditor() {
        var editor = this.FindControl<Controls.Templates.QuestTemplateEditor>("QuestEditor");
        if (editor != null && _originalTemplate != null) {
            editor.Template = _originalTemplate;
            Title = $"Edit Quest: {_originalTemplate.m_questName}";
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e) {
        try {
            var editor = this.FindControl<Controls.Templates.QuestTemplateEditor>("QuestEditor");
            if (editor?.Template != null) {
                var success = await TemplateSerializer.SaveTemplateAsync(editor.Template, this);
                if (success) {
                    MessageService.Info("Quest template saved successfully.")
                        .WithDuration(TimeSpan.FromSeconds(3))
                        .Send();
                    
                    _hasChanges = true;
                    EditedTemplate = editor.Template;
                }
            }
        }
        catch (Exception ex) {
            MessageService.Error($"Error saving template: {ex.Message}")
                .WithDuration(TimeSpan.FromSeconds(5))
                .Send();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) {
        Close();
    }

    /// <summary>
    /// Checks if the template was modified during editing.
    /// </summary>
    public bool HasChanges() => _hasChanges;

}