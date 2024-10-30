
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Media;
using binarization.Helpers;

namespace binarization.Scripts;

public static class SauvolaBinarization {
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

        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                
                Array.Clear(block, 0, block.Length);
                
                for (var i = -radius; i <= radius; i++) {
                    for (var j = -radius; j <= radius; j++) {
                        var pNeighbourPixel = pBackBuffer + (i + radius) * stride + (j + radius) * 4;
                        var colorNeighbourData = Marshal.ReadInt32(pNeighbourPixel);
                        
                        var index = (i + radius) * blockSize + (j + radius);
                        block[index] = ColorHelper.CalculateGrayscale(colorNeighbourData);
                    }
                }
                
                var mean = CalculateMean(block);
                var standardDeviation = CalculateStandardDeviation(block, mean);
                var threshold = mean * (1 + k * ((standardDeviation / r) - 1));
                
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

    private static double CalculateMean(byte[] block) {
        var sum = 0.0;
        for (var i = 0; i < block.Length; i++) {
            sum += block[i];
        }
        
        return sum / block.Length;
    }
    
    private static double CalculateStandardDeviation(byte[] block, double mean) {
        var sum = 0.0;
        for (var i = 0; i < block.Length; i++) {
            sum += Math.Pow(block[i] - mean, 2);
        }
        
        return Math.Sqrt(sum / block.Length);
    }
}