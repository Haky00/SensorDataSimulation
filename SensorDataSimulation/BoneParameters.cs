using Newtonsoft.Json;

namespace SensorDataSimulation;

public readonly struct BoneParameters(string boneName, SimulationFactor angle, SimulationFactor amount, SimulationFactor roll)
{
    public readonly string BoneName = boneName;
    public readonly SimulationFactor Angle = angle;
    public readonly SimulationFactor Amount = amount;
    public readonly SimulationFactor Roll = roll;

    public float ParametersAmplitudePortion(int n)
    {
        return (Angle.ParametersAmplitudePortion(n) + Amount.ParametersAmplitudePortion(n) + Roll.ParametersAmplitudePortion(n)) / 3f;
    }

    [JsonIgnore]
    public float NonZeroParameterPortion
    {
        get => (Angle.NonZeroParameterPortion + Amount.NonZeroParameterPortion + Roll.NonZeroParameterPortion) / 3f;
    }
}
