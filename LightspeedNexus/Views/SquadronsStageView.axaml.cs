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

public partial class SquadronsStageView : UserControl
{
    public SquadronsStageView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragOverEvent, DragOver);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private Point _offset;

    private async void RosterPlayer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Border item && DataContext is SquadronsStageViewModel SquadronsStageViewModel)
        {
            var viewModel = item.DataContext as ParticipantViewModel;

            GhostItem.Width = item.Bounds.Width;
            GhostItem.Height = item.Bounds.Height;
            var mousePos = e.GetPosition(DropPanel);
            _offset = e.GetPosition(item);
            GhostItem.RenderTransform = new TranslateTransform(mousePos.X - _offset.X, mousePos.Y - _offset.Y);

            SquadronsStageViewModel.DraggingParticipant = viewModel;
            GhostItem.IsVisible = true;
            await DragDrop.DoDragDrop(e, new DataObject(), DragDropEffects.Move);
            GhostItem.IsVisible = false;
            GhostItem.RenderTransform = new TranslateTransform(0, 0);
            SquadronsStageViewModel.DraggingParticipant = null;
        }
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        var mousePos = e.GetPosition(DropPanel);
        GhostItem.RenderTransform = new TranslateTransform(mousePos.X - _offset.X, mousePos.Y - _offset.Y);
        if (e.Source is Border item && DataContext is SquadronsStageViewModel SquadronsStageViewModel)
        {
            if (item.DataContext is ParticipantViewModel target)
                SquadronsStageViewModel.DropOnPlayer(target);
        }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (DataContext is SquadronsStageViewModel SquadronsStageViewModel)
            SquadronsStageViewModel.IsAutoAssigned = false;
    }
}