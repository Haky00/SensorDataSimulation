using Newtonsoft.Json;

namespace SensorDataSimulation;

// Contains all parameters for simulating a skeleton motion
public class SimulationParameters(LegParameters legParameters, IReadOnlyList<BoneParameters> boneParameters)
{
    [JsonProperty("name")]
    public string? Name;
    
    [JsonProperty("legs")]
    public readonly LegParameters Legs = legParameters;
    
    [JsonProperty("bones")]
    public readonly IReadOnlyList<BoneParameters> Bones = boneParameters;

    // Returns how big the portion of the amplitudes of the n first sines in bones is when compared to all amplitudes
    public float BoneParametersAmplitudePortion(int n) => Bones.Sum(x => x.ParametersAmplitudePortion(n)) / Bones.Count;

    // Returns the portion of child parameters that are not zero
    [JsonIgnore]
    public float NonZeroParameterPortion
    {
        get => (Legs.NonZeroParameterPortion + Bones.Sum(x => x.NonZeroParameterPortion)) / (Bones.Count + 1f);
    }

}
