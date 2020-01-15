using BitooBitImageEditor.ManipulationBitmap;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace BitooBitImageEditor.Helper
{
    internal static class SKCanvasExtension
    {
        internal static void DrawBitmap(this SKCanvas canvas, List<TouchManipulationBitmap> bitmapCollection, float transX = 0, float transY = 0, float scale = 1)
        {
            using (SKPaint paint = new SKPaint())
            {
                paint.IsAntialias = true;

                foreach (var item in bitmapCollection)
                {
                    if (item.Type != BitmapType.Main && !item.IsHide)
                    {
                        canvas.Save();
                        SKMatrix matrix = item.Matrix;
                        SKMatrix bitmapMatrix = new SKMatrix(matrix.ScaleX * scale, matrix.SkewX * scale, (matrix.TransX + transX) * scale, matrix.SkewY * scale, matrix.ScaleY * scale, (matrix.TransY + transY) * scale, 0, 0, 1);
                        //canvas.Concat(ref bitmapMatrix);
                        canvas.SetMatrix(bitmapMatrix);
                        canvas.DrawBitmap(item.Bitmap, 0, 0, paint);
                        canvas.Restore();
                    }
                }
            }

        }

        internal static void DrawBitmap(this SKCanvas canvas, SKBitmap mainBitmap, SKBitmap backgroundBitmap, SKRect rectImage, SKRect rect, ImageEditorConfig config)
        {
            using (SKPaint paint = new SKPaint())
            {
                paint.IsAntialias = true;

                if (config?.BackgroundType != BackgroundType.Transparent)
                {
                    paint.Color = config?.BackgroundType == BackgroundType.Color ? config.BackgroundColor : 0xFF1f1f1f;
                    paint.Style = SKPaintStyle.Fill;
                    canvas.DrawRect(rectImage, paint);
                }

                if (BackgroundType.StretchedImage == config?.BackgroundType)
                {
                    using (SKPaint paintStretch = paint.Clone())
                    {
                        float blur = 0.08f * Math.Max(rectImage.Width, rectImage.Height);
                        paintStretch.ImageFilter = SKImageFilter.CreateBlur(blur, blur);
                        canvas.DrawBitmap(backgroundBitmap, rectImage, paintStretch);
                    }
                }

                canvas.DrawBitmap(mainBitmap, rect, paint);
            }
        }


        internal static void DrawPath(this SKCanvas canvas, List<PaintedPath> completedPaths, Dictionary<long, PaintedPath> inProgressPaths)
        {
            using (SKPaint paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 10;
                paint.StrokeCap = SKStrokeCap.Round;
                paint.StrokeJoin = SKStrokeJoin.Round;
                paint.IsAntialias = true;


                if(completedPaths != null)
                    foreach (PaintedPath path in completedPaths)
                    {
                        paint.Color = path.Color;
                        canvas.DrawPath(path.Path, paint);
                    }

                if (inProgressPaths != null)
                    foreach (PaintedPath path in inProgressPaths?.Values)
                    {
                        paint.Color = path.Color;
                        canvas.DrawPath(path.Path, paint);
                    }
            }
        }


        internal static void DrawSurrounding(this SKCanvas canvas, SKRect outerRect, SKRect innerRect, SKColor color)
        {
            using (SKPaint paint = new SKPaint())
            {
                paint.Color = color;
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Fill;

                SKRect blackoutCropRectLeft = new SKRect(outerRect.Left, innerRect.Top, innerRect.Left, innerRect.Bottom);
                SKRect blackoutCropRectTop = new SKRect(outerRect.Left, outerRect.Top, outerRect.Right, innerRect.Top);
                SKRect blackoutCropRectRight = new SKRect(innerRect.Right, innerRect.Top, outerRect.Right, innerRect.Bottom);
                SKRect blackoutCropRectBottom = new SKRect(outerRect.Left, innerRect.Bottom, outerRect.Right, outerRect.Bottom);

                canvas.DrawRect(blackoutCropRectTop, paint);
                canvas.DrawRect(blackoutCropRectBottom, paint);
                canvas.DrawRect(blackoutCropRectLeft, paint);
                canvas.DrawRect(blackoutCropRectRight, paint);
            }
        }

    }
}


