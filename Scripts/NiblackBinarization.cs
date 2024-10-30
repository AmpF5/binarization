using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Media;
using binarization.Helpers;

namespace binarization.Scripts;

public static class NiblackBinarization {
    public static ImageSource Binarize(ImageSource image, int windowSize = 5, double k = 0.5, int r = 128) {
        var stopwatch = Stopwatch.StartNew();
        
        var writeableImage = image.ConvertToWriteableBitmap();
        
        writeableImage.Lock();
        
        var width = writeableImage.PixelWidth;
        var height = writeableImage.PixelHeight;
        var stride = writeableImage.BackBufferStride;
        var pBackBuffer = writeableImage.BackBuffer;
        
        var radius = (windowSize - 1) / 2;
        var blockSize = 2 * radius + 1;
        var block = new byte[blockSize * blockSize];

        for (var y = radius; y < height - radius; y++) {
            for (var x = radius; x < width - radius; x++) {
                
                Array.Clear(block, 0, block.Length);
                
                for (var i = -radius; i <= radius; i++) {
                    for (var j = -radius; j <= radius; j++) {
                        var pNeighbourPixel = pBackBuffer + (i + radius) * stride + (j + radius) * 4;
                        var colorNeighbourData = Marshal.ReadInt32(pNeighbourPixel);
                        
                        var index = (i + radius) * blockSize + (j + radius);
                        block[index] = ColorHelper.CalculateGrayscale(colorNeighbourData);
                    }
                }
                
                var mean = MathHelper.CalculateMean(block);
                var standardDeviation = MathHelper.CalculateStandardDeviation(block, mean);
                var threshold = mean + (k * standardDeviation);
                
                var pPixel = pBackBuffer + y * stride + x * 4;
                var colorData = Marshal.ReadInt32(pPixel);
                var grayscale = ColorHelper.CalculateGrayscale(colorData);
                
                var binaryColor = grayscale < threshold ? (byte)0 : (byte)255;
                var binarizedColorData = (255 << 24) | (binaryColor << 16) | (binaryColor << 8) | binaryColor;
                
                Marshal.WriteInt32(pPixel, binarizedColorData);
            }
        }
        
        writeableImage.Unlock();
        
        var timeTaken = stopwatch.Elapsed;
        Console.WriteLine("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
        
        return writeableImage;
    }
    
}