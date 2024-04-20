namespace SensorDataSimulation.MovementTemplates;

public class SittingTemplate(float constDirection, float breathTime) : IChromosomeTemplate
{
    public string Name { get => "Sitting"; }

    // Basic bones and legs setup
    public const int BoneWavesPerFactor = 4;
    public const int LegsVelocityWaves = 0;
    public const int LegsDirectionWaves = 0;

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
            waves.Add(new(genes[0 + i * 3], genes[1 + i * 3], genes[2 + i * 3]));
        }
        return new(constant, waves);
    }

    private const double AccelerationMeanTarget = 0.0015394875252633256;
    private const double AccelerationStdDevTarget = 0.0016441933923222296;
    private const double UpFacingValueMeanTarget = 0.4927963569600095;
    private const double UpFacingValueStdDevTarget = 0.012568482736003236;
    private const double AngleVelocityMeanTarget = 0.032786915354642066;
    private const double AngleVelocityStdDevTarget = 0.05228319519826992;

    public FitnessScore EvaluateSimulationResults(SimulationResults results)
    {
        FitnessScore fitness = new();
        // Encourage non-zero genes
        // fitness.AddWeighedScoreLinear("nonZeroGenes", 2, results.Parameters.NonZeroParameterPortion);
        // Step time should have a large impact on legs and bones movement
        double boneAmplitudePortions = results.Parameters.Bones.Where(x => MovingBones.Contains(x.BoneName)).Sum(x => x.ParametersAmplitudePortion(1));
        fitness.AddWeighedScoreLinear("boneAmplitudePortions", 1, boneAmplitudePortions * 5);
        // The phone should face the simulated user head
        fitness.AddWeighedScoreLinear("averageFacingValue", 6, results.FacingValues.Average());
        // The angle, amount and roll values should not be too high
        fitness.AddWeighedPenaltySqrt("maxAmountValue", 4, Math.Log10(Math.Max(results.MaxAmountValue - 1.2, 1)));
        fitness.AddWeighedPenaltySqrt("maxAngleValue", 4, Math.Log10(Math.Max(results.MaxAngleValue - 2, 1)));
        fitness.AddWeighedPenaltySqrt("maxRollValue", 4, Math.Log10(Math.Max(results.MaxRollValue - 1.2, 1)));

        // Try to get close to standard deviation target
        Descriptive accelerationDesc = new([.. results.PhoneAccelerations]);
        accelerationDesc.Analyze();
        double accelerationMeanTargetHit = Math.Min(accelerationDesc.Result.Mean, AccelerationMeanTarget) / Math.Max(accelerationDesc.Result.Mean, AccelerationMeanTarget);
        double accelerationStdDevTargetHit = Math.Min(accelerationDesc.Result.StdDev, AccelerationStdDevTarget) / Math.Max(accelerationDesc.Result.StdDev, AccelerationStdDevTarget);
        fitness.AddWeighedScoreSqrt("accelerationMeanTargetHit", 4, accelerationMeanTargetHit);
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
