using System;
using BitooBitImageEditor.Helper;
using BitooBitImageEditor.IOS;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(IOSHapticFeedback))]
namespace BitooBitImageEditor.IOS
{
    class IOSHapticFeedback : IHapticFeedback
    {
        public void Excute()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                var impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
                impact.Prepare();
                impact.ImpactOccurred();
                impact.Dispose();
            }
            else
                Vibration.Vibrate();
        }
    }
}