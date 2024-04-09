using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Numerics;
using GeneticSharp;

namespace SensorDataSimulation;

public class SimulationFitness : IFitness
{
    public double Evaluate(IChromosome c) => EvaluateChromosome(c).Score;

    private static double FirstSineAmplitudePortion(SimulationFactor factor) {
        double total = factor.SineParameters.Sum(x => Math.Abs(x.Amplitude));
        if (total == 0)
        {
            return 0;
        }
        return Math.Abs(factor.SineParameters.First().Amplitude) / total;
    }

    public static FitnessScore EvaluateChromosome(IChromosome c)
    {
        if (c is not SimulationChromosome chromosome)
        {
            throw new Exception("Wrong chromosome");
        }
        FitnessScore fitness = new();
        Skeleton skeleton = new(chromosome.GetAsSimulationParameters());
        double boneAmplitudePortions = 0;
        double bonePortionsMax = 0;
        foreach (BoneParameters boneParameters in skeleton.Parameters.Bones)
        {
            boneAmplitudePortions += FirstSineAmplitudePortion(boneParameters.Angle);
            boneAmplitudePortions += FirstSineAmplitudePortion(boneParameters.Amount);
            boneAmplitudePortions += FirstSineAmplitudePortion(boneParameters.Roll);
            bonePortionsMax += 3;
        }
        boneAmplitudePortions /= bonePortionsMax; 
        if (boneAmplitudePortions > 0.2)
        {
            fitness.AddScore("boneAmplitudePortions", 1);
        }
        else
        {
            fitness.AddWeighedScoreLinear("boneAmplitudePortions", 1, boneAmplitudePortions * 5);
        }
        double walkingAmplitudePortions = 0;
        walkingAmplitudePortions += FirstSineAmplitudePortion(skeleton.Parameters.Legs.VelocityX);
        walkingAmplitudePortions += FirstSineAmplitudePortion(skeleton.Parameters.Legs.VelocityY);
        walkingAmplitudePortions += FirstSineAmplitudePortion(skeleton.Parameters.Legs.VelocityZ);
        if (walkingAmplitudePortions > 0.5)
        {
            fitness.AddScore("walkingAmplitudePortions", 1);
        }
        else
        {
            fitness.AddWeighedScoreLinear("walkingAmplitudePortions", 1, walkingAmplitudePortions * 2);
        }
        double facingValue = 0;
        double maxAmountValue = 0;
        double maxRollValue = 0;
        float simulatedTime = 0;
        List<double> velocities = [];
        List<double> legsDirections = [];
        List<double> legsOffsets = [];
        Vector3? lastPhonePosition = null;
        // Fine-grained simulation (20s/16ms)
        float fineTimestep = 0.016f;
        for (float time = 0f; time < 20f; time += fineTimestep)
        {
            skeleton.Update(time);
            facingValue += GetFacingValuePhoneToEyes(skeleton) * fineTimestep;
            Vector3 phonePosition = skeleton.GetBoneByName("Phone")!.Location;
            if (lastPhonePosition is not null && velocities.Count < 1024)
            {
                velocities.Add(Vector3.Distance(phonePosition, lastPhonePosition.Value));
            }
            lastPhonePosition = phonePosition;
            maxAmountValue = Math.Max(maxAmountValue, skeleton.Bones.Max(x => Math.Abs(x.LastAmount)));
            maxRollValue = Math.Max(maxRollValue, skeleton.Bones.Max(x => Math.Abs(x.LastRoll)));
            simulatedTime += fineTimestep;
            Vector3 scaledLegsOffset = skeleton.LastLegsOffset * new Vector3(1f / 3f, 1f, 1f);
            legsOffsets.Add(scaledLegsOffset.Length());
            legsDirections.Add(skeleton.Parameters.Legs.Direction.Compute(time));
        }
        double averageFacingValue = facingValue / simulatedTime;
        double maxLegsDirection = legsDirections.Max(Math.Abs);
        double maxLegsOffsetLength = legsOffsets.Max();
        List<double> legsAngleVelocities = [];
        for (int i = 1; i < legsDirections.Count; i++)
        {
            legsAngleVelocities.Add(Math.Abs(legsDirections[i - 1] - legsDirections[i]));
        }
        Descriptive legsAngleVelDesc = new([.. legsAngleVelocities]);
        legsAngleVelDesc.Analyze();
        Descriptive legsOffsetsDesc = new([.. legsOffsets]);
        legsOffsetsDesc.Analyze();
        fitness.AddWeighedScoreLinear("averageFacingValue", 5, averageFacingValue);
        if (maxAmountValue <= 0.5)
        {
            fitness.AddScore("maxAmountValue", 4);
        }
        else
        {
            fitness.AddWeighedScoreSqrt("maxAmountValue", 4, 1 - Math.Min(1, Math.Log10(maxAmountValue + 0.5)));
        }
        if (maxRollValue <= 0.5)
        {
            fitness.AddScore("maxRollValue", 4);
        }
        else
        {
            fitness.AddWeighedScoreSqrt("maxRollValue", 4, 1 - Math.Min(1, Math.Log10(maxRollValue + 0.5)));
        }
        if (maxLegsOffsetLength > 0.45)
        {
            fitness.AddWeighedPenaltySqrt("maxLegsOffsetLength", 50, Math.Min(maxLegsOffsetLength - 0.45, 1));
        }
        if (legsOffsetsDesc.Result.Mean < 0.05)
        {
            fitness.AddWeighedPenaltySqrt("legsOffsetsMeanLow", 50, Math.Min((0.05 - legsOffsetsDesc.Result.Mean) * (1 / 0.05), 1));
        }
        if (maxLegsDirection > 2)
        {
            fitness.AddWeighedPenaltySqrt("maxLegsDirection", 50, Math.Min(maxLegsDirection - 2, 1));
        }
        if (legsAngleVelDesc.Result.Mean > 0.01)
        {
            fitness.AddWeighedPenaltySqrt("legsAngleVelocityMeanHigh", 50, Math.Min((legsAngleVelDesc.Result.Mean - 0.01) * 50, 1));
        }
        if (legsAngleVelDesc.Result.Mean < 0.001)
        {
            fitness.AddWeighedPenaltyLinear("legsAngleVelocityMeanLow", 50, Math.Min((0.001 - legsAngleVelDesc.Result.Mean) * (1 / 0.001), 1));
        }

        var window = new FftSharp.Windows.Hanning();
        double[] windowed = window.Apply([.. velocities]);
        Complex[] forward = FftSharp.FFT.Forward(windowed);
        double[] spectrum = FftSharp.FFT.Magnitude(forward);
        if (spectrum.Sum() == 0)
        {
            return new FitnessScore();
        }

        List<(double Frequency, double Power)> waves = [];
        for (int i = 0; i < spectrum.Length; i++)
        {
            waves.Add((i * fineTimestep, spectrum[i]));
        }
        double sumMainFrequency = waves.Where(x => x.Frequency > 0.95 && x.Frequency < 1.05).Sum(x => x.Power);
        double totalSumFrequency = waves.Sum(x => x.Power);
        // fitness.AddWeighedScoreSqrt("1hz frequency", 5, sumMainFrequency / totalSumFrequency);
        //fitness.AddWeighedScoreSqrt(1, totalSumFrequency > 1 ? 1 : totalSumFrequency);

        double meanTarget = double.Parse("1,6714288813732015E-06");
        double stdDevTarget = double.Parse("0,004865945474532672");

        List<double> accelerations = new();
        for (int i = 1; i < velocities.Count; i++)
        {
            accelerations.Add(velocities[i] - velocities[i - 1]);
        }
        Descriptive d = new([.. accelerations]);
        d.Analyze();
        double meanTargetHit = d.Result.Mean > meanTarget ? meanTarget / d.Result.Mean : d.Result.Mean / meanTarget;
        double stdDevTargetHit = d.Result.StdDev > stdDevTarget ? stdDevTarget / d.Result.StdDev : d.Result.StdDev / stdDevTarget;
        fitness.AddWeighedScoreSqrt("stdDevTargetHit", 2, stdDevTargetHit);
        //fitness.AddWeighedScoreSqrt("meanTargetHit", 15, meanTargetHit);

        float[] genes = chromosome.GetGenes().Select(gene => (float)gene.Value).ToArray();
        int nonZeroGenes = genes.Count(x => x != 0);
        fitness.AddWeighedScoreLinear("nonZeroGenes", 2, (double)nonZeroGenes / genes.Length);

        return fitness;
    }

    private static double GetFacingValuePhoneToEyes(Skeleton skeleton)
    {
        Bone? phone = skeleton.GetBoneByName("Phone");
        Bone? eyes = skeleton.GetBoneByName("Eyes");
        if (phone is null || eyes is null)
        {
            return 0;
        }
        //float distance = Vector3.Distance(phone.Location, eyes.Location);
        Vector3 directionToEyes = Vector3.Normalize(eyes.Location - phone.Location);
        Vector3 phoneForward = Vector3.Transform(Vector3.UnitZ, phone.Rotation);
        float facingValue = Vector3.Dot(directionToEyes, phoneForward);
        return facingValue * facingValue;
    }
}

// using System.ComponentModel.DataAnnotations;
// using System.Diagnostics;
// using System.Numerics;
// using GeneticSharp;

// namespace SensorDataSimulation;

// public class SimulationFitness : IFitness
// {
//     public double Evaluate(IChromosome c)
//     {
//         if (c is not SimulationChromosome chromosome)
//         {
//             throw new Exception("Wrong chromosome");
//         }
//         Skeleton skeleton = new(chromosome.GetAsBoneParameters());

//         double fitness = 0;
//         double facingValue;
//         List<double> velocities = [];
//         Vector3 lastPhonePosition = Vector3.Zero;
//         float timestep = 0.016f;
//         for (float time = 0f; time < 10f; time += timestep)
//         {
//             skeleton.Update(time);
//             facingValue = GetFacingValuePhoneToEyes(skeleton);
//             fitness += facingValue * facingValue * timestep;
//             Vector3 phonePosition = skeleton.GetBoneByName("Phone")!.Location;
//             if (time > 0f && velocities.Count < 512)
//             {
//                 velocities.Add(Vector3.Distance(phonePosition, lastPhonePosition));
//             }
//             lastPhonePosition = phonePosition;
//         }
//         for (float time = 50f; time < 55f; time += 0.1f)
//         {
//             skeleton.Update(time);
//             facingValue = GetFacingValuePhoneToEyes(skeleton);
//             fitness += facingValue * facingValue * 0.1;
//         }
//         for (float time = 2000f; time < 2500f; time += 5f)
//         {
//             skeleton.Update(time);
//             facingValue = GetFacingValuePhoneToEyes(skeleton);
//             fitness += facingValue * facingValue * 5;
//         }

//         Complex[] forward = FftSharp.FFT.Forward(velocities.ToArray());
//         double[] spectrum = FftSharp.FFT.Magnitude(forward);
//         if (spectrum.Sum() != 0)
//         {
//             List<(double Frequency, double Power)> waves = [];
//             for (int i = 0; i < spectrum.Length; i++)
//             {
//                 waves.Add((i * timestep, spectrum[i]));
//             }
//             double sumMainFrequency = waves.Where(x => x.Frequency > 0.95 && x.Frequency < 1.05).Sum(x => x.Power);
//             double totalSumFrequency = waves.Sum(x => x.Power);
//             fitness *= sumMainFrequency / totalSumFrequency;
//             if (totalSumFrequency < 1)
//             {
//                 fitness *= totalSumFrequency;
//             }
//             return fitness;
//         }
//         return 0;
//     }

//     private static double GetFacingValuePhoneToEyes(Skeleton skeleton)
//     {
//         Bone? phone = skeleton.GetBoneByName("Phone");
//         Bone? eyes = skeleton.GetBoneByName("Eyes");
//         if (phone is null || eyes is null)
//         {
//             return 0;
//         }
//         //float distance = Vector3.Distance(phone.Location, eyes.Location);
//         Vector3 directionToEyes = Vector3.Normalize(eyes.Location - phone.Location);
//         Vector3 phoneForward = Vector3.Transform(Vector3.UnitZ, phone.Rotation);
//         float facingValue = Vector3.Dot(directionToEyes, phoneForward);
//         return facingValue;
//     }
// }
