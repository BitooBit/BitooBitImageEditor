using System;

using SkiaSharp;

namespace BitooBitImageEditor.ManipulationBitmap
{
    class TouchManipulationInfo
    {
        public SKPoint PreviousPoint { set; get; }

        public SKPoint NewPoint { set; get; }
    }
}
