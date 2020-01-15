
using SkiaSharp;

namespace BitooBitImageEditor.ManipulationBitmap
{
    internal class TouchManipulationInfo
    {
        public SKPoint PreviousPoint { set; get; }

        public SKPoint NewPoint { set; get; }
    }
}
