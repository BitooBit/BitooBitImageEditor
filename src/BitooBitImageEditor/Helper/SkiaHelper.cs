using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Xamarin.Forms;

namespace BitooBitImageEditor.Helper
{
    internal static class SkiaHelper
    {
        internal const uint backgroundColor = 0xC9020204;
        internal const double trashSize = 50;
        internal const double trashBigSize = 75;
        internal static Thickness trashMargin = new Thickness(0,0,0,30);
        internal static Color backgroundColorHex = Color.FromUint(backgroundColor);

        internal static (SKRect rect, float scaleX, float scaleY) CalculateRectangle(SKRect info, SKBitmap bitmap, BBAspect aspect = BBAspect.AspectFit) => CalculateRectangle(info, bitmap.Width, bitmap.Height, aspect);

        internal static (SKRect rect, float scaleX, float scaleY) CalculateRectangle(SKRect info, float width, float height, BBAspect aspect = BBAspect.AspectFit)
        {
            BBAspect _aspect;
            if(aspect == BBAspect.Auto)
            {
                float aspectInfo = Math.Abs(info.Width / info.Height);
                float aspectBitmap = Math.Abs(width / height);
                var res = Math.Abs(aspectInfo - aspectBitmap);
                if (res < 0.27)
                    _aspect = BBAspect.AspectFill;
                else
                    _aspect = BBAspect.AspectFit;
            }
            else
                _aspect = aspect;

            float scaleX = info.Width / width;
            float scaleY = info.Height / height;

            if (_aspect != BBAspect.Fill)
            {
                scaleX = scaleY = _aspect == BBAspect.AspectFit ? Math.Min(scaleX, scaleY) : Math.Max(scaleX, scaleY);
                float left = ((info.Width - scaleX * width) / 2) + info.Left;
                float top = ((info.Height - scaleX * height) / 2) + info.Top;
                float right = left + scaleX * width;
                float bottom = top + scaleX * height;
                return (new SKRect(left, top, right, bottom), scaleX, scaleY);

            }
            else
                return (info, scaleX, scaleY);
        }


        internal static byte[] SKBitmapToBytes(SKBitmap bitmap)
        {
            byte[] imageData;

            using (SKData data = SKImage.FromBitmap(bitmap).Encode())
            using (Stream stream = data.AsStream())
            {
                imageData = new byte[stream.Length];
                stream.Read(imageData, 0, System.Convert.ToInt32(stream.Length));
            }
            GC.Collect();
            
            return imageData;
        }


        internal static ObservableCollection<Color> GetColors()
        {
            ObservableCollection<Color> colors = new ObservableCollection<Color>
            {
                 Color.White
                ,Color.Gray
                ,Color.Black
                ,Color.Red
                ,Color.Orange
                ,Color.Yellow
                ,Color.Green
                ,Color.Cyan
                ,Color.Blue
                ,Color.Violet
            };

            int count = 35;
            double offset = 16777215 / (double)count;
            for (int i = 1; i < count - 1; i++)
            {
                var color = Color.FromHex(((int)(i * offset)).ToString("X"));
                if(color.A != -1 && color.R != -1 && color.G != -1 && color.B != -1)
                    colors.Add(color);
            }

            return colors;
        }

    }
}




























//internal static float CalcLenght(SKPoint point1, SKPoint point2)
//{
//    return (float)Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
//}

//internal static SKPoint[] RotatePoint(SKPoint center, double degrees, params SKPoint[] points)
//{
//    double angel = (degrees / 180D) * Math.PI;
//    double cos = Math.Cos(angel);
//    double sin = Math.Sin(angel);
//    SKPoint[] rotatePoints = new SKPoint[points.Length];

//    for (int i = 0; i < points.Length; i++)
//        rotatePoints[i] = RotatePoint(center, cos, sin, points[i]);

//    return rotatePoints;
//}

//internal static SKPoint RotatePoint(SKPoint center, double degrees, SKPoint point)
//{
//    double angel = (degrees / 180D) * Math.PI;
//    return RotatePoint(center, Math.Cos(angel), Math.Sin(angel), point);
//}

//internal static SKPoint RotatePoint(SKPoint center, double cos, double sin, SKPoint point)
//{
//    double x = (center.X + (point.X - center.X) * cos - (point.Y - center.Y) * sin);
//    double y = (center.Y + (point.X - center.X) * sin + (point.Y - center.Y) * cos);
//    return new SKPoint((float)x, (float)y);
//}

//internal static bool CheckPointInsideTriangle(SKPoint point, SKPoint triangle1, SKPoint triangle2, SKPoint triangle3)
//{
//    float a1 = (triangle1.X - point.X) * (triangle2.Y - triangle1.Y) - (triangle2.X - triangle1.X) * (triangle1.Y - point.Y);
//    float a2 = (triangle2.X - point.X) * (triangle3.Y - triangle2.Y) - (triangle3.X - triangle2.X) * (triangle2.Y - point.Y);
//    float a3 = (triangle3.X - point.X) * (triangle1.Y - triangle3.Y) - (triangle1.X - triangle3.X) * (triangle3.Y - point.Y);

//    if (a1 == 0 || a2 == 0 || a3 == 0)
//        return true;

//    a1 /= Math.Abs(a1);
//    a2 /= Math.Abs(a2);
//    a3 /= Math.Abs(a3);

//    if (a1 == a2 && a2 == a3)
//        return true;

//    else
//        return false;

//}