namespace SensorDataSimulation.MovementTemplates;

// Template for a stationary device facing up on a table, with the legs facing in a given direction
public class OnTableTemplate(float direction, float upValueTarget) : IMovementTemplate
{
    public string Name { get => "OnTable"; }

    // Basic bones and legs setup
    public const int BoneWavesPerFactor = 0;
    public const int LegsVelocityWaves = 0;
    public const int LegsDirectionWaves = 0;

    // No waves will have set frequency
    private const int WavesWithSetFrequencyPerFactor = 0;

    // Constant X, Y and Z are set to 0
    private const int SetConstantsPerVelocityDirection = 1;
    // No waves will have set frequency
    private const int SetFrequenciesPerVelocityDirection = 0;
    // No waves will have set frequency
    private const int DirectionSetFrequencies = 0;

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
    private const int LegsSetGenesCount = 3 * SetConstantsPerVelocityDirection + 3 * SetFrequenciesPerVelocityDirection + DirectionSetFrequencies;

    // Total number of genes that can be ommited because they will be constant
    private const int SetGenesCount = BonesSetGenesCount + LegsSetGenesCount;

    public int ChromosomeLength { get; } = MaxChromosomeLength - SetGenesCount;

    // Only 1 simulation sample needed
    public float SimulationLength { get; } = 1f;

    // Only 1 simulation sample needed
    public float SimulationTimestep { get; } = 2f;

    public float[] GetInitialGenes()
    {
        float[] genes = new float[ChromosomeLength];
        Array.Clear(genes);
        return genes;
    }

    public FitnessScore EvaluateSimulationResults(SimulationResults results)
    {
        FitnessScore fitness = new();
        // The angle, amount and roll values should not be too high
        fitness.AddWeighedPenaltySqrt("maxAmountValue", 2, Math.Log10(Math.Max(results.MaxAmountValue - 2, 1)));
        fitness.AddWeighedPenaltySqrt("maxAngleValue", 2, Math.Log10(Math.Max(results.MaxAngleValue - 4, 1)));
        fitness.AddWeighedPenaltySqrt("maxRollValue", 2, Math.Log10(Math.Max(results.MaxRollValue - 2, 1)));
        float upValue = results.PhoneUpFacingValues[0];
        // The up direction should be the same as the target
        double upDirectionHit = Math.Min(upValue, upValueTarget) / Math.Max(upValue, upValueTarget);
        fitness.AddWeighedScoreLinear("upValueMeanTargetHit", 6, upDirectionHit);
        return fitness;
    }

    // Converts passed genes to simulation parameters
    public SimulationParameters GenesToParameters(float[] values)
    {
        LegParameters legParameters = new(new(0, []), new(0, []), new(0, []), new(direction, []));
        List<BoneParameters> boneParameters = [];
        for (int i = 0; i < Settings.BoneNames.Length; i++)
        {
            boneParameters.Add(new(Settings.BoneNames[i], new(values[i * 3], []), new(values[i * 3 + 1], []), new(values[i * 3 + 2], [])));
        }
        return new(legParameters, boneParameters);
    }
}
