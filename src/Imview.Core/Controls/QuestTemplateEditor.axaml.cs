using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ReactiveUI;
using System.Collections.ObjectModel;
using Imcodec.ObjectProperty.TypeCache;

namespace Imview.Core.Controls;

public partial class QuestTemplateEditor : UserControl {

    private ObservableCollection<GoalTemplate> goals;
    private QuestTemplate currentTemplate;

    public QuestTemplateEditor() {
        InitializeComponent();
        goals = new ObservableCollection<GoalTemplate>();
    }

    private Border CreateGroupBorder(string header, Control content) {
        var headerText = new TextBlock {
            Text = header,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var stack = new StackPanel {
            Spacing = 5,
            Children =
            {
                headerText,
                content
            }
        };

        return new Border {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(10),
            Margin = new Thickness(0, 0, 0, 10),
            Child = stack
        };
    }

    private void InitializeComponent() {
        var mainPanel = new StackPanel {
            Spacing = 10,
            Margin = new Thickness(20)
        };

        // Basic Quest Info Section
        var basicInfoContent = new StackPanel {
            Spacing = 5,
            Children =
            {
                new TextBlock { Text = "Quest Name:" },
                (questNameBox = new TextBox()),
                new TextBlock { Text = "Quest Title:" },
                (questTitleBox = new TextBox()),
                new TextBlock { Text = "Quest Level:" },
                (questLevelBox = new NumericUpDown
                {
                    Minimum = 1,
                    Maximum = 100,
                    Value = 1
                })
            }
        };

        // Quest Flags Section
        var flagsContent = new StackPanel {
            Spacing = 5,
            Children =
            {
                (isHiddenBox = new CheckBox { Content = "Is Hidden" }),
                (noQuestHelperBox = new CheckBox { Content = "No Quest Helper" }),
                (prepAlwaysBox = new CheckBox { Content = "Prep Always" }),
                (questRepeatBox = new CheckBox { Content = "Quest Repeatable" })
            }
        };

        // Goals Section
        var goalsContent = new DockPanel {
            Children =
            {
                new StackPanel
                {
                    Spacing = 5,
                    Children =
                    {
                        new Avalonia.Controls.Button
                        {
                            Content = "Add Achievement Goal",
                            //Command = ReactiveCommand.Create(AddAchievementGoal)
                        },
                        (goalsList = new ListBox
                        {
                            ItemsSource = goals,
                            Height = 200
                        })
                    }
                }
            }
        };

        // Save/Load Buttons
        var actionPanel = new StackPanel {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                new Avalonia.Controls.Button
                {
                    Content = "Save Template",
                    //Command = ReactiveCommand.Create(SaveTemplate)
                },
                new Avalonia.Controls.Button
                {
                    Content = "Load Template",
                    //Command = ReactiveCommand.Create(LoadTemplate)
                }
            }
        };

        mainPanel.Children.Add(CreateGroupBorder("Basic Quest Information", basicInfoContent));
        mainPanel.Children.Add(CreateGroupBorder("Quest Flags", flagsContent));
        mainPanel.Children.Add(CreateGroupBorder("Quest Goals", goalsContent));
        mainPanel.Children.Add(actionPanel);

        Content = new ScrollViewer { Content = mainPanel };
    }

    // ... rest of the code remains the same ...
}

public class GoalEditorWindow : Avalonia.Controls.Window {
    private TextBox goalNameBox;
    private TextBox goalTitleBox;
    private ComboBox goalTypeCombo;
    private NumericUpDown rankBox;
    private ComboBox schoolCombo;
    private CheckBox autoCompleteBox;
    private CheckBox autoQualifyBox;

    public GoalEditorWindow() {
        Title = "Add Achievement Goal";
        Width = 400;
        Height = 500;
        InitializeComponent();
    }

    private void InitializeComponent() {
        var mainPanel = new StackPanel {
            Spacing = 10,
            Margin = new Thickness(20)
        };

        goalNameBox = new TextBox { Watermark = "Goal Name" };
        goalTitleBox = new TextBox { Watermark = "Goal Title" };
        goalTypeCombo = new ComboBox {
            ItemsSource = new[] { "GOAL_TYPE_ACHIEVERANK" },
            SelectedIndex = 0
        };
        rankBox = new NumericUpDown {
            Minimum = 1,
            Maximum = 100,
            Value = 100
        };
        schoolCombo = new ComboBox {
            ItemsSource = new[] { "Fire", "Ice", "Storm", "Life", "Death", "Myth", "Balance" },
            SelectedIndex = 0
        };
        autoCompleteBox = new CheckBox { Content = "Auto Complete" };
        autoQualifyBox = new CheckBox { Content = "Auto Qualify" };

        mainPanel.Children.Add(new TextBlock { Text = "Goal Name:" });
        mainPanel.Children.Add(goalNameBox);
        mainPanel.Children.Add(new TextBlock { Text = "Goal Title:" });
        mainPanel.Children.Add(goalTitleBox);
        mainPanel.Children.Add(new TextBlock { Text = "Goal Type:" });
        mainPanel.Children.Add(goalTypeCombo);
        mainPanel.Children.Add(new TextBlock { Text = "Rank:" });
        mainPanel.Children.Add(rankBox);
        mainPanel.Children.Add(new TextBlock { Text = "School:" });
        mainPanel.Children.Add(schoolCombo);
        mainPanel.Children.Add(autoCompleteBox);
        mainPanel.Children.Add(autoQualifyBox);

        var buttonPanel = new StackPanel {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
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
                    Command = ReactiveCommand.Create(() => Close(null))
                }
            }
        };

        mainPanel.Children.Add(buttonPanel);
        Content = mainPanel;
    }

    private void Save() {
        var goal = new AchieveRankGoalTemplate {
            m_goalName = goalNameBox.Text,
            m_goalTitle = goalTitleBox.Text,
            //m_goalType = goalTypeCombo.SelectedItem as string,
            m_rank = (int) rankBox.Value,
            //m_schoolName = schoolCombo.SelectedItem as string,
            m_autoComplete = autoCompleteBox.IsChecked ?? false,
            m_autoQualify = autoQualifyBox.IsChecked ?? false
        };

        Close(goal);
    }

}
