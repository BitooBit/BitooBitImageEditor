using SkiaSharp;
using System;
using System.Text;

namespace BitooBitImageEditor.Helper
{
    internal static class SKCanvasExtension
    {
        private const float maxSize = 255;




        internal static void DrawMultilineText(this SKCanvas canvas, string text, SKColor color, ref SKRect rect)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    SKPaint paint = new SKPaint
                    {
                        Color = color,
                        IsAntialias = true
                    };
                    float height;
                    int emojiChardfvdf = StringUtilities.GetUnicodeCharacterCode("🚀", SKTextEncoding.Utf32);
                    int emojiChar = 1087;
                    using (SKTypeface typeface = SKFontManager.Default.MatchCharacter(emojiChar))
                    {
                        paint.Typeface = typeface;
                        string[] lines = text.Split(new string[] { Environment.NewLine, "\r\n", "\n\r", "\r", "\n", "&#10;" }, StringSplitOptions.RemoveEmptyEntries);

                        char[][] chars = new char[lines.Length][];
                        float[][] charsWidth = new float[lines.Length][];

                        SKTypeface[][] charsTypeface = new SKTypeface[lines.Length][];



                        float minTextSize = int.MaxValue;
                        for (int i = 0; i < lines.Length; i++)
                        {
                            chars[i] = lines[i].ToCharArray();
                            charsTypeface[i] = new SKTypeface[lines[i].Length];

                            float widthLine = 0;
                            for (int j = 0; j < lines[i].Length; j++)
                            {
                                SKPaint charPaint = paint.Clone();
                                string ggg = chars[i][j].ToString();

                                int emojiChar1=0;
                                try
                                {
                                    emojiChar1 = Char.ConvertToUtf32(chars[i][j].ToString(), 0);

                                    //emojiChar1 = StringUtilities.GetUnicodeCharacterCode(ggg, SKTextEncoding.Utf32);
                                }
                                catch(Exception ex)
                                {
                                    emojiChar1 = 1087;
                                }


                                charPaint.Typeface = charsTypeface[i][j] = SKFontManager.Default.MatchCharacter(emojiChar1);
                                float charWidth = charPaint.MeasureText(chars[i][j].ToString());
                                widthLine += charWidth;
                            }

                            float minTextSizeCurrent = 0.95f * rect.Width * paint.TextSize / widthLine;
                            if (minTextSizeCurrent < minTextSize)
                                minTextSize = minTextSizeCurrent;
                        }

                        paint.TextSize = minTextSize < maxSize ? minTextSize : maxSize;



                        float maxTextHeight = 0;
                        float[] linesWidth = new float[lines.Length];
                        for (int i = 0; i < lines.Length; i++)
                        {
                            linesWidth[i] = 0;
                            charsWidth[i] = new float[lines[i].Length];
                            for (int j = 0; j < lines[i].Length; j++)
                            {
                                SKPaint charPaint = paint.Clone();
                                charPaint.Typeface = charsTypeface[i][j];
                                SKRect currenttextBounds = new SKRect();
                                charPaint.MeasureText(chars[i][j].ToString(), ref currenttextBounds);

                                if (maxTextHeight < currenttextBounds.Height)
                                    maxTextHeight = currenttextBounds.Height;
                                linesWidth[i] += currenttextBounds.Width;
                                charsWidth[i][j] = currenttextBounds.Width;
                            }
                        }

                        maxTextHeight = 1.2f * maxTextHeight;
                        float yText = rect.Top;

                        for (int i = 0; i < lines.Length; i++)
                        {
                            float xText = rect.MidX - (linesWidth[i] / 2);

                            for (int j = 0; j < lines[i].Length; j++)
                            {
                                SKPaint charPaint = paint.Clone();
                                charPaint.Typeface = charsTypeface[i][j];
                                canvas.DrawText(chars[i][j].ToString(), xText += (1.2f * charsWidth[i][j]), yText, charPaint);
                            }

                            yText += maxTextHeight;
                        }
                        height = (lines.Length + 0.32f) * maxTextHeight;
                    }

                    rect.Bottom = rect.Top + height;
                }
            }
            catch(Exception ex)
            {

            }
        }






        //internal static void DrawMultilineText(this SKCanvas canvas, string text, SKColor color, ref SKRect rect)
        //{
        //    if (!string.IsNullOrWhiteSpace(text))
        //    {
        //        SKPaint paint = new SKPaint
        //        {
        //            Color = color,
        //            IsAntialias = true
        //        };
        //        float height;
        //        //int emojiChar = StringUtilities.GetUnicodeCharacterCode("🚀", SKTextEncoding.Utf32);
        //        //using (var emoji = SKTypeface.FromFamilyName("Noto Emoji"))
        //        int emojiChar = 1087;
        //        using (SKTypeface typeface = SKFontManager.Default.MatchCharacter(emojiChar))
        //        {
        //            paint.Typeface = typeface;
        //            string[] lines = text.Split(new string[] { Environment.NewLine, "\r\n", "\n\r", "\r", "\n", "&#10;" }, StringSplitOptions.None);

        //            float minTextSize = int.MaxValue;
        //            for (int i = 0; i < lines.Length; i++)
        //            {
        //                float textWidth = paint.MeasureText(lines[i]);
        //                float minTextSizecurrent = 0.95f * rect.Width * paint.TextSize / textWidth;

        //                if (minTextSizecurrent < minTextSize)
        //                    minTextSize = minTextSizecurrent;
        //            }

        //            paint.TextSize = minTextSize < maxSize ? minTextSize : maxSize;

        //            float maxTextHeight = 0;
        //            float maxTextWidth = 0;
        //            float[] linesWidth = new float[lines.Length];
        //            for (int i = 0; i < lines.Length; i++)
        //            {
        //                SKRect currenttextBounds = new SKRect();
        //                paint.MeasureText(lines[i], ref currenttextBounds);

        //                linesWidth[i] = currenttextBounds.Width;
        //                if (maxTextHeight < currenttextBounds.Height)
        //                    maxTextHeight = currenttextBounds.Height;

        //                if (maxTextWidth < currenttextBounds.Width)
        //                    maxTextWidth = currenttextBounds.Width;
        //            }

        //            maxTextHeight = 1.2f * maxTextHeight;
        //            float yText = rect.Top;

        //            for (int i = 0; i < lines.Length; i++)
        //            {
        //                float xText = rect.MidX - (linesWidth[i] / 2);
        //                canvas.DrawText(lines[i], xText, yText += maxTextHeight, paint);
        //            }
        //            height = (lines.Length + 0.32f) * maxTextHeight;
        //        }

        //        rect.Bottom = rect.Top + height;
        //    }
        //}
    }
}
