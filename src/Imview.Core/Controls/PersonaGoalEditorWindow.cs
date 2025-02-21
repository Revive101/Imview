using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Imcodec.IO;
using ReactiveUI;
using System;
using Imcodec.ObjectProperty.TypeCache;

namespace Imview.Core.Controls; 

internal sealed class PersonaGoalEditorWindow : GoalTemplateEditorWindow {

    private const string WINDOW_TITLE = "Edit Persona Goal Template";

    // Persona-specific controls
    private TextBox _personaNameBox;
    private CheckBox _usePatronBox;

    private PersonaGoalTemplate PersonaTemplate => (PersonaGoalTemplate) _template;

    public PersonaGoalEditorWindow(PersonaGoalTemplate template = null)
        : base(template ?? new PersonaGoalTemplate(), WINDOW_TITLE) {
        InitializeComponent();
        InitializeValues();
    }

    private void InitializeComponent() {
        _personaNameBox = new TextBox();
        _usePatronBox = new CheckBox { Content = "Use Patron" };

        var mainPanel = CreateBaseControlsPanel();

        // Persona-specific Group
        var personaContent = new StackPanel {
            Spacing = 5,
            Children =
            {
                CreateLabeledControl("Persona Name:", _personaNameBox),
                _usePatronBox
            }
        };

        mainPanel.Children.Add(CreateGroupBox("Persona Settings", personaContent));

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
            _personaNameBox.Text = PersonaTemplate.m_personaName?.ToString() ?? string.Empty;
            _usePatronBox.IsChecked = PersonaTemplate.m_usePatron;
        }
        catch (Exception ex) {
            Console.WriteLine($"Error initializing persona values: {ex}");
        }
    }

    private void Save() {
        SaveBaseValues();

        try {
            PersonaTemplate.m_personaName = new ByteString(_personaNameBox.Text ?? string.Empty);
            PersonaTemplate.m_usePatron = _usePatronBox.IsChecked ?? false;
            PersonaTemplate.m_goalType = GOAL_TYPE.GOAL_TYPE_PERSONA;

            _resultSource.SetResult(PersonaTemplate);
            Close();
        }
        catch (Exception ex) {
            Console.WriteLine($"Error saving persona values: {ex}");
        }
    }
    
}