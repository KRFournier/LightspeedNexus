using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Utilities;

namespace LightspeedNexus.Controls;

/// <summary>
/// A custom border for Lightspeed Nexus.
/// </summary>
public class SimpleLightspeedBorder : Decorator
{
    /// <summary>
    /// Defines the <see cref="Brush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BrushProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(Brush));

    /// <summary>
    /// Defines the <see cref="BackgroundOpacity"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BackgroundOpacityProperty =
        AvaloniaProperty.Register<Border, double>(nameof(BackgroundOpacity), 1.0);

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
    /// Defines the <see cref="CornerOpacity"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CornerOpacityProperty =
        AvaloniaProperty.Register<Border, double>(nameof(CornerOpacity), 1.0);

    private Thickness? _layoutThickness;
    private double _scale;

    /// <summary>
    /// Initializes static members of the <see cref="Border"/> class.
    /// </summary>
    static SimpleLightspeedBorder()
    {
        AffectsRender<SimpleLightspeedBorder>(
            BrushProperty,
            BorderThicknessProperty,
            CornerThicknessProperty,
            CornerGapProperty,
            CornerOpacityProperty
            );
        AffectsMeasure<SimpleLightspeedBorder>(
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
    /// Gets or sets a brush with which to paint the borders and backgrounds.
    /// </summary>
    public IBrush? Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the opacity level of the background.
    /// </summary>
    public double BackgroundOpacity
    {
        get => GetValue(BackgroundOpacityProperty);
        set => SetValue(BackgroundOpacityProperty, value);
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
    /// Gets or sets the opacity level of the top left corner.
    /// </summary>
    public double CornerOpacity
    {
        get => GetValue(CornerOpacityProperty);
        set => SetValue(CornerOpacityProperty, value);
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

        PathGeometry g = new();
        using (var ctx = g.Open())
        {
            var bounds = !MathUtilities.IsZero(thickness) ? rect.Deflate(thickness * 0.5) : rect;
            ctx.BeginFigure(new Point(bounds.Left + CornerThickness.Top + CornerGap.Top, bounds.Top), true);
            ctx.LineTo(new Point(bounds.Right, bounds.Top));
            ctx.LineTo(new Point(bounds.Right, bounds.Bottom - CornerThickness.Right - CornerGap.Right));
            ctx.LineTo(new Point(bounds.Right - CornerThickness.Bottom - CornerGap.Bottom, bounds.Bottom));
            ctx.LineTo(new Point(bounds.Left, bounds.Bottom));
            ctx.LineTo(new Point(bounds.Left, bounds.Top + CornerThickness.Left + CornerGap.Left));
            ctx.EndFigure(true);
        }
        context.DrawGeometry(new SolidColorBrush(Colors.Black, 0.4), new Pen(Brush, thickness), g);

        PathGeometry gCorners = new();
        using (var ctx = gCorners.Open())
        {
            ctx.BeginFigure(new Point(rect.Left, rect.Top), true);
            ctx.LineTo(new Point(rect.Left + CornerThickness.Top + thickness, rect.Top));
            ctx.LineTo(new Point(rect.Left, rect.Top + CornerThickness.Left + thickness));
            ctx.EndFigure(true);

            ctx.BeginFigure(new Point(rect.Right, rect.Bottom), true);
            ctx.LineTo(new Point(rect.Right - CornerThickness.Bottom - thickness, rect.Bottom));
            ctx.LineTo(new Point(rect.Right, rect.Bottom - CornerThickness.Right - thickness));
            ctx.EndFigure(true);
        }
        using var state = context.PushOpacity(CornerOpacity);
        context.DrawGeometry(Brush, new Pen(Brushes.Transparent, 0.0), gCorners);
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