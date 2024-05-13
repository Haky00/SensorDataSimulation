using System.Numerics;

namespace SensorDataSimulation;

// Represents the legs of a skeleton
public class Legs(float length) : ISkeletonPart
{
    public float Length { get; } = length;
    public Vector3 Location { get; set; } = Vector3.Zero;
    public Quaternion Rotation { get; private set; } = Quaternion.Identity;

    public float LastDirection { get; private set; }

    // Updates the Location and Rotation variables based on last Location and passed values
    public void Update(double delta, Vector3 velocity, float direction)
    {
        
        LastDirection = direction;
        Matrix4x4 rotationMatrix = Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, direction);
        Vector3 rotatedVelocity = Vector3.Transform(velocity, rotationMatrix);
        Location += rotatedVelocity * (float)delta;
        Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, direction);
    }
}

