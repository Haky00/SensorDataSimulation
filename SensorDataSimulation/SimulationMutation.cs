using GeneticSharp;

namespace SensorDataSimulation;

public class SimulationMutation : MutationBase
{
    protected override void PerformMutate(IChromosome c, float probability)
    {
        Random random = new();
        if (c is not SimulationChromosome chromosome)
        {
            throw new Exception("Wrong chromosome");
        }
        for(int i = 0; i < chromosome.Length; i++)
        {
            if (random.NextDouble() >= probability)
            {
                continue;
            }
            Gene gene = chromosome.GetGene(i);
            if (gene.Value is not float value)
            {
                throw new Exception("Wrong gene type");
            }
            //float newValue = value + ((float)random.NextDouble() - 0.5f) * 0.4f;
            float newValue = value + (float)GetNormallyDistributedRandom(random, 0, 2);
            chromosome.ReplaceGene(i, new Gene(newValue));
        }
    }

    private static double GetNormallyDistributedRandom(Random rng, double mean = 0, double variance = 1)
    {
        double r = Math.Sqrt(-2 * Math.Log(rng.NextDouble()));
        double θ = 2 * Math.PI * rng.NextDouble();
        double x = r * Math.Cos(θ);
        x *= variance;
        x += mean;
        return x;
    }
}
