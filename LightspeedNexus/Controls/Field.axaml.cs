using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace LightspeedNexus.Controls;

// Define an enum for label positioning
public enum LabelPosition
{
    Left,
    Above
}

public partial class Field : UserControl
{
    private Label? _label;
    private TextBox? _textBox;

    public Field()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        // Get references to the named controls in the XAML
        _label = this.FindControl<Label>("PART_Label");
        _textBox = this.FindControl<TextBox>("PART_TextBox");

        // Subscribe to the Loaded event to apply initial layout based on LabelPosition
        Loaded += (sender, e) => UpdatePosition();
    }

    // Dependency Property for the Label's text
    public static readonly StyledProperty<string> LabelTextProperty =
        AvaloniaProperty.Register<Field, string>(nameof(LabelText), "Label");

    public string LabelText
    {
        get => GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    // Dependency Property for the TextBox's text (TwoWay binding)
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<Field, string>(nameof(Text), "", false, BindingMode.TwoWay);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // Dependency Property for the TextBox's placeholder text (Watermark)
    public static readonly StyledProperty<string> PlaceholderTextProperty =
        AvaloniaProperty.Register<Field, string>(nameof(PlaceholderText), "");

    public string PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    // Dependency Property for the Label's position
    public static readonly StyledProperty<LabelPosition> LabelPositionProperty =
        AvaloniaProperty.Register<Field, LabelPosition>(nameof(LabelPosition), LabelPosition.Left);

    public LabelPosition LabelPosition
    {
        get => GetValue(LabelPositionProperty);
        set => SetValue(LabelPositionProperty, value);
    }

    // Override OnPropertyChanged to react to changes in LabelPosition
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LabelPositionProperty)
        {
            UpdatePosition();
        }
    }

    // Method to update the layout of the label and textbox based on LabelPosition
    private void UpdatePosition()
    {
        if (_label == null || _textBox == null) return; // Ensure controls are loaded

        if (Content is Grid grid)
        {
            if (LabelPosition == LabelPosition.Left)
            {
                // Label to the left, TextBox to the right
                grid.ColumnDefinitions.Clear();
                grid.RowDefinitions.Clear();
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Only one row needed

                Grid.SetRow(_label, 0);
                Grid.SetColumn(_label, 0);
                Grid.SetRow(_textBox, 0);
                Grid.SetColumn(_textBox, 1);
                _label.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right; // Align label to the right within its column
                _label.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                _textBox.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            }
            else // LabelPosition == LabelPosition.Above
            {
                // Label above, TextBox below
                grid.ColumnDefinitions.Clear();
                grid.RowDefinitions.Clear();
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star)); // Only one column needed

                Grid.SetRow(_label, 0);
                Grid.SetColumn(_label, 0);
                Grid.SetRow(_textBox, 1);
                Grid.SetColumn(_textBox, 0);
                _label.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left; // Align label to the left
                _label.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom; // Align label to bottom of its row
                _label.Margin = new Thickness(0, 0, 0, 2); // Small margin between label and textbox
                _textBox.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top; // Align textbox to top of its row
            }
        }
    }
}