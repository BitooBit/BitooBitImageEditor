using SkiaSharp;
using System;

namespace BitooBitImageEditor.Text
{
    internal class TextRectangle
    {
        //float MINIMUM = 500;   // pixels width or height

        internal SKRect maxRect;             // generally the size of the bitmap


        internal float height = 20f;
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

        internal SKPoint Corner => new SKPoint(Rect.Right, Rect.Bottom);

        internal SKPoint[] Corners
        {
            get
            {
                return new SKPoint[]
                {
                    new SKPoint(Rect.Left, Rect.Top),
                    new SKPoint(Rect.Right, Rect.Top),
                    new SKPoint(Rect.Right, Rect.Bottom),
                    new SKPoint(Rect.Left, Rect.Bottom)
                };
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
            float X = pixelLocation.X, Y = pixelLocation.Y;
            if (corners[0].X <= X && corners[2].X >= X && corners[0].Y <= Y && corners[2].Y >= Y)
                return true;

            return false;
        }

        internal void MoveAllCorner(SKPoint point)
        {
            SKRect rect = Rect;
            SKRect rectNew = Rect;

            rectNew.Bottom = rectNew.Top + height;
            rectNew.Top += point.Y;
            rectNew.Left += point.X;
            rectNew.Right += point.X;

            //if (!(maxRect.Left > rectNew.Left || maxRect.Right < rectNew.Right))
            //{
                rect.Left = rectNew.Left;
                rect.Right = rectNew.Right;
            //}
            //if (!(maxRect.Bottom < rectNew.Bottom || maxRect.Top > rectNew.Top))
            //{
                rect.Bottom = rectNew.Top + height;
                rect.Top = rectNew.Top;
            //}
            //else
            //{

            //}

            Rect = rect;
        }

        internal void MoveCorner(SKPoint point)
        {
            float MINIMUM = Math.Min(maxRect.Width, maxRect.Height) * 0.15f;
            SKRect rect = Rect;
            rect.Right = Math.Max(point.X, rect.Left + MINIMUM);
            //rect.Right = Math.Min(point.X, maxRect.Right);

            Rect = rect;
        }
    }
}
