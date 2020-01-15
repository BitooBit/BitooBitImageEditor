using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BitooBitImageEditor.ManipulationBitmap
{
    internal enum BitmapType
    {
        Main,
        Stickers,
        Text
    }

    internal class TouchManipulationBitmap
    {
        private readonly Dictionary<long, TouchManipulationInfo> touchDictionary =
            new Dictionary<long, TouchManipulationInfo>();

        public TouchManipulationBitmap(SKBitmap bitmap, SKMatrix matrix, BitmapType type, string text, SKColor color = default)
        {
            Bitmap = bitmap;
            Matrix = matrix;
            Type = type;
            Text = text;
            Color = color;
        }

        public TouchManipulationBitmap(SKBitmap bitmap, BitmapType type, string text, SKColor color = default)
        {
            Matrix = SKMatrix.MakeIdentity();
            Bitmap = bitmap;
            Type = type;
            Color = color;
            Text = text;
        }

        public TouchManipulationManager TouchManager { set; get; } = new TouchManipulationManager
        {
            Mode = TouchManipulationMode.ScaleRotate
        };


        public TouchActionType? TouchAction { set; get; } = null;
        public SKBitmap Bitmap { set; get; }
        public SKMatrix Matrix { set; get; }
        public string Text { set; get; }
        public SKColor Color { set; get; }
        public BitmapType Type { set; get; }
        public bool IsHide { set; get; } = false;



        public int HitTest(SKPoint location, SKRect info)
        {
            if (Type != BitmapType.Main && Matrix.TryInvert(out SKMatrix inverseMatrix))
            {
                SKRect rect = new SKRect(0, 0, Bitmap.Width, Bitmap.Height);
                SKPoint transformedPoint = inverseMatrix.MapPoint(location);

                if (Device.RuntimePlatform == Device.UWP)
                {
                    float max = info.Width > info.Height ? info.Width : info.Height;
                    float radius = max * 0.025f;

                    var corners = new SKPoint[]
                    {
                        new SKPoint(rect.Left, rect.Top),
                        new SKPoint(rect.Right, rect.Top),
                        new SKPoint(rect.Right, rect.Bottom),
                        new SKPoint(rect.Left, rect.Bottom)
                    };

                    for (int index = 0; index < corners.Length; index++)
                    {
                        SKPoint diff = transformedPoint - corners[index];
                        float delta = (float)Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);

                        if (delta < radius)
                            return index;
                    }
                }

                return rect.Contains(transformedPoint) ? 4 : -1;
            }
            return -1;
        }


        public void ProcessTouchEvent(long id, TouchActionType type, SKPoint location)
        {
            switch (type)
            {
                case TouchActionType.Pressed:
                    touchDictionary.Add(id, new TouchManipulationInfo
                    {
                        PreviousPoint = location,
                        NewPoint = location
                    });
                    break;

                case TouchActionType.Moved:
                    TouchManipulationInfo info = touchDictionary[id];
                    info.NewPoint = location;
                    Manipulate();
                    info.PreviousPoint = info.NewPoint;
                    break;

                case TouchActionType.Released:
                    touchDictionary[id].NewPoint = location;
                    Manipulate();
                    touchDictionary.Remove(id);
                    break;

                case TouchActionType.Cancelled:
                    touchDictionary.Remove(id);
                    break;
            }
        }

        private void Manipulate()
        {
            TouchManipulationInfo[] infos = new TouchManipulationInfo[touchDictionary.Count];
            touchDictionary.Values.CopyTo(infos, 0);
            SKMatrix touchMatrix = SKMatrix.MakeIdentity();

            if (infos.Length == 1)
            {
                SKPoint prevPoint = infos[0].PreviousPoint;
                SKPoint newPoint = infos[0].NewPoint;
                SKPoint pivotPoint = Matrix.MapPoint(Bitmap.Width / 2, Bitmap.Height / 2);

                touchMatrix = TouchManager.OneFingerManipulate(prevPoint, newPoint, pivotPoint);
            }
            else if (infos.Length >= 2)
            {
                int pivotIndex = infos[0].NewPoint == infos[0].PreviousPoint ? 0 : 1;
                SKPoint pivotPoint = infos[pivotIndex].NewPoint;
                SKPoint newPoint = infos[1 - pivotIndex].NewPoint;
                SKPoint prevPoint = infos[1 - pivotIndex].PreviousPoint;

                touchMatrix = TouchManager.TwoFingerManipulate(prevPoint, newPoint, pivotPoint);
            }

            SKMatrix matrix = Matrix;
            SKMatrix.PostConcat(ref matrix, touchMatrix);
            Matrix = matrix;
        }
    }
}
