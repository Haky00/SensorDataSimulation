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
            float newValue = value + ((float)random.NextDouble() - 0.5f) * 0.4f;
            chromosome.ReplaceGene(i, new Gene(newValue));
        }
    }
}
