using System.Windows;
using System.Windows.Media;

namespace PharmaDesk.Views;

public partial class ProductManagementView : System.Windows.Controls.Page
{
    public ProductManagementView() => InitializeComponent();

    private void ImageDropZone_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            DropZoneRect.Stroke = new SolidColorBrush(Color.FromRgb(0xFC, 0xA3, 0x11));
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void ImageDropZone_DragLeave(object sender, DragEventArgs e)
    {
        DropZoneRect.ClearValue(System.Windows.Shapes.Rectangle.StrokeProperty);
    }

    private void ImageDropZone_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && DataContext is ViewModels.ProductManagementViewModel vm)
            {
                vm.SetImagePath(files[0]);
            }
        }
        DropZoneRect.ClearValue(System.Windows.Shapes.Rectangle.StrokeProperty);
    }
}
