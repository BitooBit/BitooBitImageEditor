namespace BitooBitImageEditor.EditorPage
{
    internal class MenuItem : BaseNotifier
    {
        public MenuItem(string imageName, ImageEditType type, ActionEditType action)
        {
            ImageName = imageName;
            Type = type;
            Action = action;
        }

        public string ImageName { get; set; }
        public ImageEditType Type { get; set; }
        public ActionEditType Action { get; set; }

    }
}
