using Android.App;
using Android.Content;
using Android.OS;
using Xamarin.Forms.Platform.Android;

namespace BitooBitImageEditor.Droid
{
    /// <summary>
    /// Необходим для исспользования <see cref="BitooBitImageEditor"/> на Android
    /// </summary>
    public static class Platform
    {
        internal static bool IsInitialized { get; set; }

        internal static FormsAppCompatActivity CurrentActivity { get; private set; }
        internal static Bundle CurrentBundle { get; private set; }
        internal const int PickImageId = 1000;
        /// <summary>
        /// Инициализирует <see cref="BitooBitImageEditor"/>
        /// </summary>
        /// <param name="activity">Текущий <see cref="Activity"/> android приложения</param>
        /// <param name="bundle">Текущий <see cref="Bundle"/> android приложения</param>
        public static void Init(FormsAppCompatActivity activity, Bundle bundle)
        {
            CurrentActivity = activity;
            CurrentBundle = bundle;
            IsInitialized = true;
            LinkAssemblies();
        }


        public static void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {

            if (requestCode == PickImageId)
            {
                ImageHelper.OnActivityResult(resultCode, intent);
            }
        }



        private static void LinkAssemblies()
        {

        }
    }
}
