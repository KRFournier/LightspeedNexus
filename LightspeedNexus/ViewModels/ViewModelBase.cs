using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using System.Threading.Tasks;
using System;

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
        public bool IsDeleted { get; set; } = false;
        public T Item { get; set; }
        public DialogBoxResult(OpenDialogMessage.DialogResponse response, T item)
        {
            switch (response)
            {
                case OpenDialogMessage.DialogResponse.Ok: IsOk = true; break;
                case OpenDialogMessage.DialogResponse.Cancel: IsCancelled = true; break;
                case OpenDialogMessage.DialogResponse.Delete: IsDeleted = true; break;
            }
            Item = item;
        }
    }

    /// <summary>
    /// Shows the message in a message box
    /// </summary>
    protected static void MessageBox(string msg)
    {
        MessageBoxMessage messageBoxMessage = new(msg);
        WeakReferenceMessenger.Default.Send(messageBoxMessage);
    }

    /// <summary>
    /// Opens a dialog with the given initial viewmodel and returns the result when the dialog is closed.
    /// </summary>
    protected static Task<DialogBoxResult<T>> DialogBox<T>(T initial, string title, params DialogButton[] additionalButtons)
        where T : ViewModelBase
    {
        var cs = new TaskCompletionSource<DialogBoxResult<T>>();
        OpenDialogMessage message = new(initial, title, additionalButtons, (vm, response) =>
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
    /// Closes the current dialog, if any.
    /// </summary>
    protected static void CloseDialog()
    {
        WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
    }

    protected static void BeginWait(string msg)
    {
        WeakReferenceMessenger.Default.Send(new BeginWaitMessage(msg));
    }

    protected static void EndWait()
    {
        WeakReferenceMessenger.Default.Send<EndWaitMessage>();
    }
}
