using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using System;
using System.Timers;
using System.Windows.Input;

namespace LightspeedNexus.Controls;

public partial class DeleteButton : UserControl
{   
    /// <summary>
    /// The timer
    /// </summary>
    private readonly Timer timer = new(3000);

    /// <summary>
    /// Defines the <see cref="Command"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<Button, ICommand?>(nameof(Command), enableDataValidation: true);

    /// <summary>
    /// Defines the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<Button, object?>(nameof(CommandParameter));

    public DeleteButton()
    {
        InitializeComponent();
        timer.Elapsed += OnTimerTick;
    }

    /// <summary>
    /// Gets or sets an <see cref="ICommand"/> to be invoked when the button is clicked.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a parameter to be passed to the <see cref="Command"/>.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DeleteTextBlock.IsVisible)
            Command?.Execute(CommandParameter);
        else
        {
            DeleteTextBlock.IsVisible = true;
            timer.Start();
        }
    }

    /// <summary>
    /// Handles the timer tick
    /// </summary>
    private void OnTimerTick(object? source, ElapsedEventArgs e)
    {
        timer.Stop();
        Dispatcher.UIThread.Post(() => DeleteTextBlock.IsVisible = false);
    }
}