using Newtonsoft.Json;

namespace SensorDataSimulation;

// Represents a single factor in the simulation parameters
// A factor contains a constant value and a list of parameters for computing sines
public readonly struct SimulationFactor(float constant, IReadOnlyList<WaveParameters> sineParameters)
{
    [JsonProperty("constant")]
    public readonly float Constant = constant;

    [JsonProperty("sines")]
    public readonly IReadOnlyList<WaveParameters> SineParameters = sineParameters;

    public float Compute(float time)
    {
        return Constant + SineParameters.Sum(parameters => parameters.Compute(time));
    }

    // Returns the theoretical maximum of this factor: absolute value of constant + absolute amplitudes of all sines
    [JsonIgnore]
    public float TheoreticalMaximum
    {
        get => Math.Abs(Constant) + SineParameters.Sum(x => MathF.Abs(x.Amplitude));
    }

    // Returns how big the portion of the amplitudes of the n first sines in this factor is when compared to all amplitudes
    public float ParametersAmplitudePortion(int n)
    {
        if (n > SineParameters.Count)
        {
            throw new ArgumentException("Supplied number of parameters to calculate portion from is larger than total number of parameters");
        }
        float total = SineParameters.Sum(x => MathF.Abs(x.Amplitude));
        if (total == 0)
        {
            return 0;
        }
        return MathF.Abs(SineParameters.Take(n).Sum(x => MathF.Abs(x.Amplitude))) / total;
    }

    // Returns the portion of child parameters that are not zero
    [JsonIgnore]
    public float NonZeroParameterPortion
    {
        get => (SineParameters.Sum(x => x.NonZeroParameters) + (Constant != 0 ? 1 : 0)) / (SineParameters.Count * 3 + 1f);
    }
}