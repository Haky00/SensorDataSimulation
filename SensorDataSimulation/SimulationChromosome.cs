using GeneticSharp;
using SensorDataSimulation.MovementTemplates;

namespace SensorDataSimulation;

// The custom chromosome class, all it does is just call or get what it needs in the passed template
public class SimulationChromosome : ChromosomeBase
{
	public IMovementTemplate Template { get; }

	public SimulationChromosome(IMovementTemplate template) : base(template.ChromosomeLength)
	{
		Template = template;
		CreateGenes();
	}

	public override Gene GenerateGene(int geneIndex)
	{
		return new Gene(Template.GetInitialGenes()[geneIndex]);
	}

	public override IChromosome CreateNew()
	{
		return new SimulationChromosome(Template);
	}

	public SimulationParameters GetAsSimulationParameters()
	{
		return Template.GenesToParameters(GetGenes().Select(gene => (float)gene.Value).ToArray());
	}
}