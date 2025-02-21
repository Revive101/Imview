using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Imcodec.IO;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using Imcodec.ObjectProperty.TypeCache;

namespace Imview.Core.Controls; 

internal sealed class ScavengeGoalEditorWindow : GoalTemplateEditorWindow {

    private const string WINDOW_TITLE = "Edit Scavenge Goal Template";

    // Scavenge-specific controls
    private TextBox _itemAdjectivesBox;
    private TextBox _itemTotalBox;

    private ScavengeGoalTemplate ScavengeTemplate => (ScavengeGoalTemplate) _template;

    public ScavengeGoalEditorWindow(ScavengeGoalTemplate template = null)
        : base(template ?? new ScavengeGoalTemplate(), WINDOW_TITLE) {
        InitializeComponent();
        InitializeValues();
    }

    private void InitializeComponent() {
        _itemAdjectivesBox = new TextBox();
        _itemTotalBox = new TextBox();

        var mainPanel = CreateBaseControlsPanel();

        // Scavenge-specific Group
        var scavengeContent = new StackPanel {
            Spacing = 5,
            Children =
            {
                CreateLabeledControl("Item Adjectives (comma-separated):", _itemAdjectivesBox),
                CreateLabeledControl("Item Total:", _itemTotalBox)
            }
        };

        mainPanel.Children.Add(CreateGroupBox("Scavenge Settings", scavengeContent));

        // Action buttons
        var buttonPanel = new StackPanel {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Avalonia.Controls.Button
                {
                    Content = "Save",
                    Command = ReactiveCommand.Create(Save)
                },
                new Avalonia.Controls.Button
                {
                    Content = "Cancel",
                    Command = ReactiveCommand.Create(Cancel)
                }
            }
        };
        mainPanel.Children.Add(buttonPanel);

        Content = new ScrollViewer { Content = mainPanel };
    }

    private void InitializeValues() {
        InitializeBaseValues();

        try {
            _itemAdjectivesBox.Text = string.Join(", ", ScavengeTemplate.m_itemAdjectives ?? new List<String>());
            _itemTotalBox.Text = ScavengeTemplate.m_itemTotal.ToString();
        }
        catch (Exception ex) {
            Console.WriteLine($"Error initializing scavenge values: {ex}");
        }
    }

    private void Save() {
        SaveBaseValues();

        try {
            var adjectives = (_itemAdjectivesBox.Text ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => new ByteString(a.Trim()))
                .ToList();

            if (!int.TryParse(_itemTotalBox.Text, out int itemTotal)) {
                // Show error dialog for invalid number
                return;
            }

            ScavengeTemplate.m_itemAdjectives = adjectives.Select(a => a.ToString()).ToList();
            ScavengeTemplate.m_itemTotal = itemTotal;
            ScavengeTemplate.m_goalType = GOAL_TYPE.GOAL_TYPE_SCAVENGE;

            _resultSource.SetResult(ScavengeTemplate);
            Close();
        }
        catch (Exception ex) {
            Console.WriteLine($"Error saving scavenge values: {ex}");
        }
    }

}