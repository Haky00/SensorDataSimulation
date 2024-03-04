﻿using System.Numerics;

namespace SensorDataSimulation;

public class Skeleton
{
    public readonly List<Bone> Bones;
    public readonly List<Bone> EndAffectors;

    private IList<BoneParameters> _parameters;
    public IList<BoneParameters> Parameters
    {
        get => _parameters;
        set
        {
            if (value.Count != Bones.Count)
            {
                throw new Exception("Parameters count does not match bones count");
            }
            _parameters = value;
        }
    }

    public Skeleton(IList<BoneParameters> parameters)
    {
        Bone legs = new("Legs", 1.00f, null, 0, Vector3.Zero, Vector3.Zero);
        Bone torso = new("Torso", 0.55f, legs, 0, Vector3.Zero, new(15, 55, 28));
        Bone head = new("Head", 0.30f, torso, 0, Vector3.Zero, new(55, 75, 65));
        Bone eyes = new("Eyes", 0.00f, head, 0.50f, new(0, 0, -90), new(0, 0, 0));
        Bone shoulder = new("Shoulder", 0.20f, torso, 0.10f, new(98, 15, 0), new(17, 0, 0));
        Bone upperArm = new("Upper Arm", 0.30f, shoulder, 0, new(15, 10, -35), new(65, 60, 70));
        Bone lowerArm = new("Lower Arm", 0.27f, upperArm, 0, new(0, 0, -70), new(0, 0, 70));
        Bone hand = new("Hand", 0.21f, lowerArm, 0, new(0, 0, -15), new(25, 90, 75));
        Bone phone = new("Phone", 0.00f, hand, 0.50f, new(0, 90, -90), new(0, 0, 0));

        Bones = [legs, torso, head, shoulder, upperArm, lowerArm, hand];
        EndAffectors = [eyes, phone];

        _parameters = parameters;

        if (Bones.Count != Settings.BonesCount)
        {
            throw new Exception("Bones count in settings is incorrect");
        }
        if (parameters.Count != Bones.Count)
        {
            throw new Exception("Parameters count does not match bones count");
        }
    }

    public bool Update(float time)
    {
        for (int i = 0; i < Bones.Count; i++)
        {
            BoneParameters parameters = Parameters[i];
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
