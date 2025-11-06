using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using LightspeedNexus.Controls;
using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Views;

public partial class RosterView : UserControl
{
    public RosterView()
    {
        InitializeComponent();
        RosterSearch.AttachedToVisualTree += (_, _) => Dispatcher.UIThread.Post(() => RosterSearch.Focus());
        AddHandler(DragDrop.DragOverEvent, DragOver);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private Point _offset;

    private async void RosterPlayer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Border item && DataContext is RosterViewModel rosterViewModel && !rosterViewModel.IsStarted)
        {
            var viewModel = item.DataContext as ContestantViewModel;

            GhostItem.Width = item.Bounds.Width;
            GhostItem.Height = item.Bounds.Height;
            var mousePos = e.GetPosition(DropPanel);
            _offset = e.GetPosition(item);
            GhostItem.RenderTransform = new TranslateTransform(mousePos.X - _offset.X, mousePos.Y - _offset.Y);

            rosterViewModel.DraggingPlayer = viewModel;
            GhostItem.IsVisible = true;
            await DragDrop.DoDragDrop(e, new DataObject(), DragDropEffects.Move);
            GhostItem.IsVisible = false;
            GhostItem.RenderTransform = new TranslateTransform(0, 0);
            rosterViewModel.DraggingPlayer = null;
        }
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        var mousePos = e.GetPosition(DropPanel);
        GhostItem.RenderTransform = new TranslateTransform(mousePos.X - _offset.X, mousePos.Y - _offset.Y);
        if (e.Source is Border item && DataContext is RosterViewModel rosterViewModel)
        {
            if (item.DataContext is ContestantViewModel target)
                rosterViewModel.DropOnPlayer(target);
        }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (DataContext is RosterViewModel rosterViewModel)
            rosterViewModel.IsAuto = false;
    }

    public void Player_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (sender is LightspeedBorder border && border.DataContext is ContestantViewModel player)
        {
            if (this.DataContext is RosterViewModel vm && vm.EditPlayerCommand.CanExecute(player))
                vm.EditPlayerCommand.Execute(player);
        }
    }
}