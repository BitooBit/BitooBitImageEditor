using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BitooBitImageEditor.Helper
{
    internal static class SKCanvasExtension
    {
        private const float maxSize = 255;
        private static readonly string[] splitters = new string[] { Environment.NewLine, "\r\n", "\n\r", "\r", "\n", "&#10;" };

        internal static void DrawMultilineText(this SKCanvas canvas, string text, SKColor color, ref SKRect rect)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    SKPaint paint = new SKPaint
                    {
                        Color = color
                    };

                    float height;
                    string[] lines = text.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                    string[][] chars = new string[lines.Length][];
                    float[][] charsWidth = new float[chars.Length][];
                    SKTypeface[][] charsTypeface = new SKTypeface[chars.Length][];
                    float[] linesWidth = new float[chars.Length];
                    float maxTextHeight = 0;
                    float minTextSize = int.MaxValue;


                    List<string> currentLineChars = new List<string>();
                    for (int i = 0; i < chars.Length; i++)
                    {
                        TextElementEnumerator strEnumerator = StringInfo.GetTextElementEnumerator(lines[i]);
                        while (strEnumerator.MoveNext())
                            currentLineChars.Add(strEnumerator.GetTextElement());

                        chars[i] = currentLineChars.ToArray();
                        currentLineChars.Clear();

                        charsTypeface[i] = new SKTypeface[chars[i].Length];
                        charsWidth[i] = new float[chars[i].Length];

                        float lineWidth = 0;
                        for (int j = 0; j < chars[i].Length; j++)
                        {
                            using (SKPaint charPaint = paint.Clone())
                            {
                                int numberChar = 120;
                                char[] currentChar = chars[i][j].ToCharArray();

                                switch (currentChar?.Length)
                                {
                                    case 1:
                                        numberChar = Char.ConvertToUtf32(chars[i][j], 0);
                                        break;
                                    case 2:
                                        numberChar = Char.ConvertToUtf32(currentChar[0], currentChar[1]);
                                        break;
                                    case 0:
                                        numberChar = 120;
                                        chars[i][j] = $"";
                                        break;
                                    default:
                                        numberChar = Char.ConvertToUtf32(currentChar[0], currentChar[1]);
                                        chars[i][j] = $"{currentChar[0]}{currentChar[1]}";
                                        break;
                                }

                                charPaint.Typeface = charsTypeface[i][j] = SKFontManager.Default.MatchCharacter(numberChar);
                                lineWidth += charPaint.MeasureText(chars[i][j]);
                            }
                        }

                        float minTextSizeCurrent = 0.95f * rect.Width * paint.TextSize / lineWidth;
                        if (minTextSizeCurrent < minTextSize)
                            minTextSize = minTextSizeCurrent;
                    }

                    currentLineChars = null;
                    paint.TextSize = minTextSize < maxSize ? minTextSize : maxSize;

                    for (int i = 0; i < chars.Length; i++)
                    {
                        linesWidth[i] = 0;
                        for (int j = 0; j < chars[i].Length; j++)
                        {
                            using (SKPaint charPaint = paint.Clone())
                            {
                                charPaint.Typeface = charsTypeface[i][j];
                                SKRect currenttextBounds = new SKRect();
                                charsWidth[i][j] = charPaint.MeasureText(chars[i][j], ref currenttextBounds);

                                if (maxTextHeight < currenttextBounds.Height)
                                    maxTextHeight = currenttextBounds.Height;

                                linesWidth[i] += charsWidth[i][j];
                            }
                        }
                    }

                    maxTextHeight = 1.2f * maxTextHeight;
                    float yText = rect.Top + maxTextHeight;

                    for (int i = 0; i < chars.Length; i++)
                    {
                        float xText = rect.MidX - (linesWidth[i] / 2);

                        for (int j = 0; j < chars[i].Length; j++)
                        {
                            using (SKPaint charPaint = paint.Clone())
                            {
                                charPaint.Typeface = charsTypeface[i][j];
                                canvas.DrawText(chars[i][j], xText, yText, charPaint);
                                xText += charsWidth[i][j];
                            }
                        }

                        yText += maxTextHeight;
                    }
                    height = (chars.Length + 0.32f) * maxTextHeight;

                    rect.Bottom = rect.Top + height;
                }
            }
            catch { }
        }



    }
}
