using CommunityToolkit.Mvvm.Messaging.Messages;
using LightspeedNexus.ViewModels;
using System;

namespace LightspeedNexus.Messages;

/// <summary>
/// Notifies the main view that the current page should be set to the given viewmodel
/// </summary>
public class NavigatePageMessage(ViewModelBase viewModel) : ValueChangedMessage<ViewModelBase>(viewModel)
{
}

/// <summary>
/// Notifies the main view that the given viewmodel should be opened as a dialog
/// </summary>
public class OpenDialogMessage(ViewModelBase init, Action<ViewModelBase> handler)
{
    /// <summary>
    /// The initial content for the dialog
    /// </summary>
    public readonly ViewModelBase Initial = init;

    /// <summary>
    /// Final response from the dialog
    /// </summary>
    public Action<ViewModelBase> Reply = handler;
}

/// <summary>
/// Notifies the main view that the given message should be displayed
/// </summary>
public class MessageBoxMessage(string msg) : ValueChangedMessage<string>(msg)
{
}
