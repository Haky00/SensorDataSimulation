namespace SensorDataSimulation;

public static class Settings
{
    public const int BonesCount = 7;
    public static readonly string[] BoneNames = ["Legs", "Torso", "Head", "Shoulder", "Upper Arm", "Lower Arm", "Hand"];
    public const int WavesPerFactor = 5;
    public const int FactorCount = 3;
    public const int BoneFactorLength = 1 + WavesPerFactor * 3;
    public const int ChromosomeLength = BonesCount * FactorCount * BoneFactorLength;
}
