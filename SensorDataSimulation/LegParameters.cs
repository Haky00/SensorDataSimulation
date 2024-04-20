using Newtonsoft.Json;

namespace SensorDataSimulation;

public class LegParameters(SimulationFactor velocityX, SimulationFactor velocityY, SimulationFactor velocityZ, SimulationFactor direction)
{
     public readonly SimulationFactor VelocityX = velocityX;
     public readonly SimulationFactor VelocityY = velocityY;
     public readonly SimulationFactor VelocityZ = velocityZ;
     public readonly SimulationFactor Direction = direction;

     public float VelocityParametersAmplitudePortion(int n)
     {
          return (VelocityX.ParametersAmplitudePortion(n) + VelocityY.ParametersAmplitudePortion(n) + VelocityZ.ParametersAmplitudePortion(n)) / 3f;
     }

     [JsonIgnore]
     public float NonZeroParameterPortion
     {
          get => (VelocityX.NonZeroParameterPortion + VelocityY.NonZeroParameterPortion + VelocityZ.NonZeroParameterPortion + Direction.NonZeroParameterPortion) / 4f;
     }
}
