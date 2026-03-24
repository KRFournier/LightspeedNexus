using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Utilities;

namespace LightspeedNexus.Controls;

/// <summary>
/// A custom border for Lightspeed Nexus.
/// </summary>
public class LightspeedBorder : Decorator
{
    /// <summary>
    /// Defines the <see cref="Background"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BackgroundProperty =
        AvaloniaProperty.Register<Border, IBrush>(nameof(Background), Brushes.Black);

    /// <summary>
    /// Defines the <see cref="BorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(BorderBrush));

    /// <summary>
    /// Defines the <see cref="BorderThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<Border, Thickness>(nameof(BorderThickness), new Thickness(1.0));

    /// <summary>
    /// Defines the <see cref="CornerThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> CornerThicknessProperty =
        AvaloniaProperty.Register<Border, Thickness>(nameof(CornerThickness), new(10.0));

    /// <summary>
    /// Defines the <see cref="CornerGap"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> CornerGapProperty =
        AvaloniaProperty.Register<Border, Thickness>(nameof(CornerGap), new(0.0));

    /// <summary>
    /// Defines the <see cref="TopLeftBorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TopLeftBorderBrushProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(TopLeftBorderBrush), null);

    /// <summary>
    /// Defines the <see cref="TopLeftBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TopLeftBackgroundProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(TopLeftBackground), null);

    /// <summary>
    /// Defines the <see cref="TopLeftOpacity"/> property.
    /// </summary>
    public static readonly StyledProperty<double> TopLeftOpacityProperty =
        AvaloniaProperty.Register<Border, double>(nameof(TopLeftOpacity), 1.0);

    /// <summary>
    /// Defines the <see cref="BottomRightBorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BottomRightBorderBrushProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(BottomRightBorderBrush), null);

    /// <summary>
    /// Defines the <see cref="BottomRightBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BottomRightBackgroundProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(BottomRightBackground), null);

    /// <summary>
    /// Defines the <see cref="BottomRightOpacity"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BottomRightOpacityProperty =
        AvaloniaProperty.Register<Border, double>(nameof(BottomRightOpacity), 1.0);

    /// <summary>
    /// Defines the <see cref="ProgressBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ProgressBrushProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(ProgressBrush), null);

    /// <summary>
    /// Defines the <see cref="Progress"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<Border, double>(nameof(Progress), 0.0);

    private Thickness? _layoutThickness;
    private double _scale;

    /// <summary>
    /// Initializes static members of the <see cref="Border"/> class.
    /// </summary>
    static LightspeedBorder()
    {
        AffectsRender<LightspeedBorder>(
            BackgroundProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            CornerThicknessProperty,
            CornerGapProperty,
            TopLeftBorderBrushProperty,
            TopLeftBackgroundProperty,
            TopLeftOpacityProperty,
            BottomRightBorderBrushProperty,
            BottomRightBackgroundProperty,
            BottomRightOpacityProperty,
            ProgressBrushProperty,
            ProgressProperty
            );
        AffectsMeasure<LightspeedBorder>(
            CornerThicknessProperty,
            CornerGapProperty,
            BorderThicknessProperty);
    }

    /// <summary>
    /// Used to force a layout pass when the border thickness changes.
    /// </summary>
    /// <param name="change"></param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        switch (change.Property.Name)
        {
            case nameof(UseLayoutRounding):
            case nameof(BorderThickness):
                _layoutThickness = null;
                break;
        }
    }

    /// <summary>
    /// Gets or sets a color with which to paint the background.
    /// </summary>
    public IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets a brush with which to paint the border.
    /// </summary>
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of the border.
    /// </summary>
    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of the topleft and bottomright corners.
    /// </summary>
    public Thickness CornerThickness
    {
        get => GetValue(CornerThicknessProperty);
        set => SetValue(CornerThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the gap between the corners and the border.
    /// </summary>
    public Thickness CornerGap
    {
        get => GetValue(CornerGapProperty);
        set => SetValue(CornerGapProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush with which to paint the top left corner.
    /// </summary>
    public IBrush? TopLeftBorderBrush
    {
        get => GetValue(TopLeftBorderBrushProperty);
        set => SetValue(TopLeftBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the color of the top left corner.
    /// </summary>
    public IBrush? TopLeftBackground
    {
        get => GetValue(TopLeftBackgroundProperty);
        set => SetValue(TopLeftBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the opacity level of the top left corner.
    /// </summary>
    public double TopLeftOpacity
    {
        get => GetValue(TopLeftOpacityProperty);
        set => SetValue(TopLeftOpacityProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush with which to paint the top left corner. 
    /// </summary>
    public IBrush? BottomRightBorderBrush
    {
        get => GetValue(BottomRightBorderBrushProperty);
        set => SetValue(BottomRightBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the color of the top left corner.
    /// </summary>
    public IBrush? BottomRightBackground
    {
        get => GetValue(BottomRightBackgroundProperty);
        set => SetValue(BottomRightBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the opacity level of the top left corner.
    /// </summary>
    public double BottomRightOpacity
    {
        get => GetValue(BottomRightOpacityProperty);
        set => SetValue(BottomRightOpacityProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to paint the progress
    /// </summary>
    public IBrush? ProgressBrush
    {
        get => GetValue(ProgressBrushProperty);
        set => SetValue(ProgressBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the progress value (0.0 to 1.0).
    /// </summary>
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    protected Thickness LayoutThickness
    {
        get
        {
            VerifyScale();

            if (_layoutThickness == null)
            {
                var borderThickness = BorderThickness;

                if (UseLayoutRounding)
                    borderThickness = LayoutHelper.RoundLayoutThickness(borderThickness, _scale, _scale);

                _layoutThickness = borderThickness;
            }

            return _layoutThickness.Value;
        }
    }

    private void VerifyScale()
    {
        var currentScale = LayoutHelper.GetLayoutScale(this);
        if (MathUtilities.AreClose(currentScale, _scale))
            return;

        _scale = currentScale;
        _layoutThickness = null;
    }

    /// <summary>
    /// Renders the control.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    public override void Render(DrawingContext context)
    {
        var thickness = LayoutThickness.Top;

        var rect = new Rect(Bounds.Size);
        if (!MathUtilities.IsZero(thickness))
            rect = rect.Deflate(thickness * 0.5);

        PathGeometry g = new();
        using (var ctx = g.Open())
        {
            ctx.BeginFigure(new Point(rect.Left + CornerThickness.Top + CornerGap.Top, rect.Top), true);
            ctx.LineTo(new Point(rect.Right, rect.Top));
            ctx.LineTo(new Point(rect.Right, rect.Bottom - CornerThickness.Right - CornerGap.Right));
            ctx.LineTo(new Point(rect.Right - CornerThickness.Bottom - CornerGap.Bottom, rect.Bottom));
            ctx.LineTo(new Point(rect.Left, rect.Bottom));
            ctx.LineTo(new Point(rect.Left, rect.Top + CornerThickness.Left + CornerGap.Left));
            ctx.EndFigure(true);
        }
        context.DrawGeometry(Background, new Pen(BorderBrush, thickness), g);

        if (Progress > 0.0 && ProgressBrush != null)
        {
            Rect progressRect = new(rect.Left, rect.Top, rect.Width * Progress, rect.Height);
            using var clip = context.PushClip(progressRect);
            context.DrawGeometry(ProgressBrush, null, g);
        }

        PathGeometry gTopLeft = new();
        using (var ctx = gTopLeft.Open())
        {
            ctx.BeginFigure(new Point(rect.Left, rect.Top), true);
            ctx.LineTo(new Point(rect.Left + CornerThickness.Top, rect.Top));
            ctx.LineTo(new Point(rect.Left, rect.Top + CornerThickness.Left));
            ctx.EndFigure(true);
        }
        using (var state = context.PushOpacity(BottomRightOpacity))
        {
            context.DrawGeometry(TopLeftBackground, new Pen(TopLeftBorderBrush, thickness), gTopLeft);
        }

        PathGeometry gBottomRight = new();
        using (var ctx = gBottomRight.Open())
        {
            ctx.BeginFigure(new Point(rect.Right, rect.Bottom), true);
            ctx.LineTo(new Point(rect.Right - CornerThickness.Bottom, rect.Bottom));
            ctx.LineTo(new Point(rect.Right, rect.Bottom - CornerThickness.Right));
            ctx.EndFigure(true);
        }
        using (var state = context.PushOpacity(BottomRightOpacity))
        {
            context.DrawGeometry(BottomRightBackground, new Pen(BottomRightBorderBrush, thickness), gBottomRight);
        }
    }

    /// <summary>
    /// Measures the control.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size availableSize) => LayoutHelper.MeasureChild(Child, availableSize, Padding, BorderThickness);

    /// <summary>
    /// Arranges the control's child.
    /// </summary>
    /// <param name="finalSize">The size allocated to the control.</param>
    /// <returns>The space taken.</returns>
    protected override Size ArrangeOverride(Size finalSize) => LayoutHelper.ArrangeChild(Child, finalSize, Padding, BorderThickness);
}