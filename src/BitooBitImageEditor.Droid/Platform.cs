using Android.App;
using Android.Content;
using Android.OS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace BitooBitImageEditor.Droid
{
    /// <summary> Required to use <see cref="BitooBitImageEditor"/> on Android</summary>
    public static class Platform
    {
        internal static bool IsInitialized { get; set; }

        internal static FormsAppCompatActivity CurrentActivity { get; private set; }
        internal static Bundle CurrentBundle { get; private set; }
        internal const int PickImageId = 1000;
        /// <summary>
        /// initializes <see cref="BitooBitImageEditor"/>
        /// </summary>
        /// <param name="activity">current <see cref="Activity"/> </param>
        /// <param name="bundle">current <see cref="Bundle"/> </param>
        public static void Init(FormsAppCompatActivity activity, Bundle bundle)
        {
            CurrentActivity = activity;
            CurrentBundle = bundle;
            NativeMedia.Platform.Init(activity, bundle);
            IsInitialized = true;
            LinkAssemblies();
        }

        /// <summary>required to get an image from the gallery</summary>
        public static void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            if (NativeMedia.Platform.CheckCanProcessResult(requestCode, resultCode, intent))
                NativeMedia.Platform.OnActivityResult(requestCode, resultCode, intent);
        }

        public static void OnBackPressed()
        {
            if(ImageEditor.IsOpened)
                MessagingCenter.Send(Xamarin.Forms.Application.Current, "BBDroidBackButton");
        }

        private static void LinkAssemblies()
        {

        }
    }
}
