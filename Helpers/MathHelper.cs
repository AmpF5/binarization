namespace binarization.Helpers;

public static class MathHelper {
    public static double CalculateMean(byte[] block) {
        var sum = 0.0;
        for (var i = 0; i < block.Length; i++) {
            sum += block[i];
        }
        
        return sum / block.Length;
    }
    
    public static double CalculateStandardDeviation(byte[] block, double mean) {
        var sum = 0.0;
        for (var i = 0; i < block.Length; i++) {
            sum += Math.Pow(block[i] - mean, 2);
        }
        
        return Math.Sqrt(sum / block.Length);
    }
}