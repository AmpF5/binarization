using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Media;
using binarization.Helpers;

namespace binarization.Scripts;

public static class PhansalkaraBinarization {
    public static ImageSource Binarize(ImageSource image, int windowSize = 5, int p = 3, int q = 10, double k = 0.25, double r = 0.5) {
        var stopwatch = Stopwatch.StartNew();
        
        var writeableImage = image.ConvertToWriteableBitmap();
        
        var width = writeableImage.PixelWidth;
        var height = writeableImage.PixelHeight;
        var pixelFormatSize = writeableImage.Format.BitsPerPixel / 8;
        var stride = writeableImage.BackBufferStride;
        var pBackBuffer = writeableImage.BackBuffer;
        var grayscaleBuffer = new byte[width * height];
        r = r * 256;
        
        var radius = (windowSize - 1) / 2;
        var blockSize = 2 * radius + 1;
        
        writeableImage.Lock();
        var pBackBufferCopy = writeableImage.BackBuffer;
        
        Parallel.For(0, height, y => {
            for (var x = 0; x < width; x++) {
                var pPixel = pBackBufferCopy + y * stride + x * pixelFormatSize;
                var colorData = Marshal.ReadInt32(pPixel);

                var grayscale = ColorHelper.CalculateGrayscale(colorData);
                grayscaleBuffer[y * width + x] = grayscale;
            }
        });
        
        var blockLock = new ThreadLocal<byte[]>(() => new byte[blockSize * blockSize]);

        Parallel.For(radius, height - radius, y => {
            var block = blockLock.Value;
            for (var x = radius; x < width - radius; x++) {
                for (var i = -radius; i <= radius; i++) {
                    for (var j = -radius; j <= radius; j++) {
                        var index = (i + radius) * blockSize + (j + radius);
                        block[index] = grayscaleBuffer[(y + i) * width + (x + j)];
                    }
                }

                var mean = MathHelper.CalculateMean(block);
                var standardDeviation = MathHelper.CalculateStandardDeviation(block, mean);
                var threshold = (short) Math.Round(mean * (1 + p * Math.Exp(-q * mean) + k * ((standardDeviation / r) - 1)));

                var grayscale = grayscaleBuffer[y * width + x];

                var binaryColor = grayscale < threshold ? (byte) 0 : (byte) 255;
                var binarizedColorData = (255 << 24) | (binaryColor << 16) | (binaryColor << 8) | binaryColor;

                var pPixel = pBackBuffer + y * stride + x * 4;
                Marshal.WriteInt32(pPixel, binarizedColorData);
            }
        });
        
        writeableImage.Unlock();
        
        var timeTaken = stopwatch.Elapsed;
        Console.WriteLine("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
        
        return writeableImage;
    }
}