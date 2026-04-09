using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Lightspeed.Utilities;

namespace LightspeedNexus.Controls;

/// <summary>
/// A control which decorates a child with a border and background.
/// </summary>
public class BracketLines : Control
{
    /// <summary>
    /// Defines the <see cref="LineBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> LineBrushProperty =
        AvaloniaProperty.Register<BracketLines, IBrush?>(nameof(LineBrush));

    /// <summary>
    /// Defines the <see cref="IsTopVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTopVisibleProperty =
        AvaloniaProperty.Register<BracketLines, bool>(nameof(IsTopVisible), true);

    /// <summary>
    /// Defines the <see cref="IsBottomVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBottomVisibleProperty =
        AvaloniaProperty.Register<BracketLines, bool>(nameof(IsBottomVisible), true);

    /// <summary>
    /// Defines the <see cref="LineThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> LineThicknessProperty =
        AvaloniaProperty.Register<BracketLines, Thickness>(nameof(LineThickness));

    /// <summary>
    /// Defines the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<Border, CornerRadius>(nameof(CornerRadius), new(10.0));

    private Thickness? _layoutThickness;
    private double _scale;

    /// <summary>
    /// Initializes static members of the <see cref="Border"/> class.
    /// </summary>
    static BracketLines()
    {
        AffectsRender<Border>(
            LineBrushProperty,
            LineThicknessProperty,
            CornerRadiusProperty);
        AffectsMeasure<Border>(LineThicknessProperty);
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
    /// Determines if the top half is visible
    /// </summary>
    public bool IsTopVisible
    {
        get => GetValue(IsTopVisibleProperty);
        set => SetValue(IsTopVisibleProperty, value);
    }

    /// <summary>
    /// Determines if the bottom half is visible
    /// </summary>
    public bool IsBottomVisible
    {
        get => GetValue(IsBottomVisibleProperty);
        set => SetValue(IsBottomVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of the lines.
    /// </summary>
    public Thickness LineThickness
    {
        get => GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the radius of the border rounded corners.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
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
        if (!IsTopVisible && !IsBottomVisible)
            return;

        var thickness = LayoutThickness.Top;

        var rect = new Rect(Bounds.Size);
        if (!MathUtilities.IsZero(thickness))
            rect = rect.Deflate(thickness * 0.5);

        double quarterHeight = rect.Height / 4.0;
        double halfHeight = rect.Height / 2.0;
        double halfWidth = rect.Width / 2.0;

        PathGeometry g = new();
        using (var ctx = g.Open())
        {
            if (IsTopVisible && IsBottomVisible)
            {
                ctx.BeginFigure(new Point(rect.Left, rect.Top + quarterHeight), false);
                ctx.LineTo(new Point(rect.Left + halfWidth, rect.Top + quarterHeight));
                ctx.LineTo(new Point(rect.Left + halfWidth, rect.Top + (quarterHeight * 3)));
                ctx.LineTo(new Point(rect.Left, rect.Top + (quarterHeight * 3)));
                ctx.EndFigure(false);
                ctx.BeginFigure(new Point(rect.Left + halfWidth, rect.Top + halfHeight), false);
                ctx.LineTo(new Point(rect.Right, rect.Top + halfHeight));
                ctx.EndFigure(false);
            }
            else if (IsTopVisible)
            {
                ctx.BeginFigure(new Point(rect.Left, rect.Top + quarterHeight), false);
                ctx.LineTo(new Point(rect.Left + halfWidth, rect.Top + quarterHeight));
                ctx.LineTo(new Point(rect.Left + halfWidth, rect.Top + halfHeight));
                ctx.LineTo(new Point(rect.Right, rect.Top + halfHeight));
                ctx.EndFigure(false);
            }
            else if (IsBottomVisible)
            {
                ctx.BeginFigure(new Point(rect.Left, rect.Bottom - quarterHeight), false);
                ctx.LineTo(new Point(rect.Left + halfWidth, rect.Bottom - quarterHeight));
                ctx.LineTo(new Point(rect.Left + halfWidth, rect.Bottom - halfHeight));
                ctx.LineTo(new Point(rect.Right, rect.Bottom - halfHeight));
                ctx.EndFigure(false);
            }
        }

        context.DrawGeometry(null, new Pen(LineBrush, thickness), g);
    }
}