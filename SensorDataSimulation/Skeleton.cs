using System.Numerics;
using System.Reflection.Metadata;

namespace SensorDataSimulation;

public class Skeleton
{
    public readonly Legs Legs;
    public readonly Legs ConstantLegs;
    public readonly List<Bone> Bones;
    public readonly List<Bone> EndAffectors;

    public Vector3 LastLegsOffset { get; private set; }

    private SimulationParameters _parameters;
    public SimulationParameters Parameters
    {
        get => _parameters;
        set
        {
            if (value.Bones.Count() != Bones.Count)
            {
                throw new Exception("Parameters count does not match bones count");
            }
            _parameters = value;
        }
    }

    public Skeleton(SimulationParameters parameters)
    {
        Legs legs = new(1.00f);
        Legs constantLegs = new(1.00f);
        Bone torso = new("Torso", 0.55f, legs, 0, Vector3.Zero, new(15, 25, 28));
        Bone head = new("Head", 0.30f, torso, 0, Vector3.Zero, new(55, 75, 65));
        Bone eyes = new("Eyes", 0.00f, head, 0.50f, new(0, 0, -90), new(0, 0, 0));
        Bone shoulder = new("Shoulder", 0.20f, torso, 0.10f, new(98, 15, 0), new(17, 0, 0));
        Bone upperArm = new("Upper Arm", 0.30f, shoulder, 0, new(15, 10, -35), new(65, 60, 70));
        Bone lowerArm = new("Lower Arm", 0.27f, upperArm, 0, new(0, 0, -70), new(0, 0, 70));
        Bone hand = new("Hand", 0.21f, lowerArm, 0, new(0, 0, -15), new(25, 90, 75));
        Bone phone = new("Phone", 0.00f, hand, 0.50f, new(0, 90, -90), new(0, 0, 0));

        Legs = legs;
        ConstantLegs = constantLegs;
        Bones = [torso, shoulder, upperArm, lowerArm, hand];
        EndAffectors = [head, eyes, phone];

        _parameters = parameters;

        if (Bones.Count != Settings.BonesCount)
        {
            throw new Exception("Bones count in settings is incorrect");
        }
        if (parameters.Bones.Count() != Bones.Count)
        {
            throw new Exception("Parameters count does not match bones count");
        }
    }

    public bool Update(float time)
    {
        float direction = Parameters.Legs.Direction.Compute(time);
        Vector3 legsVelocity = new(Parameters.Legs.VelocityX.Compute(time), Parameters.Legs.VelocityY.Compute(time), Parameters.Legs.VelocityZ.Compute(time));
        Vector3 constantLegsVelocity = new(Parameters.Legs.VelocityX.Constant, Parameters.Legs.VelocityY.Constant, Parameters.Legs.VelocityZ.Constant);
        ConstantLegs.Location = new(Legs.Location.X, Legs.Location.Y, Legs.Location.Z);
        Legs.Update(time, legsVelocity, direction);
        ConstantLegs.Update(time, constantLegsVelocity, direction);
        Vector3 legsOffset = ConstantLegs.Location - Legs.Location;
        Matrix4x4 rotationMatrix = Matrix4x4.CreateFromAxisAngle(new(0, 1, 0), -direction * MathF.PI);
        LastLegsOffset = Vector3.Transform(legsOffset, rotationMatrix);

        for (int i = 0; i < Bones.Count; i++)
        {
            BoneParameters parameters = Parameters.Bones.First(x => x.BoneName == Bones[i].Name);
            float angle = parameters.Angle.Compute(time);
            float amount = parameters.Amount.Compute(time);
            float roll = parameters.Roll.Compute(time);
            Bones[i].Update(roll, angle, amount);
        }

        foreach (Bone endAffector in EndAffectors)
        {
            endAffector.Update(0, 0, 0);
        }

        return true;
    }

    public Bone? GetBoneByName(string name) =>
        Bones.Any(b => b.Name == name) ? Bones.First(b => b.Name == name) :
        EndAffectors.Any(b => b.Name == name) ? EndAffectors.First(b => b.Name == name) :
        null;
}
