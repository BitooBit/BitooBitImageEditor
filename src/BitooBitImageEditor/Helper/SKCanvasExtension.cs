using SkiaSharp;

namespace BitooBitImageEditor.Helper
{
    internal static class SKCanvasExtension
    {

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


