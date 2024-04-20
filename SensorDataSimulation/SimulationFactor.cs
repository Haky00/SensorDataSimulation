using Newtonsoft.Json;

namespace SensorDataSimulation;

public readonly struct SimulationFactor(float constant, IReadOnlyList<WaveParameters> sineParameters)
{
    public readonly float Constant = constant;
    public readonly IReadOnlyList<WaveParameters> SineParameters = sineParameters;

    public float Compute(float time)
    {
        return Constant + SineParameters.Sum(parameters => parameters.Compute(time));
    }

    [JsonIgnore]
    public float TheoreticalMaximum
    {
        get => Math.Abs(Constant) + SineParameters.Sum(x => MathF.Abs(x.Amplitude));
    }

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

    [JsonIgnore]
    public float NonZeroParameterPortion
    {
        get => (SineParameters.Sum(x => x.NonZeroParameters) + (Constant != 0 ? 1 : 0)) / (SineParameters.Count * 3 + 1f);
    }
}