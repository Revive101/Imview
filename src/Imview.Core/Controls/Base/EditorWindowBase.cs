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
using Avalonia.Layout;
using Imview.Core.Common.Constants;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace Imview.Core.Controls.Base;

/// <summary>
/// Base class for all editor windows in the application.
/// </summary>
public abstract class EditorWindowBase<T> : Window where T : class {

    protected readonly TaskCompletionSource<T> ResultSource = new();
    protected readonly StackPanel MainPanel;
    protected readonly ScrollViewer ContentViewer;

    protected EditorWindowBase(string title) {
        Title = title;
        Width = EditorConstants.DEFAULT_WINDOW_WIDTH;
        Height = EditorConstants.DEFAULT_WINDOW_HEIGHT;
        Background = EditorConstants.DEFAULT_WINDOW_BACKGROUND;

        MainPanel = new StackPanel {
            Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
            Margin = EditorConstants.DEFAULT_MARGIN_THICKNESS
        };

        ContentViewer = new ScrollViewer {
            Content = MainPanel
        };

        Content = ContentViewer;
    }

    /// <summary>
    /// Creates a labeled control with consistent styling.
    /// </summary>
    protected static Control CreateLabeledControl(string labelText, Control control) 
        => new StackPanel {
            Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
            Children = {
                    new TextBlock { Text = labelText },
                    control
                }
        };

    /// <summary>
    /// Creates a group box with consistent styling.
    /// </summary>
    protected static Border CreateGroupBox(string header, Control content) 
        => new() {
            BorderBrush = Avalonia.Media.Brushes.Gray,
            BorderThickness = new Thickness(1),
            CornerRadius = EditorConstants.DEFAULT_CORNER_RADIUS,
            Padding = EditorConstants.DEFAULT_GROUP_PADDING,
            Margin = new Thickness(0, 0, 0, 10),
            Child = new StackPanel {
                Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
                Children = {
                        new TextBlock {
                            Text = header,
                            FontWeight = Avalonia.Media.FontWeight.Bold,
                            Margin = new Thickness(0, 0, 0, 5)
                        },
                        content
                    }
            }
     };

    /// <summary>
    /// Creates action buttons with consistent styling.
    /// </summary>
    protected StackPanel CreateActionButtons(Action saveAction, Action cancelAction) 
        => new() {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = EditorConstants.DEFAULT_BUTTON_MARGIN,
            Children = {
                    new Button {
                        Content = "Save",
                        Command = ReactiveCommand.Create(saveAction)
                    },
                    new Button {
                        Content = "Cancel",
                        Command = ReactiveCommand.Create(cancelAction)
                    }
                }
        };

    /// <summary>
    /// Gets the result of the editor operation.
    /// </summary>
    public Task<T> GetResultAsync() => ResultSource.Task;

    /// <summary>
    /// Called when the editor is cancelled.
    /// </summary>
    protected virtual void Cancel() {
        ResultSource.SetResult(default!);
        Close();
    }

}