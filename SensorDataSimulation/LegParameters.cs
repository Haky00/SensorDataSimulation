using System.Numerics;
using Newtonsoft.Json;

namespace SensorDataSimulation;

// Represents simulation parameters for legs
public class LegParameters(SimulationFactor velocityX, SimulationFactor velocityY, SimulationFactor velocityZ, SimulationFactor direction)
{
     [JsonProperty("velocityX")]
     public readonly SimulationFactor VelocityX = velocityX;

     [JsonProperty("velocityY")]
     public readonly SimulationFactor VelocityY = velocityY;

     [JsonProperty("velocityZ")]
     public readonly SimulationFactor VelocityZ = velocityZ;

     [JsonProperty("direction")]
     public readonly SimulationFactor Direction = direction;

     // Returns how big the portion of the amplitudes of the n first sines in velocities is when compared to all amplitudes
     public float VelocityParametersAmplitudePortion(int n)
     {
          return (VelocityX.ParametersAmplitudePortion(n) + VelocityY.ParametersAmplitudePortion(n) + VelocityZ.ParametersAmplitudePortion(n)) / 3f;
     }

     // Returns the portion of child parameters that are not zero
     [JsonIgnore]
     public float NonZeroParameterPortion
     {
          get => (VelocityX.NonZeroParameterPortion + VelocityY.NonZeroParameterPortion + VelocityZ.NonZeroParameterPortion + Direction.NonZeroParameterPortion) / 4f;
     }

     // Returns the sum of maximum possible values of all sines for velocities
     [JsonIgnore]
     public Vector3 TheoreticalMaximumVelocityWithoutConstant
     {
          get => new(
               VelocityX.TheoreticalMaximum - Math.Abs(VelocityX.Constant), 
               VelocityY.TheoreticalMaximum - Math.Abs(VelocityY.Constant), 
               VelocityZ.TheoreticalMaximum - Math.Abs(VelocityZ.Constant));
     }
}
