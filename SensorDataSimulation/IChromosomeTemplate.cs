namespace SensorDataSimulation;

public interface IChromosomeTemplate
{
    public int ChromosomeLength { get; }
    public float[] GetInitialGenes();
    public SimulationParameters GenesToParameters(float[] values);
}
