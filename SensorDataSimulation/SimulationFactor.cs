namespace SensorDataSimulation;

public readonly struct SimulationFactor(float constant, IEnumerable<WaveParameters> sineParameters)
{
    public readonly float Constant = constant;
    public readonly IEnumerable<WaveParameters> SineParameters = sineParameters;

    public float Compute(float time) {
        return Constant + SineParameters.Sum(parameters => parameters.Compute(time));
    }
}
