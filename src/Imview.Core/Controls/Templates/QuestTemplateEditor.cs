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

// QuestTemplateEditor.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ReactiveUI;
using System.Collections.ObjectModel;
using Imcodec.ObjectProperty.TypeCache;
using System;
using Imview.Core.Common.Constants;
using System.Linq;
using Imcodec.IO;
using Imview.Core.Controls.Goals;

namespace Imview.Core.Controls.Templates;

/// <summary>
/// Editor for Quest templates that manages both basic quest properties and associated goals.
/// </summary>
public partial class QuestTemplateEditor : UserControl {

    // Core properties
    private readonly QuestTemplate _template;
    private readonly ObservableCollection<GoalTemplate> _goals;
    private readonly IGoalEditorFactory _goalEditorFactory;

    // UI Controls
    private TextBox _questNameBox;
    private TextBox _questTitleBox;
    private NumericUpDown _questLevelBox;
    private ListBox _goalsList;
    private TextBox _questInfoBox;
    private TextBox _questPrepBox;
    private TextBox _questUnderwayBox;
    private TextBox _questCompleteBox;
    private TextBox _onStartScriptBox;
    private TextBox _onEndScriptBox;

    // Checkboxes
    private CheckBox _isHiddenBox;
    private CheckBox _noQuestHelperBox;
    private CheckBox _prepAlwaysBox;
    private CheckBox _questRepeatBox;
    private CheckBox _outdatedBox;

    // Parameter-less constructor for design-time support
    public QuestTemplateEditor() : this(null, null) { }

    public QuestTemplateEditor(QuestTemplate? template = null, IGoalEditorFactory? goalEditorFactory = null) {
        _template = template ?? new QuestTemplate();
        _goals = new ObservableCollection<GoalTemplate>(_template.m_goals ?? []);
        _goalEditorFactory = goalEditorFactory ?? new GoalEditorFactory();

        // Initialize non-nullable fields
        _questNameBox = new TextBox();
        _questTitleBox = new TextBox();
        _questLevelBox = new NumericUpDown();
        _goalsList = new ListBox();
        _questInfoBox = new TextBox();
        _questPrepBox = new TextBox();
        _questUnderwayBox = new TextBox();
        _questCompleteBox = new TextBox();
        _onStartScriptBox = new TextBox();
        _onEndScriptBox = new TextBox();
        _isHiddenBox = new CheckBox();
        _noQuestHelperBox = new CheckBox();
        _prepAlwaysBox = new CheckBox();
        _questRepeatBox = new CheckBox();
        _outdatedBox = new CheckBox();

        InitializeComponent();
        InitializeValues();
    }

    private void InitializeComponent() {
        var mainPanel = new StackPanel {
            Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
            Margin = EditorConstants.DEFAULT_MARGIN_THICKNESS
        };

        // Initialize controls, then build the UI.
        InitializeControls();

        mainPanel.Children.Add(CreateBasicInfoSection());
        mainPanel.Children.Add(CreateTextSection());
        mainPanel.Children.Add(CreateScriptSection());
        mainPanel.Children.Add(CreateFlagsSection());
        mainPanel.Children.Add(CreateGoalsSection());
        mainPanel.Children.Add(CreateActionButtons());

        Content = new ScrollViewer { Content = mainPanel };
    }

    private void InitializeControls() {
        // Basic info controls.
        _questNameBox = new TextBox();
        _questTitleBox = new TextBox();
        _questLevelBox = new NumericUpDown {
            Minimum = 1,
            Maximum = 200,
            Value = 1
        };

        _questInfoBox = new TextBox { AcceptsReturn = true, Height = 60 };
        _questPrepBox = new TextBox { AcceptsReturn = true, Height = 60 };
        _questUnderwayBox = new TextBox { AcceptsReturn = true, Height = 60 };
        _questCompleteBox = new TextBox { AcceptsReturn = true, Height = 60 };

        // Script controls. These are the names of scripts that 
        // will be executed when the quest starts or ends.
        _onStartScriptBox = new TextBox { AcceptsReturn = true, Height = 60 };
        _onEndScriptBox = new TextBox { AcceptsReturn = true, Height = 60 };

        // Quest flags.
        _isHiddenBox = new CheckBox { Content = "Is Hidden" };
        _noQuestHelperBox = new CheckBox { Content = "No Quest Helper" };
        _prepAlwaysBox = new CheckBox { Content = "Prep Always" };
        _questRepeatBox = new CheckBox { Content = "Quest Repeatable" };
        _outdatedBox = new CheckBox { Content = "Outdated" };

        // Goals list.
        _goalsList = new ListBox {
            ItemsSource = _goals,
            Height = 200
        };
        _goalsList.DoubleTapped += GoalsList_DoubleTapped;
    }

    private Control CreateBasicInfoSection() {
        var content = new StackPanel {
            Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
            Children = {
                CreateLabeledControl("Quest Name:", _questNameBox),
                CreateLabeledControl("Quest Title:", _questTitleBox),
                CreateLabeledControl("Quest Level:", _questLevelBox)
            }
        };

        return CreateGroupBox("Basic Quest Information", content);
    }

    private Control CreateTextSection() {
        var content = new StackPanel {
            Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
            Children = {
                CreateLabeledControl("Quest Info:", _questInfoBox),
                CreateLabeledControl("Quest Prep:", _questPrepBox),
                CreateLabeledControl("Quest Underway:", _questUnderwayBox),
                CreateLabeledControl("Quest Complete:", _questCompleteBox)
            }
        };

        return CreateGroupBox("Quest Text", content);
    }

    private Control CreateScriptSection() {
        var content = new StackPanel {
            Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
            Children = {
                CreateLabeledControl("On Start Script:", _onStartScriptBox),
                CreateLabeledControl("On End Script:", _onEndScriptBox)
            }
        };

        return CreateGroupBox("Quest Scripts", content);
    }

    private Control CreateFlagsSection() {
        var content = new StackPanel {
            Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
            Children = {
                _isHiddenBox,
                _noQuestHelperBox,
                _prepAlwaysBox,
                _questRepeatBox,
                _outdatedBox
            }
        };

        return CreateGroupBox("Quest Flags", content);
    }

    private Control CreateGoalsSection() {
        var buttonPanel = new StackPanel {
            Orientation = Orientation.Horizontal,
            Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
            Children = {
                CreateGoalButton("Add Bounty Goal", () => AddGoal<BountyGoalTemplate>()),
                CreateGoalButton("Add Persona Goal", () => AddGoal<PersonaGoalTemplate>()),
                CreateGoalButton("Add Scavenge Goal", () => AddGoal<ScavengeGoalTemplate>()),
                CreateGoalButton("Add Waypoint Goal", () => AddGoal<WaypointGoalTemplate>()),
                CreateGoalButton("Add Achieve Rank Goal", () => AddGoal<AchieveRankGoalTemplate>())
            }
        };

        var content = new DockPanel {
            LastChildFill = true,
            Children = {
                buttonPanel,
                _goalsList
            }
        };
        DockPanel.SetDock(buttonPanel, Dock.Top);

        return CreateGroupBox("Quest Goals", content);
    }

    private Control CreateActionButtons() 
        => new StackPanel {
            Orientation = Orientation.Horizontal,
            Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children = {
                new Avalonia.Controls.Button {
                    Content = "Save Template",
                    Command = ReactiveCommand.Create(SaveTemplate)
                },
                new Avalonia.Controls.Button {
                    Content = "Load Template",
                    Command = ReactiveCommand.Create(LoadTemplate)
                }
            }
        };

    private static Control CreateLabeledControl(string labelText, Control control) 
        => new StackPanel {
            Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
            Children = {
                new TextBlock { Text = labelText },
                control
            }
        };

    private static Border CreateGroupBox(string header, Control content) 
        => new() {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            CornerRadius = EditorConstants.DEFAULT_CORNER_RADIUS,
            Padding = EditorConstants.DEFAULT_GROUP_PADDING,
            Margin = new Thickness(0, 0, 0, 10),
            Child = new StackPanel {
                Spacing = EditorConstants.DEFAULT_CONTROL_SPACING,
                Children = {
                    new TextBlock {
                        Text = header,
                        FontWeight = FontWeight.Bold,
                        Margin = new Thickness(0, 0, 0, 5)
                    },
                    content
                }
            }
        };

    private static Avalonia.Controls.Button CreateGoalButton(string content, System.Action onClick) 
        => new() {
            Content = content,
            Command = ReactiveCommand.Create(onClick)
        };

    private void InitializeValues() {
        _questNameBox.Text = _template.m_questName?.ToString();
        _questTitleBox.Text = _template.m_questTitle?.ToString();
        _questLevelBox.Value = _template.m_questLevel;
        _questInfoBox.Text = _template.m_questInfo?.ToString();
        _questPrepBox.Text = _template.m_questPrep?.ToString();
        _questUnderwayBox.Text = _template.m_questUnderway?.ToString();
        _questCompleteBox.Text = _template.m_questComplete?.ToString();
        _onStartScriptBox.Text = _template.m_onStartQuestScript?.ToString();
        _onEndScriptBox.Text = _template.m_onEndQuestScript?.ToString();

        _isHiddenBox.IsChecked = _template.m_isHidden;
        _noQuestHelperBox.IsChecked = _template.m_noQuestHelper;
        _prepAlwaysBox.IsChecked = _template.m_prepAlways;
        _questRepeatBox.IsChecked = _template.m_questRepeat >= 1;
        _outdatedBox.IsChecked = _template.m_outdated;
    }

    private async void AddGoal<T>() where T : GoalTemplate, new() {
        var result = await _goalEditorFactory.CreateEditor(new T());
        if (result != null) {
            _goals.Add(result);
        }
    }

    private async void GoalsList_DoubleTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        if (_goalsList.SelectedItem is GoalTemplate selectedGoal) {
            var result = await _goalEditorFactory.CreateEditor(selectedGoal);
            if (result != null) {
                var index = _goals.IndexOf(selectedGoal);
                _goals[index] = result;
            }
        }
    }

    private void SaveTemplate() {
        try {
            // Save basic quest properties.
            _template.m_questName          = new ByteString(_questNameBox.Text ?? string.Empty);
            _template.m_questTitle         = new ByteString(_questTitleBox.Text ?? string.Empty);
            _template.m_questLevel         = (int)(_questLevelBox.Value ?? 1);
            _template.m_questInfo          = new ByteString(_questInfoBox.Text ?? string.Empty);
            _template.m_questPrep          = new ByteString(_questPrepBox.Text ?? string.Empty);
            _template.m_questUnderway      = new ByteString(_questUnderwayBox.Text ?? string.Empty);
            _template.m_questComplete      = new ByteString(_questCompleteBox.Text ?? string.Empty);
            _template.m_onStartQuestScript = new ByteString(_onStartScriptBox.Text ?? string.Empty);
            _template.m_onEndQuestScript   = new ByteString(_onEndScriptBox.Text ?? string.Empty);

            // Save quest flags.
            _template.m_isHidden           = _isHiddenBox.IsChecked ?? false;
            _template.m_noQuestHelper      = _noQuestHelperBox.IsChecked ?? false;
            _template.m_prepAlways         = _prepAlwaysBox.IsChecked ?? false;
            _template.m_questRepeat        = (_questRepeatBox.IsChecked ?? false) ? 1 : 0;
            _template.m_outdated           = _outdatedBox.IsChecked ?? false;

            // Save quest goals.
            _template.m_goals              = _goals.ToList();

            // TODO: Save template to file
        }
        catch (Exception ex) {
            // TODO: Show error dialog
            Console.WriteLine($"Error saving template: {ex}");
        }
    }

    private void LoadTemplate() {
        try {
            // TODO: Implement template loading
        }
        catch (Exception ex) {
            // TODO: Show error dialog
            Console.WriteLine($"Error loading template: {ex}");
        }
    }

}