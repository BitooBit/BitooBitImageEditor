using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
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
            StrokeWidth = 3
        };

        internal readonly static SKPaint blackoutFill = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Gray.WithAlpha((byte)(0xFF * 0.5)),
            StrokeWidth = 2
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
    }
}
