using System.Numerics;

namespace SensorDataSimulation;

public interface ISkeletonPart
{
    public float Length { get; }

    public Vector3 Location { get; }
    public Quaternion Rotation { get; }
}
