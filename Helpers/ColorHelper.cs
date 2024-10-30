namespace binarization.Helpers;

public static class ColorHelper {
    
    public static byte CalculateGrayscale(int colorData) {
        var blue = (byte)(colorData & 0xFF);
        var green = (byte)((colorData >> 8) & 0xFF);
        var red = (byte)((colorData >> 16) & 0xFF);
        
        var grayscale = (byte)((red * 0.3) + (green * 0.59) + (blue * 0.11));
        return grayscale;
    }
}