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
            var collect = new ObservableCollection<CropItem> { new CropItem("rotate_right", CropRotateType.CropRotate) };

            if (IsAddAllElements)
            {
                collect.Add(new CropItem("crop_full", CropRotateType.CropFull));
                collect.Add(new CropItem("crop_free", CropRotateType.CropFree));
                collect.Add(new CropItem("crop_square", CropRotateType.CropSquare));
                collect.Add(new CropItem("2_3", CropRotateType.Crop2_3));
                collect.Add(new CropItem("3_2", CropRotateType.Crop3_2));
                collect.Add(new CropItem("3_4", CropRotateType.Crop3_4));
                collect.Add(new CropItem("4_3", CropRotateType.Crop4_3));
                collect.Add(new CropItem("9_16", CropRotateType.Crop9_16));
                collect.Add(new CropItem("16_9", CropRotateType.Crop16_9));

            }

            return collect;
        }

    }
}
