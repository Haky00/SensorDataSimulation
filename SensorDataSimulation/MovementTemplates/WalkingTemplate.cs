namespace SensorDataSimulation.MovementTemplates;

public class WalkingTemplate(float walkingSpeed, float stepTime) : IChromosomeTemplate
{
    public string Name { get => "Walking"; }

    // Basic bones and legs setup
    public const int BoneWavesPerFactor = 3;
    public const int LegsVelocityWaves = 3;
    public const int LegsDirectionWaves = 3;

    // Frequncy of one wave in each bone for each factor is set to exactly match stepTime
    private const int WavesWithSetFrequencyPerFactor = 1;

    // Constant X velocity is walkingSpeed, and Y and Z are set to 0
    private const int SetConstantsPerVelocityDirection = 1;
    // Frequency of one wave in each velocity direction is set to exactly match stepTime
    private const int SetFrequenciesPerVelocityDirection = 1;
    // Frequency of one wave in direction waves is set to exactly match stepTime
    private const int DirectionSetFrequencies = 1;

    // Values calculated from skeleton parameters setup before
    public const int BoneFactorLength = 1 + BoneWavesPerFactor * Settings.WaveParameterCount;
    public const int BoneChromosomeLength = Settings.BoneFactorCount * BoneFactorLength;
    public const int BonesChromosomeLength = Settings.BonesCount * BoneChromosomeLength;
    public const int LegsVelocityFactorLength = 1 + LegsVelocityWaves * Settings.WaveParameterCount;
    public const int LegsDirectionLength = 1 + LegsDirectionWaves * Settings.WaveParameterCount;
    public const int LegsChromosomeLength = LegsDirectionLength + 3 * LegsVelocityFactorLength;
    public const int MaxChromosomeLength = LegsChromosomeLength + BonesChromosomeLength;
    private const int WavesWithSetFrequencyPerBone = WavesWithSetFrequencyPerFactor * Settings.BoneFactorCount;
    private const int BonesSetGenesCount = WavesWithSetFrequencyPerBone * Settings.BonesCount;
    private const int BoneGeneCount = BoneChromosomeLength - WavesWithSetFrequencyPerBone;
    private const int BonesGeneCount = Settings.BonesCount * BoneGeneCount;
    private const int LegsSetGenesCount = 3 * SetConstantsPerVelocityDirection + 3 * SetFrequenciesPerVelocityDirection + DirectionSetFrequencies;
    private const int LegsGeneCount = LegsChromosomeLength - LegsSetGenesCount;
    private const int LegsVelocityDirectionGeneCount = LegsVelocityFactorLength - SetConstantsPerVelocityDirection - SetFrequenciesPerVelocityDirection;
    private const int LegsDirectionGeneCount = LegsDirectionLength - DirectionSetFrequencies;

    // Total number of genes that can be ommited because they will be constant
    private const int SetGenesCount = BonesSetGenesCount + LegsSetGenesCount;

    public int ChromosomeLength { get; } = MaxChromosomeLength - SetGenesCount;

    // 35 second simulation
    public float SimulationLength { get; } = 35f;

    // 20 ms simulation intervals
    public float SimulationTimestep { get; } = 0.020f;

    public float[] GetInitialGenes()
    {
        Random rnd = new();
        float[] values = new float[ChromosomeLength];
        for (int i = 0; i < ChromosomeLength; i++)
        {
            values[i] = (float)(rnd.NextDouble() - 0.5) * 0.05f;
        }
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
        int factorLength = BoneFactorLength - WavesWithSetFrequencyPerFactor;
        SimulationFactor angle = GetBoneSimulationFactor(genes.Slice(0, factorLength));
        SimulationFactor amount = GetBoneSimulationFactor(genes.Slice(factorLength, factorLength));
        SimulationFactor roll = GetBoneSimulationFactor(genes.Slice(factorLength * 2, factorLength));
        return new(boneName, angle, amount, roll);
    }

    private SimulationFactor GetBoneSimulationFactor(ReadOnlySpan<float> genes)
    {
        float constant = genes[0];
        WaveParameters firstWave = new(genes[1], genes[2], 1 / stepTime * MathF.PI);
        List<WaveParameters> waves = [firstWave];
        for (int i = 1; i < BoneWavesPerFactor; i++)
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
        WaveParameters firstWave = new(genes[0], genes[1], 1 / stepTime * MathF.PI);
        List<WaveParameters> waves = [firstWave];
        for (int i = 1; i < LegsVelocityWaves; i++)
        {
            waves.Add(new(genes[i * 3 - 1], genes[i * 3], genes[i * 3 + 1]));
        }
        return waves;
    }

    private const double AccelerationMeanTarget = 0.022836004595493477;
    private const double AccelerationStdDevTarget = 0.02350276981332964;
    private const double UpFacingValueMeanTarget = 0.5086066038130083;
    private const double UpFacingValueStdDevTarget = 0.017333915711476692;
    private const double AngleVelocityMeanTarget = 0.5741896104174383;
    private const double AngleVelocityStdDevTarget = 2.3133170685260254;

    public FitnessScore EvaluateSimulationResults(SimulationResults results)
    {
        FitnessScore fitness = new();
        // Encourage non-zero genes
        // fitness.AddWeighedScoreLinear("nonZeroGenes", 2, results.Parameters.NonZeroParameterPortion);
        // Step time should have a large impact on legs and bones movement
        double boneAmplitudePortions = results.Parameters.BoneParametersAmplitudePortion(1);
        fitness.AddWeighedScoreLinear("boneAmplitudePortions", 1, boneAmplitudePortions * 5);
        double walkingAmplitudePortions = results.Parameters.Legs.VelocityParametersAmplitudePortion(1);
        fitness.AddWeighedScoreLinear("walkingAmplitudePortions", 10, walkingAmplitudePortions);
        // The phone should face the simulated user head
        fitness.AddWeighedScoreLinear("averageFacingValue", 6, results.FacingValues.Average());
        // The angle, amount and roll values should not be too high
        fitness.AddWeighedPenaltySqrt("maxAmountValue", 4, Math.Log10(Math.Max(results.MaxAmountValue - 1.2, 1)));
        fitness.AddWeighedPenaltySqrt("maxAngleValue", 4, Math.Log10(Math.Max(results.MaxAngleValue - 2, 1)));
        fitness.AddWeighedPenaltySqrt("maxRollValue", 4, Math.Log10(Math.Max(results.MaxRollValue - 1.2, 1)));
        // The legs should not move (periodically) too much
        List<float> legsOffests = results.LegsPositionOffsets.Select(x => x.Length()).ToList();
        if (legsOffests.Max() > 0.15)
        {
            fitness.AddWeighedPenaltyLinear("maxLegsOffsetLength", 10, (legsOffests.Max() - 0.15) / 15f);
        }
        // The legs should not move (periodically) too little
        if (legsOffests.Average() < 0.05)
        {
            fitness.AddWeighedPenaltySqrt("legsOffsetsMeanLow", 5, (0.05 - legsOffests.Average()) * (1 / 0.05));
        }
        // The direction of legs should not be too high, for smooth transfers between other movements
        double maxLegsDirection = results.LegsDirections.Max(Math.Abs);
        if (maxLegsDirection > 2)
        {
            fitness.AddWeighedPenaltySqrt("maxLegsDirection", 50, maxLegsDirection - 2);
        }
        // The legs should not steer too fast
        double avgLegsDirectionVelocity = results.LegsDirectionVelocities.Average();
        if (avgLegsDirectionVelocity > 0.1)
        {
            fitness.AddWeighedPenaltySqrt("legsAngleVelocityMeanHigh", 50, (avgLegsDirectionVelocity - 0.1) * 50);
        }
        // The legs should not steer to little
        if (avgLegsDirectionVelocity < 0.01)
        {
            fitness.AddWeighedPenaltyLinear("legsAngleVelocityMeanLow", 50, (0.01 - avgLegsDirectionVelocity) * (1 / 0.01));
        }
        // Try to get close to standard deviation target
        Descriptive accelerationDesc = new([.. results.PhoneAccelerations]);
        accelerationDesc.Analyze();
        double accelerationStdDevTargetHit = Math.Min(accelerationDesc.Result.StdDev, AccelerationStdDevTarget) / Math.Max(accelerationDesc.Result.StdDev, AccelerationStdDevTarget);
        fitness.AddWeighedScoreSqrt("accelerationStdDevTargetHit", 2, accelerationStdDevTargetHit);

        Descriptive upValueDesc = new([.. results.PhoneUpFacingValues]);
        upValueDesc.Analyze();
        double upValueMeanTargetHit = Math.Min(upValueDesc.Result.Mean, UpFacingValueMeanTarget) / Math.Max(upValueDesc.Result.Mean, UpFacingValueMeanTarget);
        double upValueStdDevTargetHit = Math.Min(upValueDesc.Result.StdDev, UpFacingValueStdDevTarget) / Math.Max(upValueDesc.Result.StdDev, UpFacingValueStdDevTarget);
        fitness.AddWeighedScoreSqrt("upValueMeanTargetHit", 4, upValueMeanTargetHit);
        fitness.AddWeighedScoreSqrt("upValueStdDevTargetHit", 2, upValueStdDevTargetHit);

        Descriptive angleVelocitiesDesc = new([.. results.PhoneAngleVelocities]);
        angleVelocitiesDesc.Analyze();
        double angleVelocitiesMeanTargetHit = Math.Min(angleVelocitiesDesc.Result.Mean, AngleVelocityMeanTarget) / Math.Max(angleVelocitiesDesc.Result.Mean, AngleVelocityMeanTarget);
        double angleVelocitiesStdDevTargetHit = Math.Min(angleVelocitiesDesc.Result.StdDev, AngleVelocityStdDevTarget) / Math.Max(angleVelocitiesDesc.Result.StdDev, AngleVelocityStdDevTarget);
        fitness.AddWeighedScoreSqrt("angleVelocitiesMeanTargetHit", 4, angleVelocitiesMeanTargetHit);
        fitness.AddWeighedScoreSqrt("angleVelocitiesStdDevTargetHit", 2, angleVelocitiesStdDevTargetHit);

        //double meanTargetHit = d.Result.Mean > meanTarget ? meanTarget / d.Result.Mean : d.Result.Mean / meanTarget;
        //fitness.AddWeighedScoreSqrt("meanTargetHit", 15, meanTargetHit);
        return fitness;
    }
}
