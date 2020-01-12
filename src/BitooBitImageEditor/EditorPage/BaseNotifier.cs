using System.ComponentModel;

namespace BitooBitImageEditor.EditorPage
{
    public class BaseNotifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;   
    }
}
