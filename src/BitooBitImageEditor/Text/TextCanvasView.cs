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

        // Touch tracking  

        SKImageInfo Info { get; set; } = new SKImageInfo();
        SKRect ImgRect { get; set; } = new SKRect();


        Dictionary<long, SKPoint> touchPoints = new Dictionary<long, SKPoint>();
        Dictionary<long, SKPoint> touchPointsInside = new Dictionary<long, SKPoint>();

        SKPoint bitmapLocationfirst = new SKPoint();
        SKPoint bitmapLocationlast = new SKPoint();

        internal TextCanvasView(SKBitmap bitmap)
        {
            this.bitmap = bitmap;
            textRect = new TextRectangle(new SKRect(0, 0, bitmap.Width, bitmap.Height));
            
        }

        internal bool IsActive { get; set; }

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


        }





        internal SKBitmap BitmapWidthText
        {
            get
            {

                //SKBitmap textBitmap = new SKBitmap((int)textRect.Rect.Width, (int)textRect.Rect.Height);
                //SKRect textDest = new SKRect(0, 0, textRect.Rect.Width, textRect.Rect.Height);
                //using (SKCanvas canvas = new SKCanvas(textBitmap))
                //{
                //    canvas.DrawBitmap(textBitmap, textDest);
                //    canvas.DrawMultilineText(Text, currentColor, ref textDest);
                //}
            








                SKRect cropRect = textRect.maxRect;
                SKBitmap croppedBitmap = new SKBitmap((int)cropRect.Width, (int)cropRect.Height);
                SKRect dest = new SKRect(0, 0, cropRect.Width, cropRect.Height);
                SKRect source = new SKRect(cropRect.Left, cropRect.Top, cropRect.Right, cropRect.Bottom);              
                using (SKCanvas canvas = new SKCanvas(croppedBitmap))
                {
                    canvas.DrawBitmap(bitmap, source, dest);


                    SKRect rect = textRect.Rect;

                    canvas.Save();
                    canvas.Translate(rect.MidX, rect.MidY);
                    canvas.RotateDegrees((float)textRect.angel);
                    SKRect rectangle = new SKRect(-rect.Width / 2f, -rect.Height / 2f, rect.Width / 2f, rect.Height / 2f);

                    canvas.DrawMultilineText(Text, currentColor, ref rectangle);
                    //canvas.DrawBitmap(textBitmap, rectangle.Left, rectangle.Top);
                    canvas.Restore();

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
            Info = info;

            canvas.Clear(SkiaHelper.backgraundColor);

            // Calculate rectangle for displaying bitmap 
            var rect = SkiaHelper.CalculateRectangle(info, bitmap);
            canvas.DrawBitmap(bitmap, rect.rect);
            ImgRect = rect.rect;




            // Calculate a matrix transform for displaying the cropping rectangle
            SKMatrix bitmapScaleMatrix = SKMatrix.MakeIdentity();
            bitmapScaleMatrix.SetScaleTranslate(rect.scale, rect.scale, rect.left, rect.top);
            scaledCropRect = bitmapScaleMatrix.MapRect(this.textRect.Rect);


            SKBitmap bitmapText = SkiaHelper.DrawTextOnBitmap(Text, currentColor);


            canvas.DrawCircle(0, 0, 5, SkiaHelper.smallPoint);

            canvas.Save();
            canvas.Translate(scaledCropRect.MidX, scaledCropRect.MidY);


            SKRect rectangle = new SKRect(-scaledCropRect.Width / 2f, -scaledCropRect.Height / 2f, scaledCropRect.Width / 2f, scaledCropRect.Height / 2f);

            canvas.RotateDegrees((float)textRect.angel);


            canvas.DrawCircle(0, 0, 5, SkiaHelper.smallPoint);


            float scale = rectangle.Width / bitmapText.Width;
            canvas.Save();

            canvas.Scale(scale);




            canvas.DrawBitmap(bitmapText, rectangle.Left, rectangle.Top, SkiaHelper.edgeStroke);

            //canvas.DrawMultilineText(Text, currentColor, ref rectangle);





            canvas.Restore();





            scaledCropRect.Bottom = scaledCropRect.Top + rectangle.Height;

            if (!String.IsNullOrWhiteSpace(text))
            {
                canvas.DrawRect(rectangle, SkiaHelper.edgeStroke);
                float _radius = scaledCropRect.Width * 0.015f;
                float radius = _radius < 12 ? 12 : _radius;
                SKPaint cornerStroke = new SKPaint
                {
                    Style = SKPaintStyle.StrokeAndFill,
                    Color = SKColors.White
                };
                canvas.DrawOval(rectangle.Right, rectangle.Bottom, radius, radius, cornerStroke);
            }



            canvas.Restore();



            //if (!String.IsNullOrWhiteSpace(text))
            //{
            //    var point = SkiaHelper.RotatePoint(new SKPoint (scaledCropRect.MidX, scaledCropRect.MidY), textRect.angel, new SKPoint(scaledCropRect.Right, scaledCropRect.Bottom));


            //    float radius = 6;
            //    SKPaint cornerStroke = new SKPaint
            //    {
            //        Style = SKPaintStyle.StrokeAndFill,
            //        Color = SKColors.Red
            //    };
            //    canvas.DrawOval(point.X, point.Y, radius, radius, cornerStroke);
            //}


            canvas.DrawSurrounding(new SKRect(0, 0, info.Width, info.Height), rect.rect, SKColors.DarkGray.WithAlpha((byte)(0xFF * 0.5)));

            // Invert the transform for touch tracking
            bitmapScaleMatrix.TryInvert(out inverseBitmapMatrix);
        }

        internal void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
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
