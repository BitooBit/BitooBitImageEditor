using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BitooBitImageEditor.UWP
{
    /// <summary>
    /// Необходим для исспользования <see cref="BitooBitImageEditor"/> на UWP
    /// </summary>
    public static class Platform
    {
        internal static Windows.UI.Xaml.Application application;
        internal static Windows.UI.Xaml.Application Application
        {
            get => application ?? Windows.UI.Xaml.Application.Current;
            set => application = value;
        }
        internal static bool IsInitialized { get; set; }

        /// <summary>
        /// Инициализирует <see cref="BitooBitImageEditor"/>
        /// </summary>
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
