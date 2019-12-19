using BitooBitImageEditor.Helper;
using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace BitooBitImageEditor.Text
{
    internal class TextCanvasView : SKCanvasView
    {
        SKBitmap bitmap;
        //double angleR;
        TextRectangle textRect;
        SKMatrix inverseBitmapMatrix;
        string text = "";
        SKColor currentColor = Color.Black.ToSKColor();
        SKRect scaledCropRect = new SKRect();
        SKCanvas canvas;
        // Touch tracking  
        TouchEffect touchEffect = new TouchEffect();


        Dictionary<long, SKPoint> touchPoints = new Dictionary<long, SKPoint>();
        Dictionary<long, SKPoint> touchPointsInside = new Dictionary<long, SKPoint>();

        SKPoint bitmapLocationfirst = new SKPoint();
        SKPoint bitmapLocationlast = new SKPoint();

        internal TextCanvasView(SKBitmap bitmap)
        {
            this.bitmap = bitmap;
            textRect = new TextRectangle(new SKRect(0, 0, bitmap.Width, bitmap.Height));
            touchEffect.TouchAction += OnTouchEffectTouchAction;
        }



        internal SKColor CurrentColor
        {
            get => currentColor;
            set
            {
                currentColor = value;
                InvalidateSurface();
            }
        }

        internal string Text
        {
            get => text;
            set
            {
                text = value ?? "";
                InvalidateSurface();
            }
        }


        internal void SetBitmap(SKBitmap bitmap)
        {
            this.bitmap = bitmap;
            textRect = new TextRectangle(new SKRect(0, 0, bitmap.Width, bitmap.Height));
            InvalidateSurface();
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            // Attach TouchEffect to parent view
            Parent.Effects.Add(touchEffect);
        }


        internal SKBitmap BitmapWidthText
        {
            get
            {
                SKRect cropRect = textRect.maxRect;
                SKBitmap croppedBitmap = new SKBitmap((int)cropRect.Width, (int)cropRect.Height);
                SKRect dest = new SKRect(0, 0, cropRect.Width, cropRect.Height);
                SKRect source = new SKRect(cropRect.Left, cropRect.Top, cropRect.Right, cropRect.Bottom);
                using (SKCanvas canvas = new SKCanvas(croppedBitmap))
                {
                    canvas.DrawBitmap(bitmap, source, dest);


                    //SKRect rect = new SKRect(scaledCropRect.Left * scale, scaledCropRect.Top * scale, scaledCropRect.Right * scale, scaledCropRect.Bottom * scale) ;


                    SKRect rect = this.textRect.Rect;


                    canvas.DrawMultilineText(Text, currentColor, ref rect); 
                }
                return croppedBitmap;
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            //croppingRect.aspectRatio = aspectRatio =  textBounds.Width / textBounds.Height;

            base.OnPaintSurface(args);

            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;


            canvas.Clear(SkiaHelper.backgraundColor);

            // Calculate rectangle for displaying bitmap 
            var rect = SkiaHelper.CalculateRectangle(info, bitmap);
            canvas.DrawBitmap(bitmap, rect.rect);

            // Calculate a matrix transform for displaying the cropping rectangle
            SKMatrix bitmapScaleMatrix = SKMatrix.MakeIdentity();
            bitmapScaleMatrix.SetScaleTranslate(rect.scale, rect.scale, rect.left, rect.top);
            scaledCropRect = bitmapScaleMatrix.MapRect(this.textRect.Rect);

            canvas.DrawMultilineText(Text, currentColor, ref scaledCropRect);

            if (!String.IsNullOrWhiteSpace(text))
            {
                canvas.DrawRect(scaledCropRect, SkiaHelper.edgeStroke);
                float _radius = scaledCropRect.Width * 0.015f;
                float radius = _radius < 12 ? 12 : _radius;
                SKPaint cornerStroke = new SKPaint
                {
                    Style = SKPaintStyle.StrokeAndFill,
                    Color = SKColors.White
                };
                canvas.DrawOval(scaledCropRect.Right, scaledCropRect.Bottom, radius, radius, cornerStroke);
            }
            this.textRect.height = scaledCropRect.Height / rect.scale;

            SKRect blackoutCropRectTop = new SKRect(0, 0, info.Width, rect.top);
            canvas.DrawRect(blackoutCropRectTop, SkiaHelper.blackoutFill);

            SKRect blackoutCropRectBottom = new SKRect(0, rect.bottom, info.Width, info.Height);
            canvas.DrawRect(blackoutCropRectBottom, SkiaHelper.blackoutFill);

            SKRect blackoutCropRectLeft = new SKRect(0, 0, rect.left, rect.bottom);
            canvas.DrawRect(blackoutCropRectLeft, SkiaHelper.blackoutFill);

            SKRect blackoutCropRectRight = new SKRect(rect.right, rect.top, info.Width, rect.bottom);
            canvas.DrawRect(blackoutCropRectRight, SkiaHelper.blackoutFill);


            // Invert the transform for touch tracking
            bitmapScaleMatrix.TryInvert(out inverseBitmapMatrix);
        }

        void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
            SKPoint pixelLocation = SkiaHelper.ConvertToPixel(this, args.Location);
            SKPoint bitmapLocation = inverseBitmapMatrix.MapPoint(pixelLocation);

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    // Convert radius to bitmap/cropping scale
                    float radius = inverseBitmapMatrix.ScaleX * SkiaHelper.radius;


                    if (textRect.HitTest(bitmapLocation, radius) && !touchPoints.ContainsKey(args.Id))
                    {
                        touchPoints.Add(args.Id, bitmapLocation - textRect.Corner);
                    }
                    else if (textRect.TestPointInsideSquare(bitmapLocation) && !touchPointsInside.ContainsKey(args.Id))
                    {
                        touchPointsInside.Add(args.Id, bitmapLocation);
                        bitmapLocationfirst = bitmapLocation;
                    }
                    break;

                case TouchActionType.Moved:
                    if (touchPoints.ContainsKey(args.Id))
                    {
                        var touchPoint = touchPoints[args.Id];
                        textRect.MoveCorner(bitmapLocation - touchPoint);
                        InvalidateSurface();
                    }
                    if (touchPointsInside.ContainsKey(args.Id))
                    {
                        //Если перемещение соответсвует айдишнику от его кардинат вычитаем корадинаты начальной точки и передаем в метод перемещения
                        bitmapLocationlast = bitmapLocation;
                        SKPoint point = new SKPoint();
                        point.X = bitmapLocationlast.X - bitmapLocationfirst.X;
                        point.Y = bitmapLocationlast.Y - bitmapLocationfirst.Y;
                        textRect.MoveAllCorner(point);
                        bitmapLocationfirst = bitmapLocationlast;
                        InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    if (touchPoints.ContainsKey(args.Id))
                    {
                        touchPoints.Remove(args.Id);
                    }
                    if (touchPointsInside.ContainsKey(args.Id))
                    {
                        touchPointsInside.Remove(args.Id);
                    }
                    break;
            }
        }









    }
}
















//public static void DrawMultilineText(SKCanvas canvas, string text, SKColor color, o SKRect rect)
//{
//    SKPaint paint = new SKPaint
//    {
//        Color = color,
//        IsAntialias = true
//    };

//    //int emojiChar = StringUtilities.GetUnicodeCharacterCode("🚀", SKTextEncoding.Utf32);
//    //using (var emoji = SKTypeface.FromFamilyName("Noto Emoji"))
//    int emojiChar = 1087;
//    using (SKTypeface typeface = SKFontManager.Default.MatchCharacter(emojiChar))
//    {
//        paint.Typeface = typeface;
//        string[] lines = text.Split(new string[] { Environment.NewLine, "\r\n", "\n\r", "\r", "\n", "&#10;" }, StringSplitOptions.None);

//        float minTextSize = int.MaxValue;
//        for (int i = 0; i < lines.Length; i++)
//        {
//            float textWidth = paint.MeasureText(lines[i]);
//            float minTextSizecurrent = 0.95f * scaledCropRect.Width * paint.TextSize / textWidth;

//            if (minTextSizecurrent < minTextSize)
//                minTextSize = minTextSizecurrent;
//        }

//        paint.TextSize = minTextSize < 255 ? minTextSize : 255;

//        float maxTextHeight = 0;
//        float maxTextWidth = 0;
//        float[] linesWidth = new float[lines.Length];
//        for (int i = 0; i < lines.Length; i++)
//        {
//            SKRect currenttextBounds = new SKRect();
//            paint.MeasureText(lines[i], ref currenttextBounds);

//            linesWidth[i] = currenttextBounds.Width;
//            if (maxTextHeight < currenttextBounds.Height)
//                maxTextHeight = currenttextBounds.Height;

//            if (maxTextWidth < currenttextBounds.Width)
//                maxTextWidth = currenttextBounds.Width;
//        }

//        maxTextHeight = 1.2f * maxTextHeight;
//        float yText = scaledCropRect.Top;

//        for (int i = 0; i < lines.Length; i++)
//        {
//            float xText = scaledCropRect.MidX - (linesWidth[i] / 2);
//            canvas.DrawText(lines[i], xText, yText += maxTextHeight, paint);
//        }

//        this.textRect.height = ((lines.Length) * maxTextHeight) / rect.scale;
//    }

//    var textRect = this.textRect.Rect;
//    textRect.Bottom = textRect.Top + this.textRect.height;
//    scaledCropRect = bitmapScaleMatrix.MapRect(textRect);
//    if (!String.IsNullOrWhiteSpace(Text))
//    {
//        canvas.DrawRect(scaledCropRect, SkiaHelper.edgeStroke);
//        float _radius = textRect.Width * 0.015f;
//        float radius = _radius < 15 ? 15 : _radius;
//        SKPaint cornerStroke = new SKPaint
//        {
//            Style = SKPaintStyle.StrokeAndFill,
//            Color = SKColors.White
//        };
//        canvas.DrawOval(scaledCropRect.Right, scaledCropRect.Bottom, radius, radius, cornerStroke);
//    }

//}