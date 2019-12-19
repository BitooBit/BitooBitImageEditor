using BitooBitImageEditor.IOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformHelper))]

namespace BitooBitImageEditor.IOS
{
    class PlatformHelper : IPlatformHelper
    {
        public bool IsInitialized => Platform.IsInitialized;
    }
}