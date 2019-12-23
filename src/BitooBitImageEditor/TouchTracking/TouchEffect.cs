using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BitooBitImageEditor.TouchTracking
{
    public class TouchEffect : RoutingEffect
    {
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
    }
}
