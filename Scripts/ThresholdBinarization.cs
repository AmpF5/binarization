using System.Runtime.InteropServices;
using System.Windows.Media;
using binarization.Helpers;

namespace binarization.Scripts;

public static class ThresholdBinarization {
    public static ImageSource Binarize(ImageSource image, byte threshold = 128) {
        var writeableImage = image.ConvertToWriteableBitmap();
        
        writeableImage.Lock();

        var width = writeableImage.PixelWidth;
        var height = writeableImage.PixelHeight;
        var stride = writeableImage.BackBufferStride;
        var pBackBuffer = writeableImage.BackBuffer;
        
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var pPixel = pBackBuffer + y * stride + x * 4;

                var colorData = Marshal.ReadInt32(pPixel);
                var grayscale = ColorHelper.CalculateGrayscale(colorData);
                
                var binaryColor = grayscale < threshold ? (byte)0 : (byte)255;
                var binarizedColorData = (255 << 24) | (binaryColor << 16) | (binaryColor << 8) | binaryColor;
                
                Marshal.WriteInt32(pPixel, binarizedColorData);
            }
        }

        writeableImage.Unlock();
        return writeableImage;
    }
}