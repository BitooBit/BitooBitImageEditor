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


        internal static ObservableCollection<CropItem> GetCropItems(bool IsAddAllElements)
        {
            var collect = new ObservableCollection<CropItem>
            {
                 new CropItem("rotate_right", CropRotateType.CropRotate)
                ,new CropItem("crop_full", CropRotateType.CropFull)
            };

            if (IsAddAllElements)
            {
                collect.Add(new CropItem("crop_free", CropRotateType.CropFree));
                collect.Add(new CropItem("crop_square", CropRotateType.CropSquare));
            }

            return collect;
        }

    }
}
