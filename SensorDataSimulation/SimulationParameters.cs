using Newtonsoft.Json;

namespace SensorDataSimulation;

public class SimulationParameters(LegParameters legParameters, IReadOnlyList<BoneParameters> boneParameters)
{
    public string? Name;
    public readonly LegParameters Legs = legParameters;
    public readonly IReadOnlyList<BoneParameters> Bones = boneParameters;

    public float BoneParametersAmplitudePortion(int n) => Bones.Sum(x => x.ParametersAmplitudePortion(n)) / Bones.Count;

    [JsonIgnore]
    public float NonZeroParameterPortion
    {
        get => (Legs.NonZeroParameterPortion + Bones.Sum(x => x.NonZeroParameterPortion)) / (Bones.Count + 1f);
    }

}
