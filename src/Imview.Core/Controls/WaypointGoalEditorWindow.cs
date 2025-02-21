using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Imcodec.IO;
using ReactiveUI;
using System;
using Imcodec.ObjectProperty.TypeCache;

namespace Imview.Core.Controls;

internal sealed class WaypointGoalEditorWindow : GoalTemplateEditorWindow {

    private const string WINDOW_TITLE = "Edit Waypoint Goal Template";

    // Waypoint-specific controls
    private CheckBox _zoneEntryBox;
    private CheckBox _zoneExitBox;
    private TextBox _zoneTagBox;
    private TextBox _proximityTagBox;

    private WaypointGoalTemplate WaypointTemplate => (WaypointGoalTemplate) _template;

    public WaypointGoalEditorWindow(WaypointGoalTemplate template = null)
        : base(template ?? new WaypointGoalTemplate(), WINDOW_TITLE) {
        InitializeComponent();
        InitializeValues();
    }

    private void InitializeComponent() {
        _zoneEntryBox = new CheckBox { Content = "Zone Entry" };
        _zoneExitBox = new CheckBox { Content = "Zone Exit" };
        _zoneTagBox = new TextBox();
        _proximityTagBox = new TextBox();

        var mainPanel = CreateBaseControlsPanel();

        // Waypoint-specific Group
        var waypointContent = new StackPanel {
            Spacing = 5,
            Children =
            {
                _zoneEntryBox,
                _zoneExitBox,
                CreateLabeledControl("Zone Tag:", _zoneTagBox),
                CreateLabeledControl("Proximity Tag:", _proximityTagBox)
            }
        };

        mainPanel.Children.Add(CreateGroupBox("Waypoint Settings", waypointContent));

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
            _zoneEntryBox.IsChecked = WaypointTemplate.m_zoneEntry;
            _zoneExitBox.IsChecked = WaypointTemplate.m_zoneExit;
            _zoneTagBox.Text = WaypointTemplate.m_zoneTag?.ToString() ?? string.Empty;
            _proximityTagBox.Text = WaypointTemplate.m_proximityTag?.ToString() ?? string.Empty;
        }
        catch (Exception ex) {
            Console.WriteLine($"Error initializing waypoint values: {ex}");
        }
    }

    private void Save() {
        SaveBaseValues();

        try {
            WaypointTemplate.m_zoneEntry = _zoneEntryBox.IsChecked ?? false;
            WaypointTemplate.m_zoneExit = _zoneExitBox.IsChecked ?? false;
            WaypointTemplate.m_zoneTag = new ByteString(_zoneTagBox.Text ?? string.Empty);
            WaypointTemplate.m_proximityTag = new ByteString(_proximityTagBox.Text ?? string.Empty);
            WaypointTemplate.m_goalType = GOAL_TYPE.GOAL_TYPE_WAYPOINT;

            _resultSource.SetResult(WaypointTemplate);
            Close();
        }
        catch (Exception ex) {
            Console.WriteLine($"Error saving waypoint values: {ex}");
        }
    }

}