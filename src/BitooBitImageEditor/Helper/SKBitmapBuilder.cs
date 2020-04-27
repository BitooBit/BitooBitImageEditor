using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BitooBitImageEditor.Helper
{
    internal static class SKBitmapBuilder
    {
        private static readonly string[] splitters = new string[] { Environment.NewLine, "\r\n", "\n\r", "\r", "\n", "&#10;" };

        internal static SKBitmap GetBlurBitmap(SKBitmap bitmap, SKRect rect)
        {
            SKBitmap outBitmap = new SKBitmap((int)rect.Width, (int)rect.Height);
            using (SKBitmap tempBitmap = new SKBitmap(CalcBackgraundBitmapsize(bitmap.Width), CalcBackgraundBitmapsize(bitmap.Height)))
            using (SKCanvas canvas = new SKCanvas(outBitmap))
            using (SKPaint paint = new SKPaint())
            {
                bitmap.ScalePixels(tempBitmap, SKFilterQuality.Low);
                paint.IsAntialias = true;
                float blur = 0.08f * Math.Max(rect.Width, rect.Height);
                blur = blur < 100 ? blur : 100;
                paint.ImageFilter = SKImageFilter.CreateBlur(blur, blur);
                canvas.Clear();
                canvas.DrawBitmap(tempBitmap, rect, paint);
            }
            GC.Collect(0);
            return outBitmap;
        }


        internal static SKBitmap FromText(string text, SKColor color, bool isDrawRect)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    using (SKPaint paint = new SKPaint())
                    {
                        paint.Color = color;
                        paint.SubpixelText = true;
                        paint.IsEmbeddedBitmapText = true;
                        paint.IsAntialias = true;
                        paint.TextEncoding = SKTextEncoding.Utf32;
                        paint.TextSize = 255;

                        float height;
                        string[] lines = text.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                        string[][] chars = new string[lines.Length][];
                        float[][] charsWidth = new float[chars.Length][];
                        SKTypeface[][] charsTypeface = new SKTypeface[chars.Length][];
                        float[] linesWidth = new float[chars.Length];
                        float maxLineHeight = 0;
                        float maxLineWidth = 0;


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

                            linesWidth[i] = 0;
                            for (int j = 0; j < chars[i].Length; j++)
                            {
                                using (SKPaint charPaint = paint.Clone())
                                {
                                    int numberChar = 120;
                                    char[] currentChar = chars[i][j].ToCharArray();

                                    if (currentChar?.Length > 1 && !(currentChar[1] >= 55296 && currentChar[1] <= 57000)) //checking highSurrogate
                                        currentChar = new char[] { currentChar[0] };

                                    switch (currentChar?.Length)
                                    {
                                        case 1:
                                            numberChar = Char.ConvertToUtf32(chars[i][j], 0);
                                            break;
                                        case 2:
                                            numberChar = Char.ConvertToUtf32(currentChar[0], currentChar[1]);
                                            break;
                                        case 0:
                                            chars[i][j] = $"";
                                            break;
                                        default:
                                            numberChar = Char.ConvertToUtf32(currentChar[0], currentChar[1]);
                                            chars[i][j] = $"{currentChar[0]}{currentChar[1]}";
                                            break;
                                    }

                                    charPaint.Typeface = charsTypeface[i][j] = SKFontManager.Default.MatchCharacter(numberChar);
                                    SKRect currenttextBounds = new SKRect();
                                    charsWidth[i][j] = charPaint.MeasureText(chars[i][j], ref currenttextBounds);
                                    linesWidth[i] += charsWidth[i][j];

                                    if (maxLineHeight < currenttextBounds.Height)
                                        maxLineHeight = currenttextBounds.Height;
                                }

                                if (maxLineWidth < linesWidth[i])
                                    maxLineWidth = linesWidth[i];
                            }
                        }

                        currentLineChars = null;
                        maxLineHeight = (float)Math.Ceiling(maxLineHeight * 1.15);
                        maxLineWidth = (float)Math.Ceiling(maxLineWidth * 1.05);
                        height = (float)Math.Ceiling((chars.Length + 0.32f) * maxLineHeight);

                        SKBitmap textBitmap = new SKBitmap((int)maxLineWidth, (int)height);
                        SKRect textDest = new SKRect(0, 0, maxLineWidth, height);
                        using (SKCanvas canvasText = new SKCanvas(textBitmap))
                        {
                            canvasText?.Clear();
                            canvasText.DrawBitmap(textBitmap, textDest);

                            if (isDrawRect)
                                using (var paintRect = paint.Clone())
                                using (var roundRect = new SKRoundRect(new SKRect(0, 0, maxLineWidth, height), 50, 50))
                                {
                                    paintRect.Style = SKPaintStyle.Fill;
                                    paintRect.Color = color;
                                    canvasText.DrawRoundRect(roundRect, paintRect);
                                }

                            float yText = maxLineHeight;


                            for (int i = 0; i < chars.Length; i++)
                            {
                                float xText = maxLineWidth / 2 - (linesWidth[i] / 2);

                                for (int j = 0; j < chars[i].Length; j++)
                                {
                                    using (SKPaint charPaint = paint.Clone())
                                    {
                                        if (isDrawRect)
                                            charPaint.Color = color == SKColors.White ? SKColors.Black : SKColors.White;

                                        charPaint.Typeface = charsTypeface[i][j];
                                        canvasText.DrawText(chars[i][j], xText, yText, charPaint);
                                        xText += charsWidth[i][j];
                                    }
                                }

                                yText += maxLineHeight;
                            }

                            
                        }

                        foreach (var a in charsTypeface)
                            foreach (var b in a)
                                b.Dispose();

                        GC.Collect(0);

                        return textBitmap;
                    }
                }
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static int CalcBackgraundBitmapsize(float value)
        {
            int _value = (int)(value * 0.006f);
            return _value > 2 ? _value : 2;
        }

    }
}

