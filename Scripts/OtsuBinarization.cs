using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using binarization.Helpers;

namespace binarization.Scripts;

public static class OtsuBinarization {
    public static ImageSource Binarize(ImageSource image) {
        var stopwatch = Stopwatch.StartNew();
        
        var writeableImage = image.ConvertToWriteableBitmap();
        
        var w = writeableImage.PixelWidth;
        var h = writeableImage.PixelHeight;
        var bytesPerPixel = writeableImage.Format.BitsPerPixel / 8;
        var stride = writeableImage.BackBufferStride;
        var bytes = stride * h;
        var buffer = new byte[bytes];
        var result = new byte[bytes];

        writeableImage.CopyPixels(buffer, stride, 0);

        var histogram = new double[256];
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var index = y * stride + x * bytesPerPixel;
                histogram[buffer[index]]++;
            }
        }
        histogram = histogram.Select(x => x / (w * h)).ToArray();

        double mg = 0;
        for (var i = 0; i < 256; i++)
        {
            mg += i * histogram[i];
        }

        double bcv = 0;
        var threshold = 0;
        for (var i = 0; i < 256; i++)
        {
            double cs = 0;
            double m = 0;
            for (var j = 0; j < i; j++)
            {
                cs += histogram[j];
                m += j * histogram[j];
            }

            if (cs == 0 || cs == 1)
            {
                continue;
            }

            var newBcv = Math.Pow(mg * cs - m, 2) / (cs * (1 - cs));
            if (newBcv > bcv)
            {
                bcv = newBcv;
                threshold = i;
            }
        }

        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var index = y * stride + x * bytesPerPixel;
                var pixelValue = (byte)((buffer[index] > threshold) ? 255 : 0);
                result[index] = pixelValue;
                result[index + 1] = pixelValue;
                result[index + 2] = pixelValue;

                if (bytesPerPixel == 4)
                {
                    result[index + 3] = 255;
                }
            }
        }

        var resultImage = new WriteableBitmap(w, h, writeableImage.DpiX, writeableImage.DpiY, PixelFormats.Bgr32, null);
        resultImage.WritePixels(new System.Windows.Int32Rect(0, 0, w, h), result, stride, 0);
        
        var timeTaken = stopwatch.Elapsed;
        Console.WriteLine("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
        
        return resultImage;
    }
}