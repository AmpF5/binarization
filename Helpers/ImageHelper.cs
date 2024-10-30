using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace binarization.Helpers;

public static class ImageHelper {
    public static WriteableBitmap ConvertToWriteableBitmap(this ImageSource bitmapImage) {
        return new WriteableBitmap(bitmapImage as BitmapImage ?? throw new InvalidOperationException("Cannot convert to BitmapImage"));
    }
}