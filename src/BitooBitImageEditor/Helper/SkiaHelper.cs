using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Xamarin.Forms;

namespace BitooBitImageEditor
{
    internal static class SkiaHelper
    {

        internal static (SKRect rect, float scaleX, float scaleY) CalculateRectangle(SKRect info, SKBitmap bitmap, Aspect aspect = Aspect.AspectFit)
        {
            return CalculateRectangle(info, bitmap.Width, bitmap.Height, aspect);
        }

        internal static (SKRect rect, float scaleX, float scaleY) CalculateRectangle(SKRect info, float width, float height, Aspect aspect = Aspect.AspectFit)
        {
            float scaleX = (float)info.Width / width;
            float scaleY = (float)info.Height / height;

            if (aspect != Aspect.Fill)
            {
                scaleX = scaleY = aspect == Aspect.AspectFit ? Math.Min(scaleX, scaleY) : Math.Max(scaleX, scaleY);
                float left = ((info.Width - scaleX * width) / 2) + info.Left;
                float top = ((info.Height - scaleX * height) / 2) + info.Top;
                float right = left + scaleX * width;
                float bottom = top + scaleX * height;
                return (new SKRect(left, top, right, bottom), scaleX, scaleY);

            }
            else
                return (info, scaleX, scaleY);
        }

        

        static internal ObservableCollection<Color> GetColors()
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
                colors.Add(Color.FromHex(((int)((double)i * offset)).ToString("X")));

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