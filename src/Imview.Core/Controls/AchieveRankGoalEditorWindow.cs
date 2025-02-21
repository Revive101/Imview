using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ReactiveUI;
using System;
using Imcodec.ObjectProperty.TypeCache;

namespace Imview.Core.Controls;

internal sealed class AchieveRankGoalEditorWindow : GoalTemplateEditorWindow {

    private const string WINDOW_TITLE = "Edit Achieve Rank Goal Template";

    // Achieve Rank-specific controls
    private NumericUpDown _rankBox;

    private AchieveRankGoalTemplate RankTemplate => (AchieveRankGoalTemplate) _template;

    public AchieveRankGoalEditorWindow(AchieveRankGoalTemplate template = null)
        : base(template ?? new AchieveRankGoalTemplate(), WINDOW_TITLE) {
        InitializeComponent();
        InitializeValues();
    }

    private void InitializeComponent() {
        _rankBox = new NumericUpDown {
            Minimum = 1,
            Maximum = 100,
            Value = 1
        };

        var mainPanel = CreateBaseControlsPanel();

        // Rank-specific Group
        var rankContent = new StackPanel {
            Spacing = 5,
            Children =
            {
                CreateLabeledControl("Required Rank:", _rankBox)
            }
        };

        mainPanel.Children.Add(CreateGroupBox("Rank Settings", rankContent));

        // Action buttons
        var buttonPanel = new StackPanel {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Avalonia.Controls.Button { Content = "Save", Command = ReactiveCommand.Create(Save) },
                new Avalonia.Controls.Button { Content = "Cancel", Command = ReactiveCommand.Create(Cancel) }
            }
        };
        mainPanel.Children.Add(buttonPanel);

        Content = new ScrollViewer { Content = mainPanel };
    }

    private void InitializeValues() {
        InitializeBaseValues();

        try {
            _rankBox.Value = RankTemplate.m_rank;
        }
        catch (Exception ex) {
            Console.WriteLine($"Error initializing rank values: {ex}");
        }
    }

    private void Save() {
        SaveBaseValues();

        try {
            RankTemplate.m_rank = (int) (_rankBox.Value ?? 1);
            RankTemplate.m_goalType = GOAL_TYPE.GOAL_TYPE_ACHIEVERANK;

            _resultSource.SetResult(RankTemplate);
            Close();
        }
        catch (Exception ex) {
            Console.WriteLine($"Error saving rank values: {ex}");
        }
    }

}