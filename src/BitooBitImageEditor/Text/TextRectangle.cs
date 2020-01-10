using SkiaSharp;
using System;

namespace BitooBitImageEditor.Text
{
    internal class TextRectangle
    {
        //float MINIMUM = 500;   // pixels width or height

        internal SKRect maxRect;             // generally the size of the bitmap
        internal double angel;


        internal TextRectangle(SKRect maxRect)
        {
            SetRect(maxRect);
        }


        internal void SetRect(SKRect maxRect)
        {
            this.maxRect = maxRect;           

            Rect = new SKRect(0.9f * maxRect.Left + 0.1f * maxRect.Right,
                            0.9f * maxRect.Top + 0.1f * maxRect.Bottom,
                            0.1f * maxRect.Left + 0.9f * maxRect.Right,
                            0.1f * maxRect.Top + 0.9f * maxRect.Bottom);
        }



        internal SKRect Rect { set; get; }

        internal SKPoint Corner => SkiaHelper.RotatePoint(new SKPoint(Rect.MidX, Rect.MidY), angel, new SKPoint(Rect.Right, Rect.Bottom)); 

        internal SKPoint[] Corners
        {
            get
            {
                return SkiaHelper.RotatePoint(new SKPoint(Rect.MidX, Rect.MidY), angel, 
                    new SKPoint(Rect.Left, Rect.Top), new SKPoint(Rect.Right, Rect.Top), new SKPoint(Rect.Right, Rect.Bottom), new SKPoint(Rect.Left, Rect.Bottom));
            }
        }

        internal bool HitTest(SKPoint point, float radius)
        {
            SKPoint diff = point - Corner;
            if ((float)Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y) < radius)
                return true;
            return false;
        }

        internal bool TestPointInsideSquare(SKPoint pixelLocation)
        {
            SKPoint[] corners = Corners;

            return SkiaHelper.CheckPointInsideTriangle(pixelLocation, corners[0], corners[1], corners[2]) || SkiaHelper.CheckPointInsideTriangle(pixelLocation, corners[2], corners[3], corners[0]);
        }

        internal void MoveAllCorner(SKPoint point)
        {
            SKRect rect = Rect;
            SKRect rectNew = Rect;

            rectNew.Bottom += point.Y;
            rectNew.Top += point.Y;
            rectNew.Left += point.X;
            rectNew.Right += point.X;

            rect.Left = rectNew.Left;
            rect.Right = rectNew.Right;
            rect.Bottom += point.Y;
            rect.Top = rectNew.Top;
            Rect = rect;
        }

        internal void MoveCorner(SKPoint point)
        {
            float MINIMUM = Math.Min(maxRect.Width, maxRect.Height) * 0.15f;
            SKRect rect = Rect;


            float absX = Math.Abs(point.X - rect.MidX);
            float absY = Math.Abs(point.Y - rect.MidY);

            rect.Right = rect.MidX + absX;

            rect.Left = rect.MidX - absX;

            rect.Bottom = rect.MidY + absY;
            rect.Top = rect.MidY - absY;


            double a = CalcLenght(point.X, point.Y, rect.Right, rect.Bottom);
            double b = CalcLenght(rect.MidX, rect.Bottom, point.X, point.Y);
            double c = CalcLenght(rect.MidX, rect.Bottom, rect.Right, rect.Bottom);

            double _angel = Math.Acos((b * b + c * c - a * a) / (2 * b * c)) * 180 / Math.PI;
            angel = rect.Bottom < point.Y ? _angel : - _angel;


            




            

            Rect = rect;
        }


        private double CalcLenght(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }


    } 
}
