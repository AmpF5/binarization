using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace binarization.Scripts;

public static class ThresholdBinarization {
    public static ImageSource Binarize(ImageSource image, byte threshold = 128) {
        var writeableImage = new WriteableBitmap(
            image as BitmapImage ?? throw new InvalidOperationException("Cannot convert to BitmapImage"));
        
        writeableImage.Lock();

        var width = writeableImage.PixelWidth;
        var height = writeableImage.PixelHeight;
        var stride = writeableImage.BackBufferStride;
        var pBackBuffer = writeableImage.BackBuffer;

        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var pPixel = pBackBuffer + y * stride + x * 4;

                var colorData = Marshal.ReadInt32(pPixel);
                var grayscale = CalculateGrayscale(colorData);
                
                var binaryColor = grayscale < threshold ? (byte)0 : (byte)255;
                var binarizedColorData = (255 << 24) | (binaryColor << 16) | (binaryColor << 8) | binaryColor;
                
                Marshal.WriteInt32(pPixel, binarizedColorData);
            }
        }

        writeableImage.Unlock();
        return writeableImage;
    }

    private static byte CalculateGrayscale(int colorData) {
        var blue = (byte)(colorData & 0xFF);
        var green = (byte)((colorData >> 8) & 0xFF);
        var red = (byte)((colorData >> 16) & 0xFF);
        
        var grayscale = (byte)((red * 0.3) + (green * 0.59) + (blue * 0.11));
        return grayscale;
    }
}