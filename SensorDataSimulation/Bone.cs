using System.Numerics;

namespace SensorDataSimulation;

public class Bone(string name, float length, ISkeletonPart? parent, float attachedOffset, Vector3 attachedRotationEuler, Vector3 maxAngles) : ISkeletonPart
{
    public readonly string Name = name;
    public float Length { get; } = length;
    public readonly ISkeletonPart? Parent = parent;
    public readonly float AttachedOffset = attachedOffset;
    public readonly Vector3 MaxAngles = maxAngles * MathF.PI / 180.0f;
    public readonly Quaternion AttachedRotation = Quaternion.CreateFromYawPitchRoll(attachedRotationEuler.Y * MathF.PI / 180f, attachedRotationEuler.X * MathF.PI / 180f, attachedRotationEuler.Z * MathF.PI / 180f);
    public Vector3 Location { get; private set; } = Vector3.Zero;
    public Quaternion Rotation { get; private set; } = Quaternion.Identity;

    public float LastRoll { get; private set; }
    public float LastAngle { get; private set; }
    public float LastAmount { get; private set; }

    public void Update(float roll, float angle, float amount)
    {
        LastRoll = roll;
        LastAngle = angle;
        LastAmount = amount;

        if (Parent is not null)
        {
            Location = Parent.Location;
            Rotation = Parent.Rotation;
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromQuaternion(Parent.Rotation);
            Vector3 rotatedParentOffset = Vector3.Transform(new Vector3(0, Parent.Length * (1 - AttachedOffset), 0), rotationMatrix);
            Location += new Vector3(rotatedParentOffset.X, rotatedParentOffset.Y, rotatedParentOffset.Z);
        }
        else
        {
            Location = Vector3.Zero;
            Rotation = Quaternion.Identity;
        }

        if (roll != 0 || amount != 0)
        {
            float yRotation = Sigmoid(roll) * MaxAngles.Y;
            float angleRadians = angle;
            float xRotation = MathF.Cos(angleRadians) * Sigmoid(amount) * MaxAngles.X;
            float zRotation = MathF.Sin(angleRadians) * Sigmoid(amount) * MaxAngles.Z;
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
