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

        Skeleton skeleton = new(chromosome.GetAsSimulationParameters());

        List<double> facingValues = [];
        List<Vector3> phonePositions = [];
        List<Quaternion> phoneRotations = [];
        List<Vector3> legsOffsets = [];
        List<double> legsDirections = [];

        for (float time = 0f; time < chromosome.Template.SimulationLength; time += chromosome.Template.SimulationTimestep)
        {
            skeleton.Update(time);
            facingValues.Add(GetFacingValuePhoneToEyes(skeleton));
            Bone phone = skeleton.GetBoneByName("Phone")!;
            phonePositions.Add(phone.Location);
            phoneRotations.Add(phone.Rotation);
            legsOffsets.Add(skeleton.LastLegsOffset);
            legsDirections.Add(skeleton.Legs.LastDirection);
        }
        SimulationResults results = new(chromosome.Template.SimulationTimestep, skeleton.Parameters, phonePositions, phoneRotations, legsOffsets, legsDirections, facingValues);
        return chromosome.Template.EvaluateSimulationResults(results);
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