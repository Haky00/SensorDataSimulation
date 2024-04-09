namespace SensorDataSimulation;

public class SimulationParameters(LegParameters legParameters, IEnumerable<BoneParameters> boneParameters)
{
    public readonly LegParameters Legs = legParameters;
    public readonly IEnumerable<BoneParameters> Bones = boneParameters;
}
