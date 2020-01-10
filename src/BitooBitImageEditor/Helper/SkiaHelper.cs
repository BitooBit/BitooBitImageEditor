using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;

namespace BitooBitImageEditor
{
    internal static class SkiaHelper
    {
        internal const int corner = 30;      // pixel length of cropper corner
        internal const int radius = 50;     // pixel radius of touch hit-test
        internal readonly static SKColor backgraundColor = Color.FromHex("#eeeeee").ToSKColor();

        internal readonly static SKPaint cornerStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White,
            StrokeWidth = 7
        };

        internal readonly static SKPaint edgeStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White,
            StrokeWidth = 3,
            IsAntialias = true
        };

        internal readonly static SKPaint blackoutFill = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Gray.WithAlpha((byte)(0xFF * 0.5)),
            StrokeWidth = 2
        };


        internal readonly static SKPaint smallPoint = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            Color = SKColors.Red,
            StrokeWidth = 7
        };


        internal static (SKRect rect, float scale, float left, float top, float right, float bottom) CalculateRectangle(SKImageInfo info, SKBitmap bitmap)
        {
            float scale = Math.Min((float)info.Width / bitmap.Width, (float)info.Height / bitmap.Height);
            float left = (info.Width - scale * bitmap.Width) / 2;
            float top = (info.Height - scale * bitmap.Height) / 2;
            float right = left + scale * bitmap.Width;
            float bottom = top + scale * bitmap.Height;
            return (new SKRect(left, top, right, bottom), scale, left, top, right, bottom);
        }


        internal static SKPoint ConvertToPixel(SKCanvasView canvasView, Xamarin.Forms.Point pt)
        {
            return new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width), (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));
        }



        private static readonly string[] splitters = new string[] { Environment.NewLine, "\r\n", "\n\r", "\r", "\n", "&#10;" };

        internal static SKBitmap DrawTextOnBitmap(string text, SKColor color)
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
                        maxLineHeight = (float)Math.Ceiling((double)maxLineHeight * 1.2);
                        maxLineWidth = (float)Math.Ceiling((double)maxLineWidth * 1.2);
                        height = (float)Math.Ceiling((double)((chars.Length + 0.32f) * maxLineHeight));

                        SKBitmap textBitmap = new SKBitmap((int)maxLineWidth, (int)height);
                        SKRect textDest = new SKRect(0, 0, maxLineWidth, height);
                        using (SKCanvas canvasText = new SKCanvas(textBitmap))
                        {
                            canvasText.DrawBitmap(textBitmap, textDest);

                            float yText = maxLineHeight;


                            for (int i = 0; i < chars.Length; i++)
                            {
                                float xText = maxLineWidth / 2 - (linesWidth[i] / 2);

                                for (int j = 0; j < chars[i].Length; j++)
                                {
                                    using (SKPaint charPaint = paint.Clone())
                                    {
                                        charPaint.Typeface = charsTypeface[i][j];
                                        canvasText.DrawText(chars[i][j], xText, yText, charPaint);
                                        xText += charsWidth[i][j];
                                        //charsTypeface[i][j].Dispose();
                                    }
                                }

                                yText += maxLineHeight;
                            }

                            canvasText.DrawRect(new SKRect(0,0, maxLineWidth, height), SkiaHelper.edgeStroke);
                        }

                        foreach (var a in charsTypeface)
                            foreach (var b in a)
                                b.Dispose();


                        return textBitmap;
                    }
                }
                else
                    return new SKBitmap();
            }
            catch (Exception ex)
            {
                return new SKBitmap();
            }
        }









        internal static SKPoint[] RotatePoint(SKPoint center, double degrees, params SKPoint[] points)
        {
            double angel = (degrees / 180D) * Math.PI;
            double cos = Math.Cos(angel);
            double sin = Math.Sin(angel);
            SKPoint[] rotatePoints = new SKPoint[points.Length];

            for (int i = 0; i < points.Length; i++)
                rotatePoints[i] = RotatePoint(center, cos, sin, points[i]);

            return rotatePoints;
        }


        internal static SKPoint RotatePoint(SKPoint center, double degrees, SKPoint point)
        {
            double angel = (degrees / 180D) * Math.PI;
            return RotatePoint(center, Math.Cos(angel), Math.Sin(angel), point);
        }

        internal static SKPoint RotatePoint(SKPoint center, double cos, double sin, SKPoint point)
        {
            double x = (center.X + (point.X - center.X) * cos - (point.Y - center.Y) * sin);
            double y = (center.Y + (point.X - center.X) * sin + (point.Y - center.Y) * cos);
            return new SKPoint((float)x, (float)y);
        }



        internal static bool CheckPointInsideTriangle(SKPoint point, SKPoint triangle1, SKPoint triangle2, SKPoint triangle3)
        {
            float a1 = (triangle1.X - point.X) * (triangle2.Y - triangle1.Y) - (triangle2.X - triangle1.X) * (triangle1.Y - point.Y);
            float a2 = (triangle2.X - point.X) * (triangle3.Y - triangle2.Y) - (triangle3.X - triangle2.X) * (triangle2.Y - point.Y);
            float a3 = (triangle3.X - point.X) * (triangle1.Y - triangle3.Y) - (triangle1.X - triangle3.X) * (triangle3.Y - point.Y);

            if (a1 == 0 || a2 == 0 || a3 == 0)
                return true;

            a1 /= Math.Abs(a1);
            a2 /= Math.Abs(a2);
            a3 /= Math.Abs(a3);

            if (a1 == a2 && a2 == a3)
                return true;

            else
                return false;

        }




    }
}
