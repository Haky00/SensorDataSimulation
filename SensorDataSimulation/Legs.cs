using System.Numerics;

namespace SensorDataSimulation;

public class Legs(float length) : ISkeletonPart
{
    public float Length { get; } = length;
    public Vector3 Location { get; set; } = Vector3.Zero;
    public Quaternion Rotation { get; private set; } = Quaternion.Identity;

    static readonly Vector3 directionAxis = new(0, 1, 0);

    public void Update(double delta, Vector3 velocity, float direction)
    {
        Matrix4x4 rotationMatrix = Matrix4x4.CreateFromAxisAngle(directionAxis, direction);
        Vector3 rotatedVelocity = Vector3.Transform(velocity, rotationMatrix);
        Location += rotatedVelocity * (float)delta;
        Rotation = Quaternion.CreateFromAxisAngle(directionAxis, direction);
    }
}

