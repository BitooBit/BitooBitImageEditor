using BitooBitImageEditor.IOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformHelper))]

namespace BitooBitImageEditor.IOS
{
    internal class PlatformHelper : IPlatformHelper
    {
        public bool IsInitialized => Platform.IsInitialized;
    }
}