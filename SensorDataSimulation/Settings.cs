namespace SensorDataSimulation;

public static class Settings
{
    public const int WaveParameterCount = 3;
    public const int BonesCount = 5;
    public static readonly string[] BoneNames = ["Torso", "Shoulder", "Upper Arm", "Lower Arm", "Hand"];
    public const int BoneWavesPerFactor = 3;
    public const int BoneFactorLength = 1 + BoneWavesPerFactor * WaveParameterCount;
    public const int BoneFactorCount = 3;
    public const int BoneChromosomeLength = BoneFactorCount * BoneFactorLength;
    public const int BonesChromosomeLength = BonesCount * BoneChromosomeLength;
    public const int LegsVelocityWaves = 3;
    public const int LegsVelocityFactorLength = 1 + LegsVelocityWaves * WaveParameterCount;
    public const int LegsDirectionWaves = 3;
    public const int LegsDirectionLength = 1 + LegsDirectionWaves * WaveParameterCount;
    public const int LegsChromosomeLength = LegsDirectionLength + 3 * LegsVelocityFactorLength;
    public const int MaxChromosomeLength = LegsChromosomeLength + BonesChromosomeLength;    
}
