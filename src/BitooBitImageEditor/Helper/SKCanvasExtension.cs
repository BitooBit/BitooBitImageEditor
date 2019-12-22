using SkiaSharp;
using System;

namespace BitooBitImageEditor.Helper
{
    internal static class SKCanvasExtension
    {
        internal static void DrawMultilineText(this SKCanvas canvas, string text, SKColor color, ref SKRect rect)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                SKPaint paint = new SKPaint
                {
                    Color = color,
                    IsAntialias = true
                };
                float height;
                //int emojiChar = StringUtilities.GetUnicodeCharacterCode("🚀", SKTextEncoding.Utf32);
                //using (var emoji = SKTypeface.FromFamilyName("Noto Emoji"))
                int emojiChar = 1087;
                using (SKTypeface typeface = SKFontManager.Default.MatchCharacter(emojiChar))
                {
                    paint.Typeface = typeface;
                    string[] lines = text.Split(new string[] { Environment.NewLine, "\r\n", "\n\r", "\r", "\n", "&#10;" }, StringSplitOptions.None);

                    float minTextSize = int.MaxValue;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        float textWidth = paint.MeasureText(lines[i]);
                        float minTextSizecurrent = 0.95f * rect.Width * paint.TextSize / textWidth;

                        if (minTextSizecurrent < minTextSize)
                            minTextSize = minTextSizecurrent;
                    }

                    paint.TextSize = minTextSize < 255 ? minTextSize : 255;

                    float maxTextHeight = 0;
                    float maxTextWidth = 0;
                    float[] linesWidth = new float[lines.Length];
                    for (int i = 0; i < lines.Length; i++)
                    {
                        SKRect currenttextBounds = new SKRect();
                        paint.MeasureText(lines[i], ref currenttextBounds);

                        linesWidth[i] = currenttextBounds.Width;
                        if (maxTextHeight < currenttextBounds.Height)
                            maxTextHeight = currenttextBounds.Height;

                        if (maxTextWidth < currenttextBounds.Width)
                            maxTextWidth = currenttextBounds.Width;
                    }

                    maxTextHeight = 1.2f * maxTextHeight;
                    float yText = rect.Top;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        float xText = rect.MidX - (linesWidth[i] / 2);
                        canvas.DrawText(lines[i], xText, yText += maxTextHeight, paint);
                    }
                    height = (lines.Length + 0.32f) * maxTextHeight;
                }

                rect.Bottom = rect.Top + height;
            }
        }
    }
}
