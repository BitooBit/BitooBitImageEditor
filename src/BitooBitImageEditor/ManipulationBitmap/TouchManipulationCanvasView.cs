using BitooBitImageEditor.Helper;
using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xamarin.Forms;

namespace BitooBitImageEditor.ManipulationBitmap
{
    internal class TouchManipulationCanvasView : SKCanvasView
    {
        internal List<TouchManipulationBitmap> bitmapCollection =
            new List<TouchManipulationBitmap>();

        internal Dictionary<long, TouchManipulationBitmap> bitmapDictionary =
            new Dictionary<long, TouchManipulationBitmap>();

        public TouchManipulationCanvasView()
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            string[] resourceIDs = assembly.GetManifestResourceNames();
            SKPoint position = new SKPoint();

            foreach (string resourceID in resourceIDs)
            {
                if (resourceID.EndsWith("Banana.jpg") || resourceID.EndsWith("BananaMatte.png"))
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourceID))
                    {
                        SKBitmap bitmap = SKBitmap.Decode(stream);
                        bitmapCollection.Add(new TouchManipulationBitmap(bitmap)
                        {
                            Matrix = SKMatrix.MakeTranslation(position.X, position.Y),
                        });
                        position.X += 100;
                        position.Y += 100;
                    }
                }
            }
            //InvalidateSurface();

        }


        internal void AddTextToCanvas(string text, SKColor color)
        {
            var bitmapText= SKBitmapBuilder.FromText(text, color);
            if(bitmapText != null)
                bitmapCollection.Add(new TouchManipulationBitmap(bitmapText)
                {
                    Matrix = SKMatrix.MakeTranslation(0, 0),
                });

            InvalidateSurface();
        }






        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            base.OnPaintSurface(args);
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            foreach (TouchManipulationBitmap bitmap in bitmapCollection)
            {
                bitmap.Paint(canvas);
            }

        }

        internal void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
            Point pt = args.Location;
            SKPoint point =
                new SKPoint((float)(CanvasSize.Width * pt.X / Width),
                            (float)(CanvasSize.Height * pt.Y / Height));

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if(!bitmapDictionary.ContainsKey(args.Id))
                    for (int i = bitmapCollection.Count - 1; i >= 0; i--)
                    {
                        TouchManipulationBitmap bitmap = bitmapCollection[i];

                        if (bitmap.HitTest(point))
                        {
                            // Move bitmap to end of collection
                            bitmapCollection.Remove(bitmap);
                            bitmapCollection.Add(bitmap);

                            // Do the touch processing
                            bitmapDictionary.Add(args.Id, bitmap);
                            bitmap.ProcessTouchEvent(args.Id, args.Type, point);
                            InvalidateSurface();
                            break;
                        }
                    }

                    break;

                case TouchActionType.Moved:
                    if (bitmapDictionary.ContainsKey(args.Id))
                    {
                        TouchManipulationBitmap bitmap = bitmapDictionary[args.Id];
                        bitmap.ProcessTouchEvent(args.Id, args.Type, point);
                        InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    if (bitmapDictionary.ContainsKey(args.Id))
                    {
                        TouchManipulationBitmap bitmap = bitmapDictionary[args.Id];
                        bitmap.ProcessTouchEvent(args.Id, args.Type, point);
                        bitmapDictionary.Remove(args.Id);
                        InvalidateSurface();
                    }
                    break;
            }

        }
    }
}
