using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Numerics;
using GeneticSharp;

namespace SensorDataSimulation;

// The custom fitness class, it performs a simulation and passes the results to the template object
public class SimulationFitness : IFitness
{
    public double Evaluate(IChromosome c) => EvaluateChromosome(c).Score;

    public static FitnessScore EvaluateChromosome(IChromosome c)
    {
        if (c is not SimulationChromosome chromosome)
        {
            throw new Exception("Wrong chromosome");
        }

        Skeleton skeleton = new(chromosome.GetAsSimulationParameters());
        // Perform simulation and gather base results
        List<double> facingValues = [];
        List<Vector3> phonePositions = [];
        List<Quaternion> phoneRotations = [];
        List<double> legsDirections = [];

        for (float time = 10f; time < 10f + chromosome.Template.SimulationLength; time += chromosome.Template.SimulationTimestep)
        {
            skeleton.Update(time);
            facingValues.Add(GetFacingValuePhoneToEyes(skeleton));
            Bone phone = skeleton.GetBoneByName("Phone")!;
            phonePositions.Add(phone.Location);
            phoneRotations.Add(phone.Rotation);
            legsDirections.Add(skeleton.Legs.LastDirection);
        }
        // Create a results object to compute additional values
        SimulationResults results = new(chromosome.Template.SimulationTimestep, skeleton.Parameters, phonePositions, phoneRotations, legsDirections, facingValues);
        // Return the fitness value of the template
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