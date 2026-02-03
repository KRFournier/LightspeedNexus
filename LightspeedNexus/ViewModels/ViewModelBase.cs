using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using System;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

public class ViewModelBase : ObservableObject
{
    /// <summary>
    /// The result of a call to DialogBox
    /// </summary>
    protected class DialogBoxResult<T> where T : ViewModelBase
    {
        public bool IsOk { get; set; } = false;
        public bool IsCancelled { get; set; } = false;
        public T Item { get; set; }
        public DialogBoxResult(DialogResponse response, T item)
        {
            switch (response)
            {
                case DialogResponse.Ok: IsOk = true; break;
                case DialogResponse.Cancel: IsCancelled = true; break;
            }
            Item = item;
        }
    }

    /// <summary>
    /// Closes the current dialog, if any.
    /// </summary>
    protected static void CloseDialog() => WeakReferenceMessenger.Default.Send<CloseDialogMessage>();

    /// <summary>
    /// Opens an edit dialog with the given initial viewmodel and returns the result when the dialog is closed.
    /// </summary>
    protected static Task<DialogBoxResult<T>> EditDialog<T>(T initial, string title)
        where T : ViewModelBase
    {
        var cs = new TaskCompletionSource<DialogBoxResult<T>>();
        EditDialogMessage message = new(initial, title, (vm, response) =>
        {
            if (vm is T item)
                cs.SetResult(new DialogBoxResult<T>(response, item));
            else
                throw new InvalidCastException($"Expected ViewModel of type {typeof(T).Name}, but got {vm.GetType().Name}.");
        });
        WeakReferenceMessenger.Default.Send(message);
        return cs.Task;
    }

    /// <summary>
    /// Opens the login dialog and returns the result when the dialog is closed.
    /// </summary>
    protected static Task<DialogResponse> ShowLoginDialog()
    {
        var cs = new TaskCompletionSource<DialogResponse>();
        ShowLoginDialogMessage message = new(response => cs.SetResult(response));
        WeakReferenceMessenger.Default.Send(message);
        return cs.Task;
    }

    /// <summary>
    /// Opens the signature dialog and returns the signature when the dialog is closed.
    /// </summary>
    protected static Task<string?> ShowSignDialog()
    {
        var cs = new TaskCompletionSource<string?>();
        ShowSignDialogMessage message = new(response => cs.SetResult(response));
        WeakReferenceMessenger.Default.Send(message);
        return cs.Task;
    }

    /// <summary>
    /// Shows the message in a message box
    /// </summary>
    protected static Task MessageBox(string msg)
    {
        var cs = new TaskCompletionSource<bool>();
        MessageBoxMessage messageBoxMessage = new(msg, () => cs.SetResult(true));
        WeakReferenceMessenger.Default.Send(messageBoxMessage);
        return cs.Task;
    }

    /// <summary>
    /// Shows a wait dialog
    /// </summary>
    protected static void BeginWait(string msg) => WeakReferenceMessenger.Default.Send(new BeginWaitMessage(msg));

    /// <summary>
    /// Hides the wait dialog
    /// </summary>
    protected static void EndWait() => WeakReferenceMessenger.Default.Send<EndWaitMessage>();
}
