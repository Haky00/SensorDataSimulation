namespace SensorDataSimulation;

public readonly struct BoneParameters(string boneName, SimulationFactor angle, SimulationFactor amount, SimulationFactor roll)
{
    public readonly string BoneName = boneName;
    public readonly SimulationFactor Angle = angle;
    public readonly SimulationFactor Amount = amount;
    public readonly SimulationFactor Roll = roll;
}
