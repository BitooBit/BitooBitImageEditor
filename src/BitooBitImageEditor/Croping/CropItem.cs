using BitooBitImageEditor.Croping;
using BitooBitImageEditor.EditorPage;
using System.Collections.ObjectModel;

namespace BitooBitImageEditor.Croping
{
    internal class CropItem : BaseNotifier
    {
        public CropItem(string imageName, CropRotateType action)
        {
            ImageName = imageName;
            Action = action;
        }

        public string ImageName { get; set; }
        public CropRotateType Action { get; set; }


        static internal ObservableCollection<CropItem> GetCropItems()
        {
            return new ObservableCollection<CropItem>
            {
                 new CropItem("rotate_right", CropRotateType.CropRotate)
                ,new CropItem("crop_full", CropRotateType.CropFull)
                ,new CropItem("crop_free", CropRotateType.CropFree)
                ,new CropItem("crop_square", CropRotateType.CropSquare)
            };
        }

    }
}
