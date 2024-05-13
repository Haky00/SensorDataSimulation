namespace SensorDataSimulation.MovementTemplates;

// Interface for movement templates to interact with the genetic algorithm
public interface IMovementTemplate
{
    // Base name for the template (for example "Walking", "Sitting", ...)
    public string Name { get; }
    // Number of genes for this template
    public int ChromosomeLength { get; }
    // How many seconds should be simulated
    public float SimulationLength { get; }
    // What should be the time between two simulation steps
    public float SimulationTimestep { get; }
    // Get an initial value for the genes
    public float[] GetInitialGenes();
    // Converts a set of genes to a SimulationParameters object
    public SimulationParameters GenesToParameters(float[] genes);
    // Objective function that returns a fitness score for the passed results
    public FitnessScore EvaluateSimulationResults(SimulationResults results);
}
