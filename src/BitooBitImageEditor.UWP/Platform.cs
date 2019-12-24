using BitooBitImageEditor.TouchTracking;
using System.Collections.Generic;
using System.Reflection;

namespace BitooBitImageEditor.UWP
{
    /// <summary> Необходим для исспользования <see cref="BitooBitImageEditor"/> на UWP </summary>
    public static class Platform
    {
        internal static Windows.UI.Xaml.Application application;
        internal static Windows.UI.Xaml.Application Application
        {
            get => application ?? Windows.UI.Xaml.Application.Current;
            set => application = value;
        }
        internal static bool IsInitialized { get; set; }


        public static IEnumerable<Assembly> GetExtraAssemblies(IEnumerable<Assembly> defaultAssemblies = null)
        {
            var assemblies = new List<Assembly>
            {
                GetAssembly<ImageEditor>(),
                GetAssembly<TouchEffect>()
            };

            if (defaultAssemblies != null)
                assemblies.AddRange(defaultAssemblies);

            return assemblies;
        }

        private static Assembly GetAssembly<T>()
        {
            return typeof(T).GetTypeInfo().Assembly;
        }


        /// <summary> Инициализирует <see cref="BitooBitImageEditor"/> </summary>
        /// <param name="applcation">текущее UWP приложение</param>
        public static void Init(Windows.UI.Xaml.Application applcation)
        {
            if (application != null)
                return;
            Application = applcation;
            IsInitialized = true;
        }

    }
}
