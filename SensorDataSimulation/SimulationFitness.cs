using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Numerics;
using GeneticSharp;

namespace SensorDataSimulation;

public class SimulationFitness : IFitness
{
    public double Evaluate(IChromosome c)
    {
        if (c is not SimulationChromosome chromosome)
        {
            throw new Exception("Wrong chromosome");
        }
        Skeleton skeleton = new(chromosome.GetAsBoneParameters());

        double fitness = 0;
        double facingValue;
        List<double> velocities = [];
        Vector3 lastPhonePosition = Vector3.Zero;
        float timestep = 0.016f;
        for (float time = 0f; time < 10f; time += timestep)
        {
            skeleton.Update(time);
            facingValue = GetFacingValuePhoneToEyes(skeleton);
            fitness += facingValue * facingValue * timestep;
            Vector3 phonePosition = skeleton.GetBoneByName("Phone")!.Location;
            if (time > 0f && velocities.Count < 512)
            {
                velocities.Add(Vector3.Distance(phonePosition, lastPhonePosition));
            }
            lastPhonePosition = phonePosition;
        }
        for (float time = 50f; time < 55f; time += 0.1f)
        {
            skeleton.Update(time);
            facingValue = GetFacingValuePhoneToEyes(skeleton);
            fitness += facingValue * facingValue * 0.1;
        }
        for (float time = 2000f; time < 2500f; time += 5f)
        {
            skeleton.Update(time);
            facingValue = GetFacingValuePhoneToEyes(skeleton);
            fitness += facingValue * facingValue * 5;
        }

        Complex[] forward = FftSharp.FFT.Forward(velocities.ToArray());
        double[] spectrum = FftSharp.FFT.Magnitude(forward);
        if (spectrum.Sum() != 0)
        {
            List<(double Frequency, double Power)> waves = [];
            for (int i = 0; i < spectrum.Length; i++)
            {
                waves.Add((i * timestep, spectrum[i]));
            }
            double sumMainFrequency = waves.Where(x => x.Frequency > 0.95 && x.Frequency < 1.05).Sum(x => x.Power);
            double totalSumFrequency = waves.Sum(x => x.Power);
            fitness *= sumMainFrequency / totalSumFrequency;
            if (totalSumFrequency < 1)
            {
                fitness *= totalSumFrequency;
            }
            return fitness;
        }
        return 0;
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
        return facingValue;
    }
}
