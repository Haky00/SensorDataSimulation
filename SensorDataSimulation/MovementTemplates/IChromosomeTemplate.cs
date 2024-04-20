namespace SensorDataSimulation.MovementTemplates;

public interface IChromosomeTemplate
{
    public string Name { get; }
    public int ChromosomeLength { get; }
    public float SimulationLength { get; }
    public float SimulationTimestep { get; }
    public float[] GetInitialGenes();
    public SimulationParameters GenesToParameters(float[] values);
    public FitnessScore EvaluateSimulationResults(SimulationResults results);
}
