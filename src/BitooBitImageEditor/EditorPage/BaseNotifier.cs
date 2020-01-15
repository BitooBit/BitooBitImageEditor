using System.ComponentModel;

namespace BitooBitImageEditor.EditorPage
{
    /// <summary>for internal use by <see cref="BitooBitImageEditor"/></summary>
    public class BaseNotifier : INotifyPropertyChanged
    {
        /// <summary>for internal use by <see cref="BitooBitImageEditor"/></summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
