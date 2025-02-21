using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using Imcodec.ObjectProperty.TypeCache;

namespace Imview.Core.Controls;

internal sealed class BountyGoalEditorWindow : GoalTemplateEditorWindow {

    private const string WINDOW_TITLE = "Edit Bounty Goal Template";

    // Bounty-specific controls
    private TextBox _adjectivesBox;
    private TextBox _bountyTotalBox;
    private ComboBox _bountyTypeBox;

    private BountyGoalTemplate BountyTemplate => (BountyGoalTemplate) _template;

    public BountyGoalEditorWindow(BountyGoalTemplate template = null)
            : base(template ?? new BountyGoalTemplate(), WINDOW_TITLE) {
        this._adjectivesBox = new TextBox();
        this._bountyTotalBox = new TextBox();
        this._bountyTypeBox = new ComboBox {
            ItemsSource = Enum.GetValues<BOUNTY_TYPE>()
        };

        InitializeComponent();
        InitializeValues();
    }

    private void InitializeComponent() {
        _adjectivesBox = new TextBox();
        _bountyTotalBox = new TextBox();
        _bountyTypeBox = new ComboBox {
            ItemsSource = Enum.GetValues<BOUNTY_TYPE>()
        };

        var mainPanel = CreateBaseControlsPanel();

        // Bounty-specific Group
        var bountyContent = new StackPanel {
            Spacing = 5,
            Children =
            {
                CreateLabeledControl("Adjectives (comma-separated):", _adjectivesBox),
                CreateLabeledControl("Bounty Total:", _bountyTotalBox),
                CreateLabeledControl("Bounty Type:", _bountyTypeBox)
            }
        };

        mainPanel.Children.Add(CreateGroupBox("Bounty Settings", bountyContent));

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
            _adjectivesBox.Text = string.Join(", ", BountyTemplate.m_npcAdjectives ?? new List<string>());
            _bountyTotalBox.Text = BountyTemplate.m_bountyTotal.ToString();
            _bountyTypeBox.SelectedItem = BountyTemplate.m_bountyType;
        }
        catch (Exception ex) {
            Console.WriteLine($"Error initializing bounty values: {ex}");
        }
    }

    private void Save() {
        SaveBaseValues();

        try {
            var adjectives = (_adjectivesBox.Text ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .ToList();

            if (!int.TryParse(_bountyTotalBox.Text, out int bountyTotal)) {
                // Show error dialog for invalid number
                return;
            }

            BountyTemplate.m_npcAdjectives = adjectives;
            BountyTemplate.m_bountyTotal = bountyTotal;
            BountyTemplate.m_bountyType = (BOUNTY_TYPE) _bountyTypeBox.SelectedItem;

            _resultSource.TrySetResult(BountyTemplate);
            Close();
        }
        catch (Exception ex) {
            Console.WriteLine($"Error saving bounty values: {ex}");
        }
    }
}