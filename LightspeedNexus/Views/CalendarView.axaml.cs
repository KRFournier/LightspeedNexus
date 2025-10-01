using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Layout;
using LightspeedNexus.Controls;
using System;
using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Views;

public partial class CalendarView : UserControl
{
    public CalendarView()
    {
        InitializeComponent();
        DragCanvas.AddHandler(DragDrop.DragOverEvent, DragOver, handledEventsToo: true);
        DragCanvas.AddHandler(DragDrop.DropEvent, Drop, handledEventsToo: true);
    }

    #region Drag and Drop

    private Point _offset;
    private LightspeedBorder? _sourceItem = null;
    private LightspeedBorder? _dragItem = null;

    private async void NewEvent_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is LightspeedBorder item && item.Child is TextBlock itemText)
        {
            var textblock = new TextBlock()
            {
                Text = itemText.Text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false
            };

            _dragItem = new LightspeedBorder()
            {
                CornerGap = new Thickness(item.CornerGap.Left, item.CornerGap.Top, item.CornerGap.Right, item.CornerGap.Bottom),
                CornerThickness = new Thickness(item.CornerThickness.Left, item.CornerThickness.Top, item.CornerThickness.Right, item.CornerThickness.Bottom),
                Width = item.Bounds.Width,
                Height = item.Bounds.Height,
                BorderBrush = item.BorderBrush,
                Background = item.Background,
                IsHitTestVisible = false,
                Opacity = 0.5,
                Child = textblock
            };

            var mousePos = e.GetPosition(DragCanvas);
            _offset = e.GetPosition(item);
            _dragItem.RenderTransform = new TranslateTransform(mousePos.X - _offset.X, mousePos.Y - _offset.Y);

            _sourceItem = item;
            DragCanvas.IsHitTestVisible = true;
            DragCanvas.Children.Add(_dragItem);
            await DragDrop.DoDragDrop(e, new DataObject(), DragDropEffects.Move);
            DragCanvas.Children.Remove(_dragItem);
            DragCanvas.IsHitTestVisible = false;

            _sourceItem = null;
            _dragItem = null;
        }
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        if (_dragItem is not null && _sourceItem is not null)
        {
            var mousePos = e.GetPosition(Schedule);
            if (mousePos.X < 0 || mousePos.Y < 0 || mousePos.X > Schedule.Bounds.Width || mousePos.Y > Schedule.Bounds.Height)
            {
                mousePos = e.GetPosition(DragCanvas);
                _dragItem.Width = _sourceItem.Bounds.Width;
                _dragItem.Height = _sourceItem.Bounds.Height;
                _dragItem.RenderTransform = new TranslateTransform(mousePos.X - _offset.X, mousePos.Y - _offset.Y);
            }
            else
            {
                mousePos = SchedulePanel.PointToClient(Schedule.PointToScreen(mousePos));
                var rect = SchedulePanel.PointToRect(mousePos, TimeSpan.FromHours(1));
                var topLeft = DragCanvas.PointToClient(SchedulePanel.PointToScreen(rect.TopLeft));
                _dragItem.Width = rect.Width;
                _dragItem.Height = rect.Height;
                _dragItem.RenderTransform = new TranslateTransform(topLeft.X, topLeft.Y);
            }
        }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (_dragItem is not null && _sourceItem is not null && DataContext is CalendarViewModel cvm)
        {
            var mousePos = e.GetPosition(Schedule);
            if (mousePos.X >= 0 || mousePos.Y >= 0 || mousePos.X <= Schedule.Bounds.Width || mousePos.Y <= Schedule.Bounds.Height)
            {
                mousePos = SchedulePanel.PointToClient(Schedule.PointToScreen(mousePos));
                var duration = TimeSpan.FromHours(1);
                (var day, var time) = SchedulePanel.PointToAppt(mousePos, duration);
                cvm.AddEvent(_sourceItem.Child is TextBlock tb && tb.Text is not null ? tb.Text : "New Event",
                    day,
                    time,
                    duration
                    );
            }
        }
    }
    #endregion
}