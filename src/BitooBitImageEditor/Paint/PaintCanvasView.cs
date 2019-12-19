using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BitooBitImageEditor.Croping
{

    internal class LinePath 
    {
        internal SKPath path;
        internal SKColor color;

        public LinePath(SKPath path, SKColor color)
        {
            this.path = path;
            this.color = color;
        }
    }

    internal class PaintCanvasView : SKCanvasView
    {

        TouchEffect touchEffect = new TouchEffect();


        internal Dictionary<long, LinePath> inProgressPaths = new Dictionary<long, LinePath>();
        internal List<LinePath> completedPaths = new List<LinePath>();


        SKBitmap bitmap;

        internal SKColor CurrentColor { get; set; }

        internal PaintCanvasView(SKBitmap bitmap) : base()
        {
            this.bitmap = bitmap;
            touchEffect.TouchAction += OnTouchEffectTouchAction;
        }


        protected override void OnParentSet()
        {
            base.OnParentSet();

            // Attach TouchEffect to parent view
            Parent.Effects.Add(touchEffect);
        }




        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            base.OnPaintSurface(args);
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();
            canvas.DrawBitmap(bitmap, SkiaHelper.CalculateRectangle(args.Info, bitmap).rect);

            

            foreach (LinePath line in completedPaths)
            {
                SKPaint paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = line.color,
                    StrokeWidth = 10,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeJoin = SKStrokeJoin.Round
                };
                canvas.DrawPath(line.path, paint);
            }

            foreach (LinePath line in inProgressPaths.Values)
            {
                SKPaint paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = line.color,
                    StrokeWidth = 10,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeJoin = SKStrokeJoin.Round
                };
                canvas.DrawPath(line.path, paint);
            }

        }

        void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = new SKPath();
                        path.MoveTo(SkiaHelper.ConvertToPixel(this, args.Location));
                        inProgressPaths.Add(args.Id, new LinePath(path, CurrentColor));
                        InvalidateSurface();
                    }
                    break;

                case TouchActionType.Moved:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        var line = inProgressPaths[args.Id];
                        line.path.LineTo(SkiaHelper.ConvertToPixel(this, args.Location));
                        InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        completedPaths.Add(inProgressPaths[args.Id]);
                        inProgressPaths.Remove(args.Id);
                        InvalidateSurface();
                    }
                    break;

                case TouchActionType.Cancelled:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        inProgressPaths.Remove(args.Id);
                        InvalidateSurface();
                    }
                    break;
            }
        }



    }
}
