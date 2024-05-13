using System.Numerics;

namespace SensorDataSimulation;

// Represents a bone of a skeleton
public class Bone(string name, float length, ISkeletonPart? parent, float attachedOffset, Vector3 attachedRotationEuler, Vector3 maxAngles) : ISkeletonPart
{
    public readonly string Name = name;
    public float Length { get; } = length;
    // The parent bone (or legs) this bone is attached to
    public readonly ISkeletonPart? Parent = parent;
    // Offset from the end of the parent bone, range between 0 (at the end) to 1 (at the start)
    public readonly float AttachedOffset = attachedOffset;
    // Max angles for each of the 3 euler rotation directions (X and Z are horizontal rotations and Y is roll)
    public readonly Vector3 MaxAngles = maxAngles * MathF.PI / 180.0f;
    // Additional rotation from the parent (Y = roll)
    public readonly Quaternion AttachedRotation = Quaternion.CreateFromYawPitchRoll(attachedRotationEuler.Y * MathF.PI / 180f, attachedRotationEuler.X * MathF.PI / 180f, attachedRotationEuler.Z * MathF.PI / 180f);
    public Vector3 Location { get; private set; } = Vector3.Zero;
    public Quaternion Rotation { get; private set; } = Quaternion.Identity;

    public float LastRoll { get; private set; }
    public float LastAngle { get; private set; }
    public float LastAmount { get; private set; }

    // Updates the Location and Rotation variables based on the state of parent and passed values
    public void Update(float roll, float angle, float amount)
    {
        LastRoll = roll;
        LastAngle = angle;
        LastAmount = amount;

        if (Parent is not null)
        {
            Rotation = Parent.Rotation;
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromQuaternion(Parent.Rotation);
            Vector3 parentOffset = new(0, Parent.Length * (1 - AttachedOffset), 0);
            Vector3 rotatedParentOffset = Vector3.Transform(parentOffset, rotationMatrix);
            Location = Parent.Location + rotatedParentOffset;
        }
        else
        {
            Location = Vector3.Zero;
            Rotation = Quaternion.Identity;
        }

        if (roll != 0 || amount != 0)
        {
            float yRotation = Sigmoid(roll) * MaxAngles.Y;
            float xRotation = MathF.Cos(angle) * Sigmoid(amount) * MaxAngles.X;
            float zRotation = MathF.Sin(angle) * Sigmoid(amount) * MaxAngles.Z;
            Quaternion yawPitchRotation = Quaternion.CreateFromYawPitchRoll(0, xRotation, zRotation);
            Quaternion rollRotation = Quaternion.CreateFromYawPitchRoll(yRotation, 0, 0);
            Rotation *= AttachedRotation * yawPitchRotation * rollRotation;
        }
        else
        {
            Rotation *= AttachedRotation;
        }
    }

    private static float Sigmoid(float value)
    {
        float k = MathF.Exp(value);
        return (k / (1.0f + k) - 0.5f) * 2f;
    }
}
