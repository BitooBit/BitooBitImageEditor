using BitooBitImageEditor.UWP;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformHelper))]

namespace BitooBitImageEditor.UWP
{
    internal class PlatformHelper : IPlatformHelper
    {
        public bool IsInitialized => Platform.IsInitialized;
    }
}