using System.Numerics;

namespace SensorDataSimulation;

// Common interface for bons and legs
public interface ISkeletonPart
{
    public float Length { get; }

    public Vector3 Location { get; }
    public Quaternion Rotation { get; }
}
