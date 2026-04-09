using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Lightspeed.Utilities;

namespace LightspeedNexus.Controls;

/// <summary>
/// A custom border for Lightspeed Nexus.
/// </summary>
public class ChevronBorder : Decorator
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
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Border, Orientation>(nameof(Orientation), Orientation.Vertical);

    /// <summary>
    /// Defines the <see cref="ArrowDepth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ArrowDepthProperty =
        AvaloniaProperty.Register<Border, double>(nameof(ArrowDepth), 0.0);


    private Thickness? _layoutThickness;
    private double _scale;

    /// <summary>
    /// Initializes static members of the <see cref="Border"/> class.
    /// </summary>
    static ChevronBorder()
    {
        AffectsRender<ChevronBorder>(
            BackgroundProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            OrientationProperty
            );
        AffectsMeasure<ChevronBorder>(
            BorderThicknessProperty,
            OrientationProperty);
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
    /// Chevrons point down (vertical) or to the right (horizontal).
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// The depth of the arrow points.
    /// </summary>
    public double ArrowDepth
    {
        get => GetValue(ArrowDepthProperty);
        set => SetValue(ArrowDepthProperty, value);
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
                    borderThickness = LayoutHelper.RoundLayoutThickness(borderThickness, _scale);

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

        if (Orientation == Orientation.Vertical)
        {
            // down chevron
            using var ctx = g.Open();
            ctx.BeginFigure(new Point(rect.Left, rect.Top), true);
            ctx.LineTo(new Point(rect.Left + (rect.Width / 2.0), rect.Top + ArrowDepth));
            ctx.LineTo(new Point(rect.Right, rect.Top));
            ctx.LineTo(new Point(rect.Right, rect.Bottom - ArrowDepth));
            ctx.LineTo(new Point(rect.Left + (rect.Width / 2.0), rect.Bottom));
            ctx.LineTo(new Point(rect.Left, rect.Bottom - ArrowDepth));
            ctx.EndFigure(true);
        }
        else
        {
            // right chevron
            using var ctx = g.Open();
            ctx.BeginFigure(new Point(rect.Left, rect.Top), true);
            ctx.LineTo(new Point(rect.Right - ArrowDepth, rect.Top));
            ctx.LineTo(new Point(rect.Right, rect.Top + (rect.Height / 2.0)));
            ctx.LineTo(new Point(rect.Right - ArrowDepth, rect.Bottom));
            ctx.LineTo(new Point(rect.Left, rect.Bottom));
            ctx.LineTo(new Point(rect.Left + ArrowDepth, rect.Top + (rect.Height / 2.0)));
            ctx.EndFigure(true);
        }

        context.DrawGeometry(Background, new Pen(BorderBrush, thickness), g);
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