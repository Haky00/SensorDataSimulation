namespace SensorDataSimulation.MovementTemplates;

// Template for a sitting motion with given constant facing direction and time between breaths
public class SittingTemplate(float constDirection, float breathTime) : IMovementTemplate
{
    public string Name { get => "Sitting"; }

    // Basic bones and legs setup
    public const int BoneWavesPerFactor = 6;
    public const int LegsVelocityWaves = 0;
    public const int LegsDirectionWaves = 0;
    const float MinFrequency = 2 * MathF.PI / 35f;

    // Frequncy of one wave in each bone for each factor is set to exactly match stepTime
    private const int WavesWithSetFrequencyPerFactor = 1;

    // Constant X velocity is walkingSpeed, and Y and Z are set to 0
    private const int SetConstantsPerVelocityDirection = 1;
    // Frequency of one wave in each velocity direction is set to exactly match stepTime
    private const int SetFrequenciesPerVelocityDirection = 0;
    // Frequency of one wave in direction waves is set to exactly match stepTime
    private const int DirectionSetFrequencies = 0;
    // Shoulder and upper arm should not move
    private const int BonesWithoutMovement = 3;

    private static readonly string[] MovingBones = ["Lower Arm", "Hand"];
    private static readonly string[] StationaryBones = ["Torso", "Shoulder", "Upper Arm"];

    // Values calculated from skeleton parameters setup before
    public const int BoneFactorLength = 1 + BoneWavesPerFactor * Settings.WaveParameterCount;
    public const int BoneChromosomeLength = Settings.BoneFactorCount * BoneFactorLength;
    public const int BonesChromosomeLength = (Settings.BonesCount - BonesWithoutMovement) * BoneChromosomeLength + BonesWithoutMovement * Settings.BoneFactorCount;
    public const int LegsVelocityFactorLength = 1 + LegsVelocityWaves * Settings.WaveParameterCount;
    public const int LegsDirectionLength = 1 + LegsDirectionWaves * Settings.WaveParameterCount;
    public const int LegsChromosomeLength = LegsDirectionLength + 3 * LegsVelocityFactorLength;
    public const int MaxChromosomeLength = LegsChromosomeLength + BonesChromosomeLength;
    private const int WavesWithSetFrequencyPerBone = WavesWithSetFrequencyPerFactor * Settings.BoneFactorCount;
    private const int BonesSetGenesCount = WavesWithSetFrequencyPerBone * (Settings.BonesCount - BonesWithoutMovement);
    private const int BoneGeneCount = BoneChromosomeLength - WavesWithSetFrequencyPerBone;
    private const int BonesGeneCount = BonesChromosomeLength - BonesSetGenesCount;
    private const int LegsSetGenesCount = 3 * SetConstantsPerVelocityDirection + 3 * SetFrequenciesPerVelocityDirection + DirectionSetFrequencies;
    private const int LegsGeneCount = LegsChromosomeLength - LegsSetGenesCount;
    private const int LegsVelocityDirectionGeneCount = LegsVelocityFactorLength - SetConstantsPerVelocityDirection - SetFrequenciesPerVelocityDirection;
    private const int LegsDirectionGeneCount = LegsDirectionLength - DirectionSetFrequencies;

    // Total number of genes that can be ommited because they will be constant
    private const int SetGenesCount = BonesSetGenesCount + LegsSetGenesCount;

    public int ChromosomeLength { get; } = MaxChromosomeLength - SetGenesCount;

    // 35 second simulation
    public float SimulationLength { get; } = 35f;

    // 40 ms simulation intervals
    public float SimulationTimestep { get; } = 0.040f;

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

    private const double VelocityMeanTarget = 0.006536578384818213;
    private const double VelocityStdDevTarget = 0.0031485489303939664;
    private const double AccelerationMeanTarget = 0.0700441741975513;
    private const double AccelerationStdDevTarget = 0.08289680478705595;
    private const double UpFacingValueMeanTarget = 0.773362970126794;
    private const double UpFacingValueStdDevTarget = 0.011569981076266491;
    private const double VerticalAlignmentsMeanTarget = 0.9947331032183104;
    private const double VerticalAlignmentsStdDevTarget = 0.005830482082136991;
    private const double AngleVelocityMeanTarget = 0.10745778488993558;
    private const double AngleVelocityStdDevTarget = 2.6593325544282607;

    public FitnessScore EvaluateSimulationResults(SimulationResults results)
    {
        FitnessScore fitness = new();
        // Encourage non-zero genes
        // fitness.AddWeighedScoreLinear("nonZeroGenes", 2, results.Parameters.NonZeroParameterPortion);
        // Step time should have a large impact on legs and bones movement
        double boneAmplitudePortions = results.Parameters.Bones.Where(x => MovingBones.Contains(x.BoneName)).Sum(x => x.ParametersAmplitudePortion(1));
        fitness.AddWeighedScoreLinear("boneAmplitudePortions", 1, boneAmplitudePortions * 4);
        // The phone should face the simulated user head
        fitness.AddWeighedScoreLinear("averageFacingValue", 6, results.FacingValues.Average());
        // The angle, amount and roll values should not be too high
        fitness.AddWeighedScoreLinear("maxAmountValue", 6, Math.Log10(Math.Max(results.MaxAmountValue - 1.2, 1)));
        fitness.AddWeighedScoreLinear("maxAngleValue", 6, Math.Log10(Math.Max(results.MaxAngleValue - 2, 1)));
        fitness.AddWeighedScoreLinear("maxRollValue", 6, Math.Log10(Math.Max(results.MaxRollValue - 1.2, 1)));

        // Try to get close to standard deviation target
        Descriptive velocitiesDesc = new([.. results.PhoneVelocities]);
        velocitiesDesc.Analyze();
        double velocitiesMeanTargetHit = Math.Min(velocitiesDesc.Result.Mean, VelocityMeanTarget) / Math.Max(velocitiesDesc.Result.Mean, VelocityMeanTarget);
        double velocitiesStdDevTargetHit = Math.Min(velocitiesDesc.Result.StdDev, VelocityStdDevTarget) / Math.Max(velocitiesDesc.Result.StdDev, VelocityStdDevTarget);
        fitness.AddWeighedScoreLinear("velocitiesMeanTargetHit", 8, velocitiesMeanTargetHit);
        fitness.AddWeighedScoreLinear("velocitiesStdDevTargetHit", 4, velocitiesStdDevTargetHit);

        Descriptive accelerationDesc = new([.. results.PhoneAccelerations]);
        accelerationDesc.Analyze();
        double accelerationMeanTargetHit = Math.Min(accelerationDesc.Result.Mean, AccelerationMeanTarget) / Math.Max(accelerationDesc.Result.Mean, AccelerationMeanTarget);
        double accelerationStdDevTargetHit = Math.Min(accelerationDesc.Result.StdDev, AccelerationStdDevTarget) / Math.Max(accelerationDesc.Result.StdDev, AccelerationStdDevTarget);
        fitness.AddWeighedScoreLinear("accelerationMeanTargetHit", 4, accelerationMeanTargetHit);
        fitness.AddWeighedScoreLinear("accelerationStdDevTargetHit", 2, accelerationStdDevTargetHit);

        Descriptive upValueDesc = new([.. results.PhoneUpFacingValues]);
        upValueDesc.Analyze();
        double upValueMeanTargetHit = Math.Min(upValueDesc.Result.Mean, UpFacingValueMeanTarget) / Math.Max(upValueDesc.Result.Mean, UpFacingValueMeanTarget);
        double upValueStdDevTargetHit = Math.Min(upValueDesc.Result.StdDev, UpFacingValueStdDevTarget) / Math.Max(upValueDesc.Result.StdDev, UpFacingValueStdDevTarget);
        fitness.AddWeighedScoreLinear("upValueMeanTargetHit", 4, upValueMeanTargetHit);
        fitness.AddWeighedScoreLinear("upValueStdDevTargetHit", 2, upValueStdDevTargetHit);

        Descriptive verticalAlignmentsDesc = new([.. results.PhoneVerticalAlignments]);
        verticalAlignmentsDesc.Analyze();
        double verticalMeanTargetHit = Math.Min(verticalAlignmentsDesc.Result.Mean, VerticalAlignmentsMeanTarget) / Math.Max(verticalAlignmentsDesc.Result.Mean, VerticalAlignmentsMeanTarget);
        double verticalStdDevTargetHit = Math.Min(verticalAlignmentsDesc.Result.StdDev, VerticalAlignmentsStdDevTarget) / Math.Max(verticalAlignmentsDesc.Result.StdDev, VerticalAlignmentsStdDevTarget);
        fitness.AddWeighedScoreLinear("verticalMeanTargetHit", 4, verticalMeanTargetHit);
        fitness.AddWeighedScoreLinear("verticalStdDevTargetHit", 2, verticalStdDevTargetHit);

        Descriptive angleVelocitiesDesc = new([.. results.PhoneAngleVelocities]);
        angleVelocitiesDesc.Analyze();
        double angleVelocitiesMeanTargetHit = Math.Min(angleVelocitiesDesc.Result.Mean, AngleVelocityMeanTarget) / Math.Max(angleVelocitiesDesc.Result.Mean, AngleVelocityMeanTarget);
        double angleVelocitiesStdDevTargetHit = Math.Min(angleVelocitiesDesc.Result.StdDev, AngleVelocityStdDevTarget) / Math.Max(angleVelocitiesDesc.Result.StdDev, AngleVelocityStdDevTarget);
        fitness.AddWeighedScoreLinear("angleVelocitiesMeanTargetHit", 4, angleVelocitiesMeanTargetHit);
        fitness.AddWeighedScoreLinear("angleVelocitiesStdDevTargetHit", 2, angleVelocitiesStdDevTargetHit);

        return fitness;
    }

    // Converts passed genes to simulation parameters
    public SimulationParameters GenesToParameters(float[] genes)
    {
        ReadOnlySpan<float> bonesGenes = genes.AsSpan(0, BonesGeneCount);

        LegParameters legParameters = new(new(0, []), new(0, []), new(0, []), new(constDirection, []));

        List<BoneParameters> movingBoneParameters = [];
        for (int bone = 0; bone < MovingBones.Length; bone++)
        {
            ReadOnlySpan<float> boneGenes = bonesGenes.Slice(bone * BoneGeneCount, BoneGeneCount);
            string boneName = MovingBones[bone];
            movingBoneParameters.Add(GetBoneParameters(boneGenes, boneName));
        }
        List<BoneParameters> stationaryBoneParameters = [];
        for (int bone = 0; bone < StationaryBones.Length; bone++)
        {
            ReadOnlySpan<float> boneGenes = bonesGenes.Slice((MovingBones.Length * BoneGeneCount) + bone * Settings.BoneFactorCount, Settings.BoneFactorCount);
            string boneName = StationaryBones[bone];
            stationaryBoneParameters.Add(new(boneName, new(boneGenes[0], []), new(boneGenes[1], []), new(boneGenes[2], [])));
        }
        List<BoneParameters> boneParameters = [.. stationaryBoneParameters, movingBoneParameters[0], movingBoneParameters[1]];
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
        WaveParameters firstWave = new(genes[1], genes[2], 1 / breathTime * MathF.PI);
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
}
