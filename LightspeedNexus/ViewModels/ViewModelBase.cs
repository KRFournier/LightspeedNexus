using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;

namespace LightspeedNexus.ViewModels;

public class ViewModelBase : ObservableObject
{
    protected void MessageBox(string msg)
    {
        MessageBoxMessage messageBoxMessage = new(msg);
        WeakReferenceMessenger.Default.Send(messageBoxMessage);
    }
}
