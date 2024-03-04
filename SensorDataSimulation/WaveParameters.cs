namespace SensorDataSimulation;

public readonly struct WaveParameters(float amplitude, float phase, float frequency)
{
    public readonly float Amplitude = amplitude;
    public readonly float Phase = phase;
    public readonly float Frequency = frequency;

    public float Compute(float time) {
        return Amplitude * MathF.Sin(Frequency * time + Phase);
    }
}
