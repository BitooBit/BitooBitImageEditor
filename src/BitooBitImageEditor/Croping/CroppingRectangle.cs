using SkiaSharp;
using System;

namespace BitooBitImageEditor.Croping
{
    internal class CroppingRectangle
    {
        private float MINIMUM = 500;   // pixels width or height
        private SKRect maxRect;             // generally the size of the bitmap
        private float? aspectRatio;

        internal CroppingRectangle(SKRect maxRect, float? aspectRatio = null)
        {
            SetRect(maxRect, aspectRatio);
        }

        internal SKRect Rect { set; get; }

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

        internal int HitTest(SKPoint point, float radius)
        {
            SKPoint[] corners = Corners;

            for (int index = 0; index < corners.Length; index++)
            {
                SKPoint diff = point - corners[index];

                if ((float)Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y) < radius)
                {
                    return index;
                }
            }

            return -1;
        }



        internal bool TestPointInsideSquare(SKPoint pixelLocation)
        {
            bool rezult = false;
            SKPoint[] corners = Corners;
            float X = pixelLocation.X, Y = pixelLocation.Y;
            if (Corners[0].X < X && corners[2].X > X && corners[0].Y < Y && Corners[2].Y > Y)
                rezult = true;
            return rezult;
        }

        internal void MoveAllCorner(SKPoint point)
        {
            SKRect rect = Rect;
            SKRect rectNew = Rect;
            rectNew.Bottom += point.Y;
            rectNew.Top += point.Y;
            rectNew.Left += point.X;
            rectNew.Right += point.X;

            if (!(maxRect.Left > rectNew.Left || maxRect.Right < rectNew.Right))
            {
                rect.Left = rectNew.Left;
                rect.Right = rectNew.Right;
            }
            if (!(maxRect.Bottom < rectNew.Bottom || maxRect.Top > rectNew.Top))
            {
                rect.Bottom = rectNew.Bottom;
                rect.Top = rectNew.Top;
            }

            Rect = rect;
        }

        internal void MoveCorner(int index, SKPoint point)
        {
            SKRect rect = Rect;

            switch (index)
            {
                case 0: // upper-left
                    rect.Left = Math.Min(Math.Max(point.X, maxRect.Left), rect.Right - MINIMUM);
                    rect.Top = Math.Min(Math.Max(point.Y, maxRect.Top), rect.Bottom - MINIMUM);
                    break;

                case 1: // upper-right
                    rect.Right = Math.Max(Math.Min(point.X, maxRect.Right), rect.Left + MINIMUM);
                    rect.Top = Math.Min(Math.Max(point.Y, maxRect.Top), rect.Bottom - MINIMUM);
                    break;

                case 2: // lower-right
                    rect.Right = Math.Max(Math.Min(point.X, maxRect.Right), rect.Left + MINIMUM);
                    rect.Bottom = Math.Max(Math.Min(point.Y, maxRect.Bottom), rect.Top + MINIMUM);
                    break;

                case 3: // lower-left
                    rect.Left = Math.Min(Math.Max(point.X, maxRect.Left), rect.Right - MINIMUM);
                    rect.Bottom = Math.Max(Math.Min(point.Y, maxRect.Bottom), rect.Top + MINIMUM);
                    break;
            }

            // Adjust for aspect ratio
            if (aspectRatio.HasValue)
            {
                float aspect = aspectRatio.Value;

                if (rect.Width > aspect * rect.Height)
                {
                    float width = aspect * rect.Height;

                    switch (index)
                    {
                        case 0:
                        case 3: rect.Left = rect.Right - width; break;
                        case 1:
                        case 2: rect.Right = rect.Left + width; break;
                    }
                }
                else
                {
                    float height = rect.Width / aspect;

                    switch (index)
                    {
                        case 0:
                        case 1: rect.Top = rect.Bottom - height; break;
                        case 2:
                        case 3: rect.Bottom = rect.Top + height; break;
                    }
                }
            }

            Rect = rect;
        }

        internal void SetRect(SKRect maxRect, float? aspectRatio = null, bool isFullRect = false)
        {
            this.maxRect = maxRect;
            this.aspectRatio = aspectRatio;

            MINIMUM = Math.Min(maxRect.Width, maxRect.Height) * 0.22f;

            // Set initial cropping rectangle
            if (isFullRect)
                Rect = new SKRect(1f * maxRect.Left + 0f * maxRect.Right,
                                1f * maxRect.Top + 0f * maxRect.Bottom,
                                0f * maxRect.Left + 1f * maxRect.Right,
                                0f * maxRect.Top + 1f * maxRect.Bottom);
            else
                Rect = new SKRect(0.9f * maxRect.Left + 0.1f * maxRect.Right,
                                0.9f * maxRect.Top + 0.1f * maxRect.Bottom,
                                0.1f * maxRect.Left + 0.9f * maxRect.Right,
                                0.1f * maxRect.Top + 0.9f * maxRect.Bottom);


            // Adjust for aspect ratio
            if (aspectRatio.HasValue)
            {
                SKRect rect = Rect;
                float aspect = aspectRatio.Value;

                if (rect.Width > aspect * rect.Height)
                {
                    float width = aspect * rect.Height;
                    rect.Left = (maxRect.Width - width) / 2;
                    rect.Right = rect.Left + width;
                }
                else
                {
                    float height = rect.Width / aspect;
                    rect.Top = (maxRect.Height - height) / 2;
                    rect.Bottom = rect.Top + height;
                }

                Rect = rect;
            }
        }


    }
}
