using BitooBitImageEditor.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformHelper))]

namespace BitooBitImageEditor.Droid
{
    internal class PlatformHelper : IPlatformHelper
    {
        public bool IsInitialized => Platform.IsInitialized;
    }
}