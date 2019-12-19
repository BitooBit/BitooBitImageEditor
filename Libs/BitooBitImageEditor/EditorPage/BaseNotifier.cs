using System.ComponentModel;

namespace BitooBitImageEditor.EditorPage
{
    internal class BaseNotifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;   
    }
}
