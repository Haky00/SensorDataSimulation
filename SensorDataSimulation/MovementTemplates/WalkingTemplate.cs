namespace SensorDataSimulation.MovementTemplates;

// Template for a walking motion with given speed and time between steps
public class WalkingTemplate(float walkingSpeed, float stepTime) : IMovementTemplate
{
    public string Name { get => "Walking"; }

    // Basic bones and legs setup
    public const int BoneWavesPerFactor = 5;
    public const int LegsVelocityWaves = 3;
    public const int LegsDirectionWaves = 3;
    const float MinFrequency = 2 * MathF.PI / 35f;

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

    private const double VelocityMeanTarget = 0.22377654213544979;
    private const double VelocityStdDevTarget = 0.08711275273697351;
    // Acceleration mean is set lower than in real measurements, as the real high values were making the generated movement way too unstable
    private const double AccelerationMeanTarget = 1.2026032686709813;
    private const double AccelerationStdDevTarget = 0.637425618317317;
    private const double UpFacingValueMeanTarget = 0.8516780614784941;
    private const double UpFacingValueStdDevTarget = 0.015626926172786673;
    private const double VerticalAlignmentsMeanTarget = 0.9812675862994678;
    private const double VerticalAlignmentsStdDevTarget = 0.017599923114221077;
    private const double AngleVelocityMeanTarget = 0.5741832154443963;
    private const double AngleVelocityStdDevTarget = 1.31332073902392;

    public FitnessScore EvaluateSimulationResults(SimulationResults results)
    {
        FitnessScore fitness = new();
        // Encourage non-zero genes
        // fitness.AddWeighedScoreLinear("nonZeroGenes", 2, results.Parameters.NonZeroParameterPortion);
        // Step time should have a large impact on legs and bones movement
        double boneAmplitudePortions = results.Parameters.BoneParametersAmplitudePortion(1);
        fitness.AddWeighedScoreLinear("boneAmplitudePortions", 1, boneAmplitudePortions * 2);
        double walkingAmplitudePortions = results.Parameters.Legs.VelocityParametersAmplitudePortion(1);
        fitness.AddWeighedScoreLinear("walkingAmplitudePortions", 10, walkingAmplitudePortions);
        // The phone should face the simulated user head
        fitness.AddWeighedScoreLinear("averageFacingValue", 6, results.FacingValues.Average());
        // The angle, amount and roll values should not be too high
        fitness.AddWeighedPenaltySqrt("maxAmountValue", 4, Math.Log10(Math.Max(results.MaxAmountValue - 1.2, 1)));
        fitness.AddWeighedPenaltySqrt("maxAngleValue", 4, Math.Log10(Math.Max(results.MaxAngleValue - 2, 1)));
        fitness.AddWeighedPenaltySqrt("maxRollValue", 4, Math.Log10(Math.Max(results.MaxRollValue - 1.2, 1)));
        // The legs should not move (periodically) too much
        float maxLegsVelocity = results.Parameters.Legs.TheoreticalMaximumVelocityWithoutConstant.Length();
        if (maxLegsVelocity > 0.10)
        {
            fitness.AddWeighedPenaltyLinear("maxLegsVelocity", 10, (maxLegsVelocity - 0.10) / 5f);
        }
        // The legs should not move (periodically) too little
        if (maxLegsVelocity < 0.01)
        {
            fitness.AddWeighedPenaltySqrt("maxLegsVelocityLow", 5, (0.01 - maxLegsVelocity) * (1 / 0.01));
        }
        // The direction of legs should not be too high, for smooth transfers between other movements
        double maxLegsDirection = results.LegsDirections.Max(Math.Abs);
        fitness.AddWeighedPenaltySqrt("maxLegsDirection", 50, maxLegsDirection - Math.PI);
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
        // Try to get close to real statistics
        // Descriptive velocitiesDesc = new([.. results.PhoneVelocities]);
        // velocitiesDesc.Analyze();
        // double velocitiesMeanTargetHit = Math.Min(velocitiesDesc.Result.Mean, VelocityMeanTarget) / Math.Max(velocitiesDesc.Result.Mean, VelocityMeanTarget);
        // double velocitiesStdDevTargetHit = Math.Min(velocitiesDesc.Result.StdDev, VelocityStdDevTarget) / Math.Max(velocitiesDesc.Result.StdDev, VelocityStdDevTarget);
        // fitness.AddWeighedScoreLinear("velocitiesMeanTargetHit", 4, velocitiesMeanTargetHit);
        // fitness.AddWeighedScoreLinear("velocitiesStdDevTargetHit", 2, velocitiesStdDevTargetHit);

        Descriptive accelerationDesc = new([.. results.PhoneAccelerations]);
        accelerationDesc.Analyze();
        double accelerationMeanTargetHit = Math.Min(accelerationDesc.Result.Mean, AccelerationMeanTarget) / Math.Max(accelerationDesc.Result.Mean, AccelerationMeanTarget);
        double accelerationStdDevTargetHit = Math.Min(accelerationDesc.Result.StdDev, AccelerationStdDevTarget) / Math.Max(accelerationDesc.Result.StdDev, AccelerationStdDevTarget);
        fitness.AddWeighedScoreLinear("accelerationMeanTargetHit", 4, accelerationMeanTargetHit);
        fitness.AddWeighedScoreLinear("accelerationStdDevTargetHit", 8, accelerationStdDevTargetHit);

        Descriptive upValueDesc = new([.. results.PhoneUpFacingValues]);
        upValueDesc.Analyze();
        double upValueMeanTargetHit = Math.Min(upValueDesc.Result.Mean, UpFacingValueMeanTarget) / Math.Max(upValueDesc.Result.Mean, UpFacingValueMeanTarget);
        double upValueStdDevTargetHit = Math.Min(upValueDesc.Result.StdDev, UpFacingValueStdDevTarget) / Math.Max(upValueDesc.Result.StdDev, UpFacingValueStdDevTarget);
        fitness.AddWeighedScoreLinear("upValueMeanTargetHit", 2, upValueMeanTargetHit);
        fitness.AddWeighedScoreLinear("upValueStdDevTargetHit", 1, upValueStdDevTargetHit);

        Descriptive verticalAlignmentsDesc = new([.. results.PhoneVerticalAlignments]);
        verticalAlignmentsDesc.Analyze();
        double verticalMeanTargetHit = Math.Min(verticalAlignmentsDesc.Result.Mean, VerticalAlignmentsMeanTarget) / Math.Max(verticalAlignmentsDesc.Result.Mean, VerticalAlignmentsMeanTarget);
        double verticalStdDevTargetHit = Math.Min(verticalAlignmentsDesc.Result.StdDev, VerticalAlignmentsStdDevTarget) / Math.Max(verticalAlignmentsDesc.Result.StdDev, VerticalAlignmentsStdDevTarget);
        fitness.AddWeighedScoreLinear("verticalMeanTargetHit", 2, verticalMeanTargetHit);
        fitness.AddWeighedScoreLinear("verticalStdDevTargetHit", 1, verticalStdDevTargetHit);

        Descriptive angleVelocitiesDesc = new([.. results.PhoneAngleVelocities]);
        angleVelocitiesDesc.Analyze();
        double angleVelocitiesMeanTargetHit = Math.Min(angleVelocitiesDesc.Result.Mean, AngleVelocityMeanTarget) / Math.Max(angleVelocitiesDesc.Result.Mean, AngleVelocityMeanTarget);
        double angleVelocitiesStdDevTargetHit = Math.Min(angleVelocitiesDesc.Result.StdDev, AngleVelocityStdDevTarget) / Math.Max(angleVelocitiesDesc.Result.StdDev, AngleVelocityStdDevTarget);
        fitness.AddWeighedScoreLinear("angleVelocitiesMeanTargetHit", 8, angleVelocitiesMeanTargetHit);
        fitness.AddWeighedScoreLinear("angleVelocitiesStdDevTargetHit", 4, angleVelocitiesStdDevTargetHit);

        return fitness;
    }

    // Converts passed genes to simulation parameters
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

    // Below are helper functions for gathering parameters from genes

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
            float amplitude = genes[0 + i * 3];
            float phase = genes[1 + i * 3];
            float frequency = genes[2 + i * 3];
            frequency += frequency >= 0 ? MinFrequency : -MinFrequency;
            waves.Add(new(amplitude, phase, frequency));
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
            float amplitude = genes[i * 3 - 1];
            float phase = genes[i * 3];
            float frequency = genes[i * 3 + 1];
            frequency += frequency >= 0 ? MinFrequency : -MinFrequency;
            waves.Add(new(amplitude, phase, frequency));
        }
        return waves;
    }
}
