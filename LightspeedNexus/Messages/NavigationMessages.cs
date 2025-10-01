using CommunityToolkit.Mvvm.Messaging.Messages;
using LightspeedNexus.ViewModels;
using System;

namespace LightspeedNexus.Messages;

/// <summary>
/// Additional buttons that may be added to the dialog.
/// </summary>
public enum DialogButton
{
    Delete
}

/// <summary>
/// Notifies the main view that the current page should be set to the given viewmodel
/// </summary>
public class NavigatePageMessage(ViewModelBase viewModel)
{
    public ViewModelBase Page = viewModel;
}

/// <summary>
/// Notifies the main view that the current page should be set to the home viewmodel
/// </summary>
public class NavigateHomeMessage()
{
}

/// <summary>
/// Notifies the main view that the given viewmodel should be opened as a dialog
/// </summary>
public class OpenDialogMessage(
    ViewModelBase item,
    string title,
    DialogButton[] additionalButtons,
    Action<ViewModelBase, OpenDialogMessage.DialogResponse> handler
    )
{
    /// <summary>
    /// Possible responses from the dialog
    /// </summary>
    public enum DialogResponse
    {
        None,
        Ok,
        Cancel,
        Delete
    }

    /// <summary>
    /// The initial content for the dialog
    /// </summary>
    public readonly ViewModelBase Item = item;

    /// <summary>
    /// The dialog box title
    /// </summary>
    public readonly string Title = title;

    /// <summary>
    /// Whether the dialog should show a delete button
    /// </summary>
    public readonly DialogButton[] AdditionalButtons = additionalButtons;

    /// <summary>
    /// Final response from the dialog
    /// </summary>
    private readonly Action<ViewModelBase, DialogResponse> _handler = handler;

    /// <summary>
    /// Respond to the caller
    /// </summary>
    public void Respond(DialogResponse response) => _handler(Item, response);
}

/// <summary>
/// Notifies the main view to close the current dialog
/// </summary>
public class CloseDialogMessage()
{
}

/// <summary>
/// Notifies the main view that the given message should be displayed
/// </summary>
public class MessageBoxMessage(string msg) : ValueChangedMessage<string>(msg)
{
}
