using BitooBitImageEditor.UWP;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformHelper))]

namespace BitooBitImageEditor.UWP
{
    class PlatformHelper : IPlatformHelper
    {
        public bool IsInitialized => Platform.IsInitialized;
    }
}