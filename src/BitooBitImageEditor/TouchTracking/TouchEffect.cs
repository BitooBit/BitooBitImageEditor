using Xamarin.Forms;

namespace BitooBitImageEditor.TouchTracking
{
    /// <summary>for internal use by <see cref="BitooBitImageEditor"/></summary>
    public class TouchEffect : RoutingEffect
    {
#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена

        public event TouchActionEventHandler TouchAction;

        public const string resolutionGroupName = "BitooBitDocs";
        public const string uniqueName = "BBTouchEffect";

        public TouchEffect() : base($"{resolutionGroupName}.{uniqueName}")
        {
        }

        public bool Capture { set; get; }

        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            TouchAction?.Invoke(element, args);
        }
#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
    }
}
