using Newtonsoft.Json;

namespace SensorDataSimulation;

// Represents simulation parameters for a single bone
public readonly struct BoneParameters(string boneName, SimulationFactor angle, SimulationFactor amount, SimulationFactor roll)
{
    [JsonProperty("name")]
    public readonly string BoneName = boneName;
    
    [JsonProperty("angle")]
    public readonly SimulationFactor Angle = angle;
    
    [JsonProperty("amount")]
    public readonly SimulationFactor Amount = amount;
    
    [JsonProperty("roll")]
    public readonly SimulationFactor Roll = roll;

    // Returns how big the portion of the amplitudes of the n first sines in angle, amount and roll is when compared to all amplitudes
    public float ParametersAmplitudePortion(int n)
    {
        return (Angle.ParametersAmplitudePortion(n) + Amount.ParametersAmplitudePortion(n) + Roll.ParametersAmplitudePortion(n)) / 3f;
    }

    // Returns the portion of child parameters that are not zero
    [JsonIgnore]
    public float NonZeroParameterPortion
    {
        get => (Angle.NonZeroParameterPortion + Amount.NonZeroParameterPortion + Roll.NonZeroParameterPortion) / 3f;
    }
}
