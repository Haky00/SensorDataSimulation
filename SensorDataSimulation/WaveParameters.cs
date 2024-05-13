using Newtonsoft.Json;

namespace SensorDataSimulation;

// Parameters for a single sine
public readonly struct WaveParameters(float amplitude, float phase, float frequency)
{
    [JsonProperty("a")]
    public readonly float Amplitude = amplitude;

    [JsonProperty("p")]
    public readonly float Phase = phase;

    [JsonProperty("f")]
    public readonly float Frequency = frequency;

    public float Compute(float time)
    {
        return Amplitude * MathF.Sin(Frequency * time + Phase);
    }

    // Returns the portion of child parameters that are not zero
    [JsonIgnore]
    public int NonZeroParameters
    {
        get
        {
            int nonZeroParameters = 0;
            if (Amplitude != 0)
            {
                nonZeroParameters++;
            }
            if (Phase != 0)
            {
                nonZeroParameters++;
            }
            if (Frequency != 0)
            {
                nonZeroParameters++;
            }
            return nonZeroParameters; ;
        }
    }
}
