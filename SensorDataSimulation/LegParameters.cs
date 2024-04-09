namespace SensorDataSimulation;

public class LegParameters(SimulationFactor velocityX, SimulationFactor velocityY, SimulationFactor velocityZ, SimulationFactor direction)
{
     public readonly SimulationFactor VelocityX = velocityX;
     public readonly SimulationFactor VelocityY = velocityY;
     public readonly SimulationFactor VelocityZ = velocityZ;
     public readonly SimulationFactor Direction = direction;
}
