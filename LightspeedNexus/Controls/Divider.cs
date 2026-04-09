using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Lightspeed.Utilities;

namespace LightspeedNexus.Controls;

/// <summary>
/// A control which decorates a child with a border and background.
/// </summary>
public class Divider : Control
{
    /// <summary>
    /// Defines the <see cref="LineBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> LineBrushProperty =
        AvaloniaProperty.Register<Divider, IBrush?>(nameof(LineBrush));

    /// <summary>
    /// Defines the <see cref="LineThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> LineThicknessProperty =
        AvaloniaProperty.Register<Divider, Thickness>(nameof(LineThickness));

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Divider, Orientation>(nameof(Orientation), Orientation.Vertical);

    private Thickness? _layoutThickness;
    private double _scale;

    /// <summary>
    /// Initializes static members of the <see cref="Border"/> class.
    /// </summary>
    static Divider()
    {
        AffectsRender<Border>(
            LineBrushProperty,
            LineThicknessProperty,
            OrientationProperty);
        AffectsMeasure<Border>(LineThicknessProperty, OrientationProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        switch (change.Property.Name)
        {
            case nameof(UseLayoutRounding):
            case nameof(LineThickness):
                _layoutThickness = null;
                break;
        }
    }

    /// <summary>
    /// Gets or sets a brush with which to paint the lines in the top half
    /// </summary>
    public IBrush? LineBrush
    {
        get => GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of the lines.
    /// </summary>
    public Thickness LineThickness
    {
        get => GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    protected Thickness LayoutThickness
    {
        get
        {
            VerifyScale();

            if (_layoutThickness == null)
            {
                var borderThickness = LineThickness;

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

        double halfHeight = rect.Height / 2.0;
        double halfWidth = rect.Width / 2.0;

        LineGeometry g = Orientation switch
        {
            Orientation.Horizontal => new(new Point(rect.Left, rect.Top + halfHeight), new Point(rect.Right, rect.Top + halfHeight)),
            _ => new(new Point(rect.Left + halfWidth, rect.Top), new Point(rect.Left + halfWidth, rect.Bottom))
        };

        context.DrawGeometry(null, new Pen(LineBrush, thickness), g);
    }
}