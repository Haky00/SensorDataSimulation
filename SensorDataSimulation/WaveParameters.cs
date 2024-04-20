using Newtonsoft.Json;

namespace SensorDataSimulation;

public readonly struct WaveParameters(float amplitude, float phase, float frequency)
{
    public readonly float Amplitude = amplitude;
    public readonly float Phase = phase;
    public readonly float Frequency = frequency;

    public float Compute(float time)
    {
        return Amplitude * MathF.Sin(Frequency * time + Phase);
    }

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
