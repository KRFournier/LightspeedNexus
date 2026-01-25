using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Controls;
using LightspeedNexus.Messages;
using System.Linq;

namespace LightspeedNexus.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<OpenDialogMessage>(this, (_, m) => OpenDialog(m));
        WeakReferenceMessenger.Default.Register<CloseDialogMessage>(this, (_, _) => CloseModal());
    }

    private void OpenDialog(OpenDialogMessage msg)
    {
        Border back = new()
        {
            Background = Brushes.Black,
            Opacity = 0.80,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            IsHitTestVisible = true
        };

        StackPanel dialogPanel = new()
        {
            Spacing = 10,
            Width = 400,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        LightspeedBorder dialogBorder = new();
        dialogBorder.Classes.Add("colorcorners");
        dialogBorder.Classes.Add("darkback");

        // title
        TextBlock titleBlock = new()
        {
            Text = msg.Title,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        titleBlock.Classes.Add("large");

        Grid buttonPanel = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,10,Auto")
        };

        // additional buttons
        if (msg.AdditionalButtons.Length > 0)
        {
            StackPanel additionalButtonPanel = new()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            if (msg.AdditionalButtons.Contains(DialogButton.Delete))
            {
                Button deleteButton = new()
                {
                    Command = new RelayCommand(() => CloseDialog(msg, OpenDialogMessage.DialogResponse.Delete)),
                    Content = "Delete"
                };
                deleteButton.Classes.Add("red");
                additionalButtonPanel.Children.Add(deleteButton);
            }
            buttonPanel.Children.Add(additionalButtonPanel);
        }

        // Cancel, OK buttons
        StackPanel mainButtonsPanel = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10
        };
        Grid.SetColumn(mainButtonsPanel, 2);

        Button cancelButton = new()
        {
            Command = new RelayCommand(() => CloseDialog(msg, OpenDialogMessage.DialogResponse.Cancel)),
            Width = 75,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Content = "Cancel"
        };

        Button okButton = new()
        {
            Command = new RelayCommand(() => CloseDialog(msg, OpenDialogMessage.DialogResponse.Ok)),
            Width = 75,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Content = "OK"
        };

        // setup keybindings
        KeyBindings.Add(new KeyBinding()
        {
            Command = okButton.Command,
            Gesture = new KeyGesture(Key.Enter)
        });
        KeyBindings.Add(new KeyBinding()
        {
            Command = cancelButton.Command,
            Gesture = new KeyGesture(Key.Escape)
        });

        Control dialog = new ViewLocator().Build(msg.Item);
        dialog.DataContext = msg.Item;

        cancelButton.Classes.Add("gray");
        mainButtonsPanel.Children.Add(cancelButton);
        mainButtonsPanel.Children.Add(okButton);
        buttonPanel.Children.Add(mainButtonsPanel);
        dialogPanel.Children.Add(titleBlock);
        dialogPanel.Children.Add(dialogBorder);
        dialogPanel.Children.Add(buttonPanel);
        dialogBorder.Child = dialog;
        back.Child = dialogPanel;

        MainPanel.Children.Add(back);
    }

    private void CloseDialog(OpenDialogMessage msg, OpenDialogMessage.DialogResponse response)
    {
        CloseModal();
        msg.Respond(response);
    }

    private void CloseModal()
    {
        KeyBindings.Clear();

        if (MainPanel.Children.LastOrDefault() is Border)
            MainPanel.Children.RemoveAt(MainPanel.Children.Count - 1);
    }
}
