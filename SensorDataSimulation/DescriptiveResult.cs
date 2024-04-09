namespace SensorDataSimulation;

/// <summary>
/// The result class the holds the analysis results
/// </summary>
public class DescriptiveResult
{
    // sortedData is used to calculate percentiles
    internal double[]? sortedData;

    /// <summary>
    /// DescriptiveResult default constructor
    /// </summary>
    public DescriptiveResult() { }

    /// <summary>
    /// Count
    /// </summary>
    public uint Count;
    /// <summary>
    /// Sum
    /// </summary>
    public double Sum;
    /// <summary>
    /// Arithmatic mean
    /// </summary>
    public double Mean;
    /// <summary>
    /// Geometric mean
    /// </summary>
    public double GeometricMean;
    /// <summary>
    /// Harmonic mean
    /// </summary>
    public double HarmonicMean;
    /// <summary>
    /// Minimum value
    /// </summary>
    public double Min;
    /// <summary>
    /// Maximum value
    /// </summary>
    public double Max;
    /// <summary>
    /// The range of the values
    /// </summary>
    public double Range;
    /// <summary>
    /// Sample variance
    /// </summary>
    public double Variance;
    /// <summary>
    /// Sample standard deviation
    /// </summary>
    public double StdDev;
    /// <summary>
    /// Skewness of the data distribution
    /// </summary>
    public double Skewness;
    /// <summary>
    /// Kurtosis of the data distribution
    /// </summary>
    public double Kurtosis;
    /// <summary>
    /// Interquartile range
    /// </summary>
    public double IQR;
    /// <summary>
    /// Median, or second quartile, or at 50 percentile
    /// </summary>
    public double Median;
    /// <summary>
    /// First quartile, at 25 percentile
    /// </summary>
    public double FirstQuartile;
    /// <summary>
    /// Third quartile, at 75 percentile
    /// </summary>
    public double ThirdQuartile;


    /// <summary>
    /// Sum of Error
    /// </summary>
    internal double SumOfError;

    /// <summary>
    /// The sum of the squares of errors
    /// </summary>
    internal double SumOfErrorSquare;

    /// <summary>
    /// Percentile
    /// </summary>
    /// <param name="percent">Pecentile, between 0 to 100</param>
    /// <returns>Percentile</returns>
    public double Percentile(double percent)
    {
        if (sortedData is null)
        {
            return -1;
        }
        return Descriptive.percentile(sortedData, percent);
    }
} // end of class DescriptiveResult

