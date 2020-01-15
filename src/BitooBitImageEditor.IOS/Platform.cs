namespace BitooBitImageEditor.IOS
{
    /// <summary>
    /// Необходим для исспользования <see cref="BitooBitImageEditor"/> на IOS
    /// </summary>
    public static class Platform
    {
        internal static bool IsInitialized { get; set; }

        /// <summary>
        /// Инициализирует <see cref="BitooBitImageEditor"/>
        /// </summary>
        public static void Init()
        {
            IsInitialized = true;
            LinkAssemblies();
        }

        private static void LinkAssemblies()
        {

        }
    }
}
