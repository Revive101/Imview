using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Imcodec.IO;
using Imcodec.ObjectProperty.TypeCache;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Imview.Core.Controls;

internal abstract class GoalTemplateEditorWindow : Avalonia.Controls.Window {

    protected const int DEFAULT_WIDTH = 500;
    protected const int DEFAULT_HEIGHT = 600;

    // Base template controls
    protected TextBox _goalNameBox;
    protected NumericUpDown _goalNameIdBox;
    protected TextBox _goalTitleBox;
    protected TextBox _goalUnderwayBox;
    protected TextBox _hyperlinkBox;
    protected TextBox _completeTextBox;
    protected TextBox _locationNameBox;
    protected TextBox _displayImage1Box;
    protected TextBox _displayImage2Box;
    protected TextBox _destinationZoneBox;
    protected TextBox _clientTagsBox;
    protected TextBox _genericEventsBox;
    protected ListBox _resultsListBox;
    protected ObservableCollection<Result> _completionResults;
    protected ListBox _activationResultsListBox;
    protected ObservableCollection<Result> _activationResults;
    protected readonly CheckBox _autoQualifyBox = new() { Content = "Auto Qualify" };
    protected readonly CheckBox _autoCompleteBox = new() { Content = "Auto Complete" };
    protected readonly CheckBox _noQuestHelperBox = new() { Content = "No Quest Helper" };
    protected readonly CheckBox _petOnlyQuestBox = new() { Content = "Pet Only Quest" };
    protected readonly CheckBox _hideGoalFloatyTextBox = new() { Content = "Hide Goal Floaty Text" };
    protected ComboBox _goalTypeBox;

    protected readonly GoalTemplate _template;
    protected TaskCompletionSource<GoalTemplate> _resultSource;

    protected GoalTemplateEditorWindow(GoalTemplate template, string title) {
        this._goalNameBox = new TextBox();
        this._goalNameIdBox = new NumericUpDown();
        this._goalTitleBox = new TextBox();
        this._goalUnderwayBox = new TextBox();
        this._hyperlinkBox = new TextBox();
        this._completeTextBox = new TextBox();
        this._locationNameBox = new TextBox();
        this._displayImage1Box = new TextBox();
        this._displayImage2Box = new TextBox();
        this._destinationZoneBox = new TextBox();
        this._clientTagsBox = new TextBox();
        this._genericEventsBox = new TextBox();
        this._goalTypeBox = new ComboBox {
            ItemsSource = Enum.GetValues<GOAL_TYPE>()
        };

        this._template = template;
        this._resultSource = new TaskCompletionSource<GoalTemplate>();

        InitializeBaseControls();

        Title = title;
        Width = DEFAULT_WIDTH;
        Height = DEFAULT_HEIGHT;
    }

    private void InitializeBaseControls() {
        _goalNameBox = new TextBox();
        _goalNameIdBox = new NumericUpDown();
        _goalTitleBox = new TextBox();
        _goalUnderwayBox = new TextBox();
        _hyperlinkBox = new TextBox();
        _completeTextBox = new TextBox();
        _locationNameBox = new TextBox();
        _displayImage1Box = new TextBox();
        _displayImage2Box = new TextBox();
        _destinationZoneBox = new TextBox();
        _clientTagsBox = new TextBox();
        _genericEventsBox = new TextBox();
        _goalTypeBox = new ComboBox {
            ItemsSource = Enum.GetValues<GOAL_TYPE>()
        };
    }

    protected virtual void InitializeBaseValues() {
        try {
            _goalNameBox.Text = _template.m_goalName?.ToString() ?? string.Empty;
            _goalNameIdBox.Value = _template.m_goalNameID;
            _goalTitleBox.Text = _template.m_goalTitle?.ToString() ?? string.Empty;
            _goalUnderwayBox.Text = _template.m_goalUnderway?.ToString() ?? string.Empty;
            _hyperlinkBox.Text = _template.m_hyperlink?.ToString() ?? string.Empty;
            _completeTextBox.Text = _template.m_completeText?.ToString() ?? string.Empty;
            _locationNameBox.Text = _template.m_locationName?.ToString() ?? string.Empty;
            _displayImage1Box.Text = _template.m_displayImage1?.ToString() ?? string.Empty;
            _displayImage2Box.Text = _template.m_displayImage2?.ToString() ?? string.Empty;
            _destinationZoneBox.Text = _template.m_destinationZone?.ToString() ?? string.Empty;
            _clientTagsBox.Text = string.Join(", ", _template.m_clientTags ?? new List<string>());
            _genericEventsBox.Text = string.Join(", ", _template.m_genericEvents ?? new List<string>());

            _autoQualifyBox.IsChecked = _template.m_autoQualify;
            _autoCompleteBox.IsChecked = _template.m_autoComplete;
            _noQuestHelperBox.IsChecked = _template.m_noQuestHelper;
            _petOnlyQuestBox.IsChecked = _template.m_petOnlyQuest;
            _hideGoalFloatyTextBox.IsChecked = _template.m_hideGoalFloatyText;

            _goalTypeBox.SelectedItem = _template.m_goalType;
        }
        catch (Exception ex) {
            Console.WriteLine($"Error initializing base values: {ex}");
        }
    }

    protected static Control CreateLabeledControl(string labelText, Control control) => new StackPanel {
        Spacing = 5,
        Children =
        {
            new TextBlock { Text = labelText },
            control
        }
    };

    protected static Border CreateGroupBox(string header, Control content) => new() {
        BorderBrush = Avalonia.Media.Brushes.Gray,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(3),
        Padding = new Thickness(10),
        Margin = new Thickness(0, 0, 0, 10),
        Child = new StackPanel {
            Spacing = 5,
            Children =
            {
                new TextBlock
                {
                    Text = header,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Margin = new Thickness(0, 0, 0, 5)
                },
                content
            }
        }
    };

    protected StackPanel CreateBaseControlsPanel() {
        var mainPanel = new StackPanel {
            Spacing = 10,
            Margin = new Thickness(20)
        };

        // Basic Information Group
        var basicInfoContent = new StackPanel {
            Spacing = 5,
            Children =
            {
                CreateLabeledControl("Goal Name:", _goalNameBox),
                CreateLabeledControl("Goal Name ID:", _goalNameIdBox),
                CreateLabeledControl("Goal Title:", _goalTitleBox),
                CreateLabeledControl("Goal Type:", _goalTypeBox)
            }
        };

        // Display Text Group
        var displayTextContent = new StackPanel {
            Spacing = 5,
            Children =
            {
                CreateLabeledControl("Goal Underway Text:", _goalUnderwayBox),
                CreateLabeledControl("Complete Text:", _completeTextBox),
                CreateLabeledControl("Hyperlink:", _hyperlinkBox)
            }
        };

        // Location and Display Group
        var locationContent = new StackPanel {
            Spacing = 5,
            Children =
            {
                CreateLabeledControl("Location Name:", _locationNameBox),
                CreateLabeledControl("Destination Zone:", _destinationZoneBox),
                CreateLabeledControl("Display Image 1:", _displayImage1Box),
                CreateLabeledControl("Display Image 2:", _displayImage2Box)
            }
        };

        // Tags and Events Group
        var tagsContent = new StackPanel {
            Spacing = 5,
            Children =
            {
                CreateLabeledControl("Client Tags (comma-separated):", _clientTagsBox),
                CreateLabeledControl("Generic Events (comma-separated):", _genericEventsBox)
            }
        };

        // Flags Group
        var flagsContent = new StackPanel {
            Spacing = 5,
            Children =
            {
                _autoQualifyBox,
                _autoCompleteBox,
                _noQuestHelperBox,
                _petOnlyQuestBox,
                _hideGoalFloatyTextBox,
            }
        };

        mainPanel.Children.Add(CreateGroupBox("Basic Information", basicInfoContent));
        mainPanel.Children.Add(CreateGroupBox("Display Text", displayTextContent));
        mainPanel.Children.Add(CreateGroupBox("Location and Display", locationContent));
        mainPanel.Children.Add(CreateGroupBox("Tags and Events", tagsContent));
        mainPanel.Children.Add(CreateGroupBox("Flags", flagsContent));
        AddActivationResultsPanel(mainPanel);
        AddCompletionResultsPanel(mainPanel);

        return mainPanel;
    }

    protected void AddCompletionResultsPanel(StackPanel mainPanel) {
        _completionResults = new ObservableCollection<Result>(
            _template.m_completeResults?.m_results ?? new List<Result>()
        );

        var resultsContent = new DockPanel {
            LastChildFill = true
        };

        // Button panel for Add/Remove.
        var buttonPanel = new StackPanel {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Margin = new Thickness(0, 0, 0, 5)
        };
        DockPanel.SetDock(buttonPanel, Dock.Top);

        // Add Result button.
        var addButton = new Avalonia.Controls.Button {
            Content = "Add Result",
            Command = ReactiveCommand.Create(AddNewCompletionResult),
            Margin = new Thickness(0, 0, 0, 5)
        };

        // Remove Result button.
        var removeButton = new Avalonia.Controls.Button {
            Content = "Remove Result",
            Command = ReactiveCommand.Create(() => {
                if (_resultsListBox.SelectedItem is Result selectedResult) {
                    _completionResults.Remove(selectedResult);
                }
            })
        };

        buttonPanel.Children.Add(addButton);
        buttonPanel.Children.Add(removeButton);
        resultsContent.Children.Add(buttonPanel);

        // Results list.
        _resultsListBox = new ListBox {
            ItemsSource = _completionResults,
            Height = 150
        };
        _resultsListBox.DoubleTapped += ResultsListBox_DoubleTapped;
        resultsContent.Children.Add(_resultsListBox);

        mainPanel.Children.Add(CreateGroupBox("Completion Results", resultsContent));
    }

    private async void AddNewCompletionResult() {
        var editor = new ResultTemplateEditor();
        await editor.ShowDialog(this);

        var result = await editor.GetResultAsync();
        if (result != null) {
            _completionResults.Add(result);
        }
    }

    private async void ResultsListBox_DoubleTapped(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
        if (_resultsListBox.SelectedItem is Result selectedResult) {
            var editor = new ResultTemplateEditor(selectedResult);
            await editor.ShowDialog(this);

            var result = await editor.GetResultAsync();
            if (result != null) {
                var index = _completionResults.IndexOf(selectedResult);
                _completionResults[index] = result;
            }
        }
    }

    protected void AddActivationResultsPanel(StackPanel mainPanel) {
        _activationResults = new ObservableCollection<Result>(
            _template.m_activateResults?.m_results ?? new List<Result>()
        );

        // Create a container for the whole section
        var sectionContent = new StackPanel {
            Spacing = 5
        };

        // Button panel for Add/Remove
        var buttonPanel = new StackPanel {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var addButton = new Avalonia.Controls.Button {
            Content = "Add Result",
            Command = ReactiveCommand.Create(AddNewActivationResult)
        };

        var removeButton = new Avalonia.Controls.Button {
            Content = "Remove Result",
            Command = ReactiveCommand.Create(() => {
                if (_activationResultsListBox.SelectedItem is Result selectedResult) {
                    _activationResults.Remove(selectedResult);
                }
            })
        };

        buttonPanel.Children.Add(addButton);
        buttonPanel.Children.Add(removeButton);
        sectionContent.Children.Add(buttonPanel);

        // Results list
        _activationResultsListBox = new ListBox {
            ItemsSource = _activationResults,
            Height = 150
        };
        _activationResultsListBox.DoubleTapped += ActivationResultsListBox_DoubleTapped;
        sectionContent.Children.Add(_activationResultsListBox);

        mainPanel.Children.Add(CreateGroupBox("Activation Results", sectionContent));
    }

    private async void AddNewActivationResult() {
        var editor = new ResultTemplateEditor();
        await editor.ShowDialog(this);

        var result = await editor.GetResultAsync();
        if (result != null) {
            _activationResults.Add(result);
        }
    }

    private async void ActivationResultsListBox_DoubleTapped(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
        if (_activationResultsListBox.SelectedItem is Result selectedResult) {
            var editor = new ResultTemplateEditor(selectedResult);
            await editor.ShowDialog(this);

            var result = await editor.GetResultAsync();
            if (result != null) {
                var index = _activationResults.IndexOf(selectedResult);
                _activationResults[index] = result;
            }
        }
    }

    public Task<GoalTemplate> GetResultAsync() => _resultSource.Task;

    protected virtual void SaveBaseValues() {
        try {
            _template.m_goalName = new ByteString(_goalNameBox.Text ?? string.Empty);
            _template.m_goalNameID = (uint) (_goalNameIdBox.Value ?? 0);
            _template.m_goalTitle = new ByteString(_goalTitleBox.Text ?? string.Empty);
            _template.m_goalUnderway = new ByteString(_goalUnderwayBox.Text ?? string.Empty);
            _template.m_hyperlink = new ByteString(_hyperlinkBox.Text ?? string.Empty);
            _template.m_completeText = new ByteString(_completeTextBox.Text ?? string.Empty);
            _template.m_locationName = new ByteString(_locationNameBox.Text ?? string.Empty);
            _template.m_displayImage1 = new ByteString(_displayImage1Box.Text ?? string.Empty);
            _template.m_displayImage2 = new ByteString(_displayImage2Box.Text ?? string.Empty);
            _template.m_destinationZone = new ByteString(_destinationZoneBox.Text ?? string.Empty);

            _template.m_clientTags = (_clientTagsBox.Text ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => new ByteString(tag.Trim()).ToString())
                .ToList();

            _template.m_genericEvents = (_genericEventsBox.Text ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(evt => new ByteString(evt.Trim()).ToString())
                .ToList();

            _template.m_autoQualify = _autoQualifyBox.IsChecked ?? false;
            _template.m_autoComplete = _autoCompleteBox.IsChecked ?? false;
            _template.m_noQuestHelper = _noQuestHelperBox.IsChecked ?? false;
            _template.m_petOnlyQuest = _petOnlyQuestBox.IsChecked ?? false;
            _template.m_hideGoalFloatyText = _hideGoalFloatyTextBox.IsChecked ?? false;

            _template.m_goalType = (GOAL_TYPE) _goalTypeBox.SelectedItem;

            // Save the completion results.
            _template.m_completeResults = new ResultList {
                m_results = _completionResults.ToList()
            };

            // Save the activation results.
            _template.m_activateResults = new ResultList {
                m_results = _activationResults.ToList()
            };
        }
        catch (Exception ex) {
            Console.WriteLine($"Error saving base values: {ex}");
        }
    }

    protected virtual void Cancel() {
        _resultSource.SetResult(null);
        Close();
    }

}