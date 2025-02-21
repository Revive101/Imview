using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ReactiveUI;
using System.Collections.ObjectModel;
using Imcodec.ObjectProperty.TypeCache;
using System;
using Avalonia.VisualTree;
using Avalonia.Interactivity;

namespace Imview.Core.Controls;

public partial class QuestTemplateEditor : UserControl {

    private ObservableCollection<GoalTemplate> _goals;
    private QuestTemplate _currentTemplate = new();

    public QuestTemplateEditor() {
        this._goals = [];

        InitializeComponent();
    }

    private void InitializeComponent() {
        var mainPanel = new StackPanel {
            Spacing = 10,
            Margin = new Thickness(20)
        };

        // Basic Quest Info Section
        var basicInfoContent = new StackPanel {
            Spacing = 5,
            Children = {
                new TextBlock { Text = "Quest Name:" },
                (questNameBox = new TextBox()),
                new TextBlock { Text = "Quest Title:" },
                (questTitleBox = new TextBox()),
                new TextBlock { Text = "Quest Level:" },
                (questLevelBox = new NumericUpDown {
                    Minimum = 1,
                    Maximum = 50,
                    Value = 1
                })
            }
        };

        // Quest Flags Section
        var flagsContent = new StackPanel {
            Spacing = 5,
            Children = {
                (isHiddenBox = new CheckBox { Content = "Is Hidden" }),
                (noQuestHelperBox = new CheckBox { Content = "No Quest Helper" }),
                (prepAlwaysBox = new CheckBox { Content = "Prep Always" }),
                (questRepeatBox = new CheckBox { Content = "Quest Repeatable" })
            }
        };

        // Goals Section
        var goalsContent = new DockPanel {
            LastChildFill = true,
            Children = {
                    new StackPanel {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        [DockPanel.DockProperty] = Dock.Top,
                        Margin = new Thickness(0, 0, 0, 5),
                        Children = {
                            CreateGoalButton("Add Bounty Goal", AddBountyGoal),
                            CreateGoalButton("Add Persona Goal", AddPersonaGoal),
                            CreateGoalButton("Add Scavenge Goal", AddScavengeGoal),
                            CreateGoalButton("Add Waypoint Goal", AddWaypointGoal),
                            CreateGoalButton("Add Achieve Rank Goal", AddAchieveRankGoal)
                        }
                    },
                    new StackPanel {
                        Children = {
                            (goalsList = new ListBox {
                                ItemsSource = _goals,
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
            Children = {
                new Avalonia.Controls.Button {
                    Content = "Save Template",
                    //Command = ReactiveCommand.Create(SaveTemplate)
                },
                new Avalonia.Controls.Button {
                    Content = "Load Template",
                    //Command = ReactiveCommand.Create(LoadTemplate)
                }
            }
        };

        mainPanel.Children.Add(CreateGroupBorder("Basic Quest Information", basicInfoContent));
        mainPanel.Children.Add(CreateGroupBorder("Quest Flags", flagsContent));
        mainPanel.Children.Add(CreateGroupBorder("Quest Goals", goalsContent));
        mainPanel.Children.Add(actionPanel);

        goalsList.DoubleTapped += GoalsList_DoubleTapped;

        Content = new ScrollViewer { Content = mainPanel };
    }

    private static Border CreateGroupBorder(string header, Control content) {
        var headerText = new TextBlock {
            Text = header,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var stack = new StackPanel {
            Spacing = 5,
            Children = {
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

    private Avalonia.Controls.Button CreateGoalButton(string content, System.Action onClick) => new() {
        Content = content,
        Command = ReactiveCommand.Create(onClick)
    };

    private async void AddBountyGoal() {
        if (this.GetVisualRoot() is Avalonia.Controls.Window owner) {
            var window = new BountyGoalEditorWindow();
            await window.ShowDialog(owner);

            var result = await window.GetResultAsync();
            if (result != null) {
                AddGoal(result);
            }
        }
    }

    private async void AddPersonaGoal() {
        if (this.GetVisualRoot() is Avalonia.Controls.Window owner) {
            var window = new PersonaGoalEditorWindow();
            await window.ShowDialog(owner);

            var result = await window.GetResultAsync();
            if (result != null) {
                AddGoal(result);
            }
        }
    }

    private async void AddScavengeGoal() {
        if (this.GetVisualRoot() is Avalonia.Controls.Window owner) {
            var window = new ScavengeGoalEditorWindow();
            await window.ShowDialog(owner);

            var result = await window.GetResultAsync();
            if (result != null) {
                AddGoal(result);
            }
        }
    }

    private async void AddWaypointGoal() {
        if (this.GetVisualRoot() is Avalonia.Controls.Window owner) {
            var window = new WaypointGoalEditorWindow();
            await window.ShowDialog(owner);

            var result = await window.GetResultAsync();
            if (result != null) {
                AddGoal(result);
            }
        }
    }

    private async void AddAchieveRankGoal() {
        if (this.GetVisualRoot() is Avalonia.Controls.Window owner) {
            var window = new AchieveRankGoalEditorWindow();
            await window.ShowDialog(owner);

            var result = await window.GetResultAsync();
            if (result != null) {
                AddGoal(result);
            }
        }
    }

    private async void GoalsList_DoubleTapped(object sender, RoutedEventArgs e) {
        if (goalsList.SelectedItem is GoalTemplate selectedGoal) {
            if (this.GetVisualRoot() is Avalonia.Controls.Window owner) {
                switch (selectedGoal) {
                    case BountyGoalTemplate bountyGoal:
                        var bountyEditor = new BountyGoalEditorWindow(bountyGoal);
                        await bountyEditor.ShowDialog(owner);
                        var editedBountyGoal = await bountyEditor.GetResultAsync();
                        if (editedBountyGoal != null) {
                            UpdateGoal(selectedGoal, editedBountyGoal);
                        }
                        break;

                    case PersonaGoalTemplate personaGoal:
                        var personaEditor = new PersonaGoalEditorWindow(personaGoal);
                        await personaEditor.ShowDialog(owner);
                        var editedPersonaGoal = await personaEditor.GetResultAsync();
                        if (editedPersonaGoal != null) {
                            UpdateGoal(selectedGoal, editedPersonaGoal);
                        }
                        break;

                    case ScavengeGoalTemplate scavengeGoal:
                        var scavengeEditor = new ScavengeGoalEditorWindow(scavengeGoal);
                        await scavengeEditor.ShowDialog(owner);
                        var editedScavengeGoal = await scavengeEditor.GetResultAsync();
                        if (editedScavengeGoal != null) {
                            UpdateGoal(selectedGoal, editedScavengeGoal);
                        }
                        break;

                    case WaypointGoalTemplate waypointGoal:
                        var waypointEditor = new WaypointGoalEditorWindow(waypointGoal);
                        await waypointEditor.ShowDialog(owner);
                        var editedWaypointGoal = await waypointEditor.GetResultAsync();
                        if (editedWaypointGoal != null) {
                            UpdateGoal(selectedGoal, editedWaypointGoal);
                        }
                        break;

                    case AchieveRankGoalTemplate rankGoal:
                        var rankEditor = new AchieveRankGoalEditorWindow(rankGoal);
                        await rankEditor.ShowDialog(owner);
                        var editedRankGoal = await rankEditor.GetResultAsync();
                        if (editedRankGoal != null) {
                            UpdateGoal(selectedGoal, editedRankGoal);
                        }
                        break;

                    default:
                        Console.WriteLine($"Unknown goal type: {selectedGoal.GetType().Name}");
                        break;
                }
            }
        }
    }

    private void UpdateGoal(GoalTemplate oldGoal, GoalTemplate newGoal) {
        var index = _goals.IndexOf(oldGoal);
        if (index != -1) {
            _goals[index] = newGoal;
        }

        var templateIndex = _currentTemplate.m_goals?.IndexOf(oldGoal) ?? -1;
        if (templateIndex != -1) {
            _currentTemplate.m_goals[templateIndex] = newGoal;
        }
    }

    private void AddGoal(GoalTemplate goal) {
        _goals.Add(goal);
        _currentTemplate.m_goals ??= [];
        _currentTemplate.m_goals.Add(goal);
    }

}