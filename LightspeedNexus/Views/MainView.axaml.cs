using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.Dialogs;
using Lightspeed.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus.Views;

public partial class MainView : UserControl
{
    private readonly Stack<Control> _dialogStack = [];

    public MainView()
    {
        InitializeComponent();

        var messenger = App.Services.GetRequiredService<IMessenger>();
        messenger.Register<CloseDialogMessage>(this, (_, _) => CloseModal());
        messenger.Register<EditDialogMessage>(this, (_, m) => EditDialog(m));
        messenger.Register<BeginWaitMessage>(this, (_, m) => BeginWait(m.Value));
        messenger.Register<EndWaitMessage>(this, (_, _) => EndWait());
        messenger.Register<ShowSignDialogMessage>(this, (_, m) => ShowSignDialog(m));
        messenger.Register<MessageBoxMessage>(this, (_, m) => ShowMessageBox(m.Value));
    }

    private void OpenModel(Control dialog)
    {
        _dialogStack.Push(dialog);
        ContentControl.IsEnabled = false;

        Border back = new()
        {
            Background = Brushes.Black,
            Opacity = 0.80,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            IsHitTestVisible = true,
            Child = dialog
        };

        MainPanel.Children.Add(back);
    }

    private void CloseModal()
    {
        if (_dialogStack.Count > 0)
        {
            _dialogStack.Pop();
            MainPanel.Children.RemoveAt(MainPanel.Children.Count - 1);
        }

        if (_dialogStack.Count == 0)
            ContentControl.IsEnabled = true;
    }

    private void EditDialog(EditDialogMessage msg) => OpenModel(new EditDialog(msg, () => CloseModal()));

    private void BeginWait(string message) => OpenModel(new WaitDialog(message));

    private void ShowSignDialog(ShowSignDialogMessage message) => OpenModel(new SignDialog(signature =>
    {
        CloseModal();
        message.Respond(signature);
    }));

    private void ShowMessageBox(string message) => OpenModel(new MessageDialog(message, () => CloseModal()));

    private void EndWait()
    {
        if (_dialogStack.Peek() is WaitDialog)
            CloseModal();
    }
}
