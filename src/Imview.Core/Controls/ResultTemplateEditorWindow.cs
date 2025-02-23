using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ReactiveUI;
using System;
using System.Reflection;
using Imcodec.ObjectProperty.TypeCache;
using System.ComponentModel;
using System.Threading.Tasks;
using Imview.Core.Managers;
using System.Linq;
using Avalonia.Controls.Primitives;
using System.Collections.Generic;
using Avalonia.Controls.Templates;
using Imcodec.Types;
using Imcodec.IO;

namespace Imview.Core.Controls;

public class ResultTemplateEditor : Avalonia.Controls.Window {

    private const string WINDOW_TITLE = "Edit Result Template";
    private const int WINDOW_WIDTH = 400;
    private const int WINDOW_HEIGHT = 500;
    private const int TYPE_SELECTOR_WIDTH = 100;

    private readonly Result _result;
    private readonly TaskCompletionSource<Result> _resultSource = new();
    private readonly ComboBox _typeSelector;
    private readonly StackPanel _propertyPanel;
    private string _selectedResultType;

    public ResultTemplateEditor(Result existingResult = null) {
        _result = existingResult;

        Title = WINDOW_TITLE;
        Width = WINDOW_WIDTH;
        Height = WINDOW_HEIGHT;

        // Main layout
        var mainPanel = new StackPanel {
            Spacing = 10,
            Margin = new Thickness(20)
        };

        // Type selector
        _typeSelector = new ComboBox {
            ItemsSource = HumanizeResultsList(),
            Width = TYPE_SELECTOR_WIDTH,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        // ? Bugfix: Virtualization causes the ComboBox to grow/shrink as items are shown/hidden.
        // https://github.com/AvaloniaUI/Avalonia/issues/11018#issuecomment-1510803095
        _typeSelector.ItemsPanel = new FuncTemplate<Panel?>(new(() => new StackPanel()));

        if (_result != null) {
            _typeSelector.SelectedItem = _result.GetType().Name;
            _selectedResultType = _result.GetType().Name.Replace("Res", "");
            _typeSelector.IsEnabled = false; // Don't allow type changes for existing results.
        }

        _typeSelector.SelectionChanged += TypeSelector_SelectionChanged;

        mainPanel.Children.Add(CreateGroupBox("Result Type", new StackPanel {
            Spacing = 5,
            Children = {
                new TextBlock { Text = "Select Result Type:" },
                _typeSelector
            }
        }));

        // Property editor panel.
        _propertyPanel = new StackPanel { Spacing = 10 };
        var propertyScroller = new ScrollViewer {
            Content = _propertyPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        mainPanel.Children.Add(CreateGroupBox("Properties", propertyScroller));

        // Buttons.
        var buttonPanel = new StackPanel {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children = {
                new Avalonia.Controls.Button {
                    Content = "Save",
                    Command = ReactiveCommand.Create(Save)
                },
                new Avalonia.Controls.Button {
                    Content = "Cancel",
                    Command = ReactiveCommand.Create(Cancel)
                }
            }
        };

        mainPanel.Children.Add(buttonPanel);

        Content = new ScrollViewer { Content = mainPanel };

        // If editing existing result, initialize properties.
        if (_result != null) {
            PopulatePropertyEditors(_result.GetType());
        }
    }

    private void TypeSelector_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
        _selectedResultType = _typeSelector.SelectedItem as string;
        if (_typeSelector.SelectedItem is string typeName) {
            // We removed the prefix when we added the items to the combo box.
            // So we need to add it back before we look up the type.
            var prefixAddedTypeName = $"Res{typeName}";

            var type = ResultTypeManager.GetResultType(prefixAddedTypeName);
            if (type != null) {
                PopulatePropertyEditors(type);
            }
        }
    }

    private void PopulatePropertyEditors(System.Type resultType) {
        _propertyPanel.Children.Clear();

        var properties = resultType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && !p.Name.StartsWith("_")); // Filter out backing fields

        foreach (var property in properties) {
            var editor = CreateEditorForProperty(property);
            if (editor != null) {
                _propertyPanel.Children.Add(editor);
            }
        }
    }

    private Control CreateEditorForProperty(PropertyInfo property) {
        var container = new StackPanel {
            Spacing = 5,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var label = new TextBlock {
            Text = HumanizePropertyInfo(property)
        };

        Control editor;
        string hint = "";

        if (property.PropertyType == typeof(bool)) {
            var checkbox = new CheckBox();
            if (_result != null) {
                checkbox.IsChecked = (bool) property.GetValue(_result);
            }
            editor = checkbox;
        }
        else if (property.PropertyType.IsEnum) {
            var comboBox = new ComboBox {
                ItemsSource = Enum.GetValues(property.PropertyType)
            };
            if (_result != null) {
                comboBox.SelectedItem = property.GetValue(_result);
            }
            editor = comboBox;
        }
        else if (property.PropertyType == typeof(List<string>) ||
                 property.PropertyType == typeof(List<ByteString>) ||
                 property.PropertyType == typeof(IList<string>) ||
                 property.PropertyType == typeof(IList<ByteString>)) {
            var textBox = new TextBox();
            if (_result != null) {
                var value = property.GetValue(_result);
                if (value is IEnumerable<object> list) {
                    textBox.Text = string.Join(", ", list);
                }
            }
            editor = textBox;
            hint = "Enter values separated by commas (e.g., value1, value2, value3)";
        }
        else if (property.PropertyType == typeof(byte)) {
            var textBox = new TextBox();
            if (_result != null) {
                textBox.Text = property.GetValue(_result)?.ToString() ?? "";
            }
            editor = textBox;
            hint = "0 and 255";
        }
        else if (property.PropertyType == typeof(sbyte)) {
            var textBox = new TextBox();
            if (_result != null) {
                textBox.Text = property.GetValue(_result)?.ToString() ?? "";
            }
            editor = textBox;
            hint = "-128 and 127";
        }
        else if (property.PropertyType == typeof(short)) {
            var textBox = new TextBox();
            if (_result != null) {
                textBox.Text = property.GetValue(_result)?.ToString() ?? "";
            }
            editor = textBox;
            hint = "-32,768 and 32,767";
        }
        else if (property.PropertyType == typeof(ushort)) {
            var textBox = new TextBox();
            if (_result != null) {
                textBox.Text = property.GetValue(_result)?.ToString() ?? "";
            }
            editor = textBox;
            hint = "0 and 65,535";
        }
        else if (property.PropertyType == typeof(int)) {
            var textBox = new TextBox();
            if (_result != null) {
                textBox.Text = property.GetValue(_result)?.ToString() ?? "";
            }
            editor = textBox;
            hint = "-2,147,483,648 and 2,147,483,647";
        }
        else if (property.PropertyType == typeof(uint)) {
            var textBox = new TextBox();
            if (_result != null) {
                textBox.Text = property.GetValue(_result)?.ToString() ?? "";
            }
            editor = textBox;
            hint = "0 and 4,294,967,295";
        }
        else if (property.PropertyType == typeof(long)) {
            var textBox = new TextBox();
            if (_result != null) {
                textBox.Text = property.GetValue(_result)?.ToString() ?? "";
            }
            editor = textBox;
            hint = "-9,223,372,036,854,775,808 and 9,223,372,036,854,775,807";
        }
        else if (property.PropertyType == typeof(ulong)) {
            var textBox = new TextBox();
            if (_result != null) {
                textBox.Text = property.GetValue(_result)?.ToString() ?? "";
            }
            editor = textBox;
            hint = "0 and 18,446,744,073,709,551,615";
        }
        else if (property.PropertyType == typeof(float)) {
            var textBox = new TextBox();
            if (_result != null) {
                textBox.Text = property.GetValue(_result)?.ToString() ?? "";
            }
            editor = textBox;
            hint = "Enter a decimal number (e.g., 3.14)";
        }
        else if (property.PropertyType == typeof(double)) {
            var textBox = new TextBox();
            if (_result != null) {
                textBox.Text = property.GetValue(_result)?.ToString() ?? "";
            }
            editor = textBox;
            hint = "Enter a decimal number with up to 15-17 significant digits";
        }
        else if (property.PropertyType == typeof(string) ||
                 property.PropertyType == typeof(ByteString) ||
                 property.PropertyType == typeof(WideByteString)) {
            var textBox = new TextBox();
            if (_result != null) {
                textBox.Text = property.GetValue(_result)?.ToString() ?? "";
            }
            editor = textBox;
            hint = "Enter text";
        }
        else if (property.PropertyType == typeof(GID)) {
            var textBox = new TextBox();
            if (_result != null) {
                textBox.Text = property.GetValue(_result)?.ToString() ?? "";
            }
            editor = textBox;
            hint = "Template ID number";
        }
        else {
            var button = new Avalonia.Controls.Button {
                Content = $"Edit {property.Name}",
                Command = ReactiveCommand.Create(() => EditComplexProperty(property))
            };
            editor = button;
            hint = "Click to edit complex type";
        }

        container.Children.Add(label);
        container.Children.Add(editor);

        // Add hint text if we have it
        if (!string.IsNullOrEmpty(hint)) {
            container.Children.Add(new TextBlock {
                Text = hint,
                FontSize = 12,
                Foreground = Avalonia.Media.Brushes.Gray,
                FontStyle = Avalonia.Media.FontStyle.Italic
            });
        }

        return container;
    }

    private async void EditComplexProperty(PropertyInfo property) {
        var currentValue = _result != null ? property.GetValue(_result) : null;

        var window = new Avalonia.Controls.Window {
            Title = $"Edit {property.Name}",
            Width = 400,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        // Create a DockPanel as the main container
        var dockPanel = new DockPanel();

        // Button panel stays at bottom
        var buttonPanel = new StackPanel {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(20, 10, 20, 20)
        };
        DockPanel.SetDock(buttonPanel, Dock.Bottom);

        var tcs = new TaskCompletionSource<object>();

        var saveButton = new Avalonia.Controls.Button {
            Content = "Save",
            Command = ReactiveCommand.Create(() => {
                try {
                    var instance = currentValue ?? Activator.CreateInstance(property.PropertyType);
                    // ... rest of save logic ...
                    tcs.SetResult(instance);
                    window.Close();
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error saving complex property: {ex}");
                }
            })
        };

        var cancelButton = new Avalonia.Controls.Button {
            Content = "Cancel",
            Command = ReactiveCommand.Create(() => {
                tcs.SetResult(currentValue);
                window.Close();
            })
        };

        buttonPanel.Children.Add(saveButton);
        buttonPanel.Children.Add(cancelButton);
        dockPanel.Children.Add(buttonPanel);

        // Content panel takes remaining space
        var contentPanel = new ScrollViewer {
            Margin = new Thickness(20, 20, 20, 0),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        var propertyPanel = new StackPanel {
            Spacing = 10
        };

        var properties = property.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && !p.Name.StartsWith("_"));

        foreach (var prop in properties) {
            var editor = CreateEditorForProperty(prop);
            if (currentValue != null) {
                var control = (editor as StackPanel)?.Children[1];
                var value = prop.GetValue(currentValue);

                if (control is CheckBox checkbox) {
                    checkbox.IsChecked = (bool?) value ?? false;
                }
                else if (control is ComboBox comboBox) {
                    comboBox.SelectedItem = value;
                }
                else if (control is TextBox textBox) {
                    textBox.Text = value?.ToString() ?? "";
                }
            }
            propertyPanel.Children.Add(editor);
        }

        contentPanel.Content = propertyPanel;
        dockPanel.Children.Add(contentPanel);

        window.Content = dockPanel;

        await window.ShowDialog(this);
        var result = await tcs.Task;

        if (result != null && _result != null) {
            property.SetValue(_result, result);
        }
    }

    private static Border CreateGroupBox(string header, Control content) => new() {
        BorderBrush = Avalonia.Media.Brushes.Gray,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(3),
        Padding = new Thickness(10),
        Margin = new Thickness(0, 0, 0, 10),
        Child = new StackPanel {
            Spacing = 5,
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

    private void Save() {
        try {
            var prefixedAddedTypeName = $"Res{_selectedResultType}";
            var selectedTypeName = prefixedAddedTypeName
                ?? throw new InvalidOperationException("No result type selected.");
            var resultType = ResultTypeManager.GetResultType(prefixedAddedTypeName)
                ?? throw new InvalidOperationException($"Result type '{prefixedAddedTypeName}' not found.");

            Result result = _result ?? (Result) Activator.CreateInstance(resultType);

            foreach (var child in _propertyPanel.Children) {
                if (child is StackPanel panel && panel.Children.Count >= 2) {
                    var label = (TextBlock) panel.Children[0];
                    var editor = panel.Children[1];
                    var propertyName = DehumanizePropertyName(label.Text);
                    var property = result.GetType().GetProperty(propertyName);

                    if (property != null) {
                        object? value = null;
                        if (editor is CheckBox checkbox) {
                            value = checkbox.IsChecked ?? false;
                        }
                        else if (editor is ComboBox comboBox) {
                            value = comboBox.SelectedItem;
                        }
                        else if (editor is TextBox textBox) {
                            value = ConvertValue(textBox.Text ?? string.Empty, property.PropertyType);
                        }

                        if (value != null) {
                            property.SetValue(result, value);
                        }
                    }
                }
            }

            _resultSource.SetResult(result);
            Close();
        }
        catch (Exception ex) {
            // Show error dialog
            Console.WriteLine($"Error saving result: {ex}");
        }
    }

    private void Cancel() {
        _resultSource.SetResult(null);
        Close();
    }

    private static object? ConvertValue(string text, System.Type targetType) {
        if (string.IsNullOrEmpty(text)) {
            return null;
        }

        if (targetType == typeof(List<string>) || targetType == typeof(IList<string>)) {
            return text.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
        else if (targetType == typeof(List<ByteString>) || targetType == typeof(IList<ByteString>)) {
            return text.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => new ByteString(s))
                .ToList();
        }
        else if (targetType == typeof(int)) {
            return int.Parse(text);
        }
        else if (targetType == typeof(float)) {
            return float.Parse(text);
        }
        else if (targetType == typeof(double)) {
            return double.Parse(text);
        }
        else if (targetType == typeof(string)) {
            return text;
        }
        else if (targetType == typeof(ByteString)) {
            return new ByteString(text);
        }
        else if (targetType == typeof(WideByteString)) {
            return new WideByteString(text);
        }
        else if (targetType == typeof(GID)) {
            return new GID(ulong.Parse(text));
        }
        else if (targetType.IsEnum) {
            return Enum.Parse(targetType, text);
        }
        else if (targetType == typeof(bool)) {
            return bool.Parse(text);
        }
        else if (targetType == typeof(byte)) {
            return byte.Parse(text);
        }
        else if (targetType == typeof(sbyte)) {
            return sbyte.Parse(text);
        }
        else if (targetType == typeof(short)) {
            return short.Parse(text);
        }
        else if (targetType == typeof(ushort)) {
            return ushort.Parse(text);
        }
        else if (targetType == typeof(uint)) {
            return uint.Parse(text);
        }
        else if (targetType == typeof(long)) {
            return long.Parse(text);
        }
        else if (targetType == typeof(ulong)) {
            return ulong.Parse(text);
        }
        else {
            return text;
        }
    }

    private static List<string> HumanizeResultsList() {
        var resultTypes = ResultTypeManager.ResultTypes.Keys;

        // Trim out any result types that do not begin with "Res"
        // Remove the "Res." prefix from the remaining result types.
        return [.. resultTypes
            .Where(t => !t.StartsWith("Result"))
            .Where(t => t.StartsWith("Res"))
            .Select(t => t[3..])];
    }

    private static string HumanizePropertyInfo(PropertyInfo property) {
        var attribute = property.GetCustomAttribute<DisplayNameAttribute>();
        if (attribute != null) {
            return attribute.DisplayName;
        }

        // Properties are named as "m_propertyName" and we want to display it as "Property Name"
        var propertyName = property.Name[2..]; // Remove the "m_" prefix

        // Capitalize the first letter.
        propertyName = char.ToUpper(propertyName[0]) + propertyName[1..];

        // Add a space between each subsequent capital letter, unless two capitals are together.
        // e.g. "PropertyName" -> "Property Name"
        propertyName = System.Text.RegularExpressions.Regex.Replace(propertyName, "([A-Z])", " $1");

        return propertyName;
    }

    private static string DehumanizePropertyName(string humanizedName) {
        // Remove spaces and capitalize the first letter.
        var dehumanizedName = humanizedName.Replace(" ", "");
        dehumanizedName = char.ToLower(dehumanizedName[0]) + dehumanizedName[1..];

        // Attach the "m_" prefix.
        dehumanizedName = "m_" + dehumanizedName;

        return dehumanizedName;
    }

    public Task<Result> GetResultAsync() => _resultSource.Task;

}