using BitooBitImageEditor.Controls;
using BitooBitImageEditor.UWP.Renders;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CustomEditor), typeof(CustomEditorRender))]
namespace BitooBitImageEditor.UWP.Renders
{
    internal class CustomEditorRender : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.TextAlignment = Windows.UI.Xaml.TextAlignment.Center;
            }
        }
    }
}
