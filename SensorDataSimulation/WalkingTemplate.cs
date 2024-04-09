namespace SensorDataSimulation;

public class WalkingTemplate(float walkingSpeed, float stepTime) : IChromosomeTemplate
{
    // Frequncy of one wave in each bone for each factor is set to exactly match stepTime
    private const int WavesWithSetFrequencyPerFactor = 1;
    private const int WavesWithSetFrequencyPerBone = WavesWithSetFrequencyPerFactor * Settings.BoneFactorCount;

    private const int BonesSetGenesCount = WavesWithSetFrequencyPerBone * Settings.BonesCount;
    private const int BoneGeneCount = Settings.BoneChromosomeLength - WavesWithSetFrequencyPerBone;
    private const int BonesGeneCount = Settings.BonesCount * BoneGeneCount;

    // Constant X velocity is walkingSpeed, and Y and Z are be set to 0
    private const int SetConstantsPerVelocityDirection = 1;
    // Frequency of one wave in each velocity direction is set to exactly match stepTime
    private const int SetFrequenciesPerVelocityDirection = 1;
    // Frequency of one wave in direction waves is set to exactly match stepTime
    private const int DirectionSetFrequencies = 1;

    private const int LegsSetGenesCount = 3 * SetConstantsPerVelocityDirection + 3 * SetFrequenciesPerVelocityDirection + DirectionSetFrequencies;
    private const int LegsGeneCount = Settings.LegsChromosomeLength - LegsSetGenesCount;
    private const int LegsVelocityDirectionGeneCount = Settings.LegsVelocityFactorLength - SetConstantsPerVelocityDirection - SetFrequenciesPerVelocityDirection;
    private const int LegsDirectionGeneCount = Settings.LegsDirectionLength - DirectionSetFrequencies;

    // Total number of genes that can be ommited because they will be constant
    private const int SetGenesCount = BonesSetGenesCount + LegsSetGenesCount;

    public int ChromosomeLength { get; } = Settings.MaxChromosomeLength - SetGenesCount;

    public float[] GetInitialGenes()
    {
        float[] values = new float[ChromosomeLength];
        Array.Clear(values);
        return values;
    }

    public SimulationParameters GenesToParameters(float[] genes)
    {
        ReadOnlySpan<float> legGenes = genes.AsSpan(0, LegsGeneCount);
        ReadOnlySpan<float> bonesGenes = genes.AsSpan(LegsGeneCount, BonesGeneCount);

        LegParameters legParameters = GetLegParameters(legGenes);

        List<BoneParameters> boneParameters = [];
        for (int bone = 0; bone < Settings.BonesCount; bone++)
        {
            ReadOnlySpan<float> boneGenes = bonesGenes.Slice(bone * BoneGeneCount, BoneGeneCount);
            string boneName = Settings.BoneNames[bone];
            boneParameters.Add(GetBoneParameters(boneGenes, boneName));
        }

        return new(legParameters, boneParameters);
    }

    private BoneParameters GetBoneParameters(ReadOnlySpan<float> genes, string boneName)
    {
        int factorLength = Settings.BoneFactorLength - WavesWithSetFrequencyPerFactor;
        SimulationFactor angle = GetBoneSimulationFactor(genes.Slice(0, factorLength));
        SimulationFactor amount = GetBoneSimulationFactor(genes.Slice(factorLength, factorLength));
        SimulationFactor roll = GetBoneSimulationFactor(genes.Slice(factorLength * 2, factorLength));
        return new(boneName, angle, amount, roll);
    }

    private SimulationFactor GetBoneSimulationFactor(ReadOnlySpan<float> genes)
    {
        float constant = genes[0];
        WaveParameters firstWave = new(genes[1], genes[2], stepTime * MathF.PI / 2);
        List<WaveParameters> waves = [firstWave];
        for (int i = 1; i < Settings.BoneWavesPerFactor; i++)
        {
            waves.Add(new(genes[0 + i * 3], genes[1 + i * 3], genes[2 + i * 3]));
        }
        return new(constant, waves);
    }

    private LegParameters GetLegParameters(ReadOnlySpan<float> genes)
    {
        ReadOnlySpan<float> velocityXGenes = genes.Slice(0, LegsVelocityDirectionGeneCount);
        ReadOnlySpan<float> velocityYGenes = genes.Slice(LegsVelocityDirectionGeneCount, LegsVelocityDirectionGeneCount);
        ReadOnlySpan<float> velocityZGenes = genes.Slice(LegsVelocityDirectionGeneCount * 2, LegsVelocityDirectionGeneCount);
        ReadOnlySpan<float> directionGenes = genes.Slice(LegsVelocityDirectionGeneCount * 3, LegsDirectionGeneCount);
        SimulationFactor velocityX = new(walkingSpeed, GetLegVelocityWaveParameters(velocityXGenes));
        SimulationFactor velocityY = new(0, GetLegVelocityWaveParameters(velocityYGenes));
        SimulationFactor velocityZ = new(0, GetLegVelocityWaveParameters(velocityZGenes));
        SimulationFactor direction = new(directionGenes[0], GetLegVelocityWaveParameters(directionGenes[1..]));
        return new(velocityX, velocityY, velocityZ, direction);
    }

    private List<WaveParameters> GetLegVelocityWaveParameters(ReadOnlySpan<float> genes)
    {
        WaveParameters firstWave = new(genes[0], genes[1], stepTime * MathF.PI / 2);
        List<WaveParameters> waves = [firstWave];
        for (int i = 1; i < Settings.LegsVelocityWaves; i++)
        {
            waves.Add(new(genes[i * 3 - 1], genes[i * 3], genes[i * 3 + 1]));
        }
        return waves;
    }
}
