using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Utilities;
using Avalonia;
using System;
using System.Globalization;
using Avalonia.Media.TextFormatting;
using Avalonia.Controls.Documents;

namespace LightspeedNexus.Controls;

/// <summary>
/// A custom border for Lightspeed Nexus.
/// </summary>
public class TextControlBase : Control
{
    /// <summary>
    /// Defines the <see cref="FontFamily"/> property.
    /// </summary>
    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<TimeBar>();

    /// <summary>
    /// Defines the <see cref="FontSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<TimeBar>();

    /// <summary>
    /// Defines the <see cref="FontStyle"/> property.
    /// </summary>
    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        TextElement.FontStyleProperty.AddOwner<TimeBar>();

    /// <summary>
    /// Defines the <see cref="FontWeight"/> property.
    /// </summary>
    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        TextElement.FontWeightProperty.AddOwner<TimeBar>();

    /// <summary>
    /// Defines the <see cref="FontStretch"/> property.
    /// </summary>
    public static readonly StyledProperty<FontStretch> FontStretchProperty =
        TextElement.FontStretchProperty.AddOwner<TimeBar>();

    /// <summary>
    /// Defines the <see cref="Foreground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<TimeBar>();

    /// <summary>
    /// Defines the <see cref="FontFeatures"/> property.
    /// </summary>
    public static readonly StyledProperty<FontFeatureCollection?> FontFeaturesProperty =
        TextElement.FontFeaturesProperty.AddOwner<TextBlock>();

    /// <summary>
    /// Defines the <see cref="LineHeight"/> property.
    /// </summary>
    public static readonly AttachedProperty<double> LineHeightProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, Control, double>(
            nameof(LineHeight),
            double.NaN,
            validate: IsValidLineHeight,
            inherits: true);

    /// <summary>
    /// Defines the <see cref="LineSpacing"/> property.
    /// </summary>
    public static readonly AttachedProperty<double> LineSpacingProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, Control, double>(
            nameof(LineSpacing),
            0,
            validate: IsValidLineSpacing,
            inherits: true);

    /// <summary>
    /// Defines the <see cref="LetterSpacing"/> property.
    /// </summary>
    public static readonly AttachedProperty<double> LetterSpacingProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, Control, double>(
            nameof(LetterSpacing),
            0,
            inherits: true);

    /// <summary>
    /// Defines the <see cref="MaxLines"/> property.
    /// </summary>
    public static readonly AttachedProperty<int> MaxLinesProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, Control, int>(
            nameof(MaxLines),
            validate: IsValidMaxLines,
            inherits: true);

    /// <summary>
    /// Gets or sets the font family used to draw the control's text.
    /// </summary>
    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of the control's text in points.
    /// </summary>
    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the font style used to draw the control's text.
    /// </summary>
    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the font weight used to draw the control's text.
    /// </summary>
    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the font stretch used to draw the control's text.
    /// </summary>
    public FontStretch FontStretch
    {
        get => GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to draw the control's text and other foreground elements.
    /// </summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the font features.
    /// </summary>
    public FontFeatureCollection? FontFeatures
    {
        get => GetValue(FontFeaturesProperty);
        set => SetValue(FontFeaturesProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of each line of content.
    /// </summary>
    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the extra distance of each line to the next line.
    /// </summary>
    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the letter spacing.
    /// </summary>
    public double LetterSpacing
    {
        get => GetValue(LetterSpacingProperty);
        set => SetValue(LetterSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of text lines.
    /// </summary>
    public int MaxLines
    {
        get => GetValue(MaxLinesProperty);
        set => SetValue(MaxLinesProperty, value);
    }

    private static bool IsValidMaxLines(int maxLines) => maxLines >= 0;

    private static bool IsValidLineHeight(double lineHeight) => double.IsNaN(lineHeight) || lineHeight > 0;

    private static bool IsValidLineSpacing(double lineSpacing) => !double.IsNaN(lineSpacing) && !double.IsInfinity(lineSpacing);

    /// <summary>
    /// Creates the <see cref="TextLayout"/> used to render the text.
    /// </summary>
    /// <returns>A <see cref="TextLayout"/> object.</returns>
    protected virtual TextLayout CreateTextLayout(string? text, Size bounds, TextAlignment alignment)
    {
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

        var defaultProperties = new GenericTextRunProperties(
            typeface,
            FontFeatures,
            FontSize,
            [],
            Foreground);

        var paragraphProperties = new GenericTextParagraphProperties(FlowDirection, alignment, true, false,
            defaultProperties, TextWrapping.NoWrap, LineHeight, 0, LetterSpacing);

        return new TextLayout(
            new SimpleTextSource(text ?? "", defaultProperties),
            paragraphProperties,
            TextTrimming.None,
            bounds.Width,
            bounds.Height,
            MaxLines);
    }

    /// <summary>
    /// Invalidates <see cref="TextLayout"/>.
    /// </summary>
    protected void InvalidateTextLayout()
    {
        InvalidateVisual();
        InvalidateMeasure();
    }

    protected readonly record struct SimpleTextSource : ITextSource
    {
        private readonly string _text;
        private readonly TextRunProperties _defaultProperties;

        public SimpleTextSource(string text, TextRunProperties defaultProperties)
        {
            _text = text;
            _defaultProperties = defaultProperties;
        }

        public TextRun? GetTextRun(int textSourceIndex)
        {
            if (textSourceIndex > _text.Length)
            {
                return new TextEndOfParagraph();
            }

            var runText = _text.AsMemory(textSourceIndex);

            if (runText.IsEmpty)
            {
                return new TextEndOfParagraph();
            }

            return new TextCharacters(runText, _defaultProperties);
        }
    }
}