using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace SensorDataSimulation;

public class SimulationResults
{
    public readonly SimulationParameters Parameters;
    public readonly List<Vector3> PhonePositions;
    public readonly List<float> PhoneVelocities;
    public readonly List<float> PhoneAccelerations;
    public readonly List<Quaternion> PhoneRotations;
    public readonly List<float> PhoneUpFacingValues;
    public readonly List<float> PhoneAngleVelocities;
    public readonly List<Vector3> LegsPositionOffsets;
    public readonly List<double> LegsDirections;
    public readonly List<double> LegsDirectionVelocities;
    public readonly List<double> FacingValues;
    public readonly double MaxAmountValue;
    public readonly double MaxRollValue;
    public readonly double MaxAngleValue;

    public SimulationResults(float simulationStep, SimulationParameters parameters, List<Vector3> phonePositions, List<Quaternion> phoneRotations, List<Vector3> legsPositionOffsets, List<double> legsDirections, List<double> facingValues)
    {
        Parameters = parameters;
        PhonePositions = phonePositions;
        PhoneRotations = phoneRotations;
        LegsPositionOffsets = legsPositionOffsets;
        LegsDirections = legsDirections;
        FacingValues = facingValues;

        LegsDirectionVelocities = LegsDirections.Skip(1).Zip(LegsDirections, (a, b) => Math.Abs(a - b) / simulationStep).ToList();
        PhoneVelocities = PhonePositions.Skip(1).Zip(PhonePositions, (a, b) => (a - b).Length() / simulationStep).ToList();
        PhoneAccelerations = PhoneVelocities.Skip(1).Zip(PhoneVelocities, (a, b) => MathF.Abs(a - b)).ToList();
        PhoneUpFacingValues = PhoneRotations.Select(RotationToUpFacingValue).ToList();
        PhoneAngleVelocities = PhoneRotations.Skip(1).Zip(PhoneRotations, (a, b) => RotationsToAngleVelocity(a, b, simulationStep)).ToList();

        foreach (var boneParameters in parameters.Bones)
        {
            MaxAmountValue = Math.Max(MaxAmountValue, boneParameters.Amount.TheoreticalMaximum);
            MaxRollValue = Math.Max(MaxRollValue, boneParameters.Roll.TheoreticalMaximum);
            MaxAngleValue = Math.Max(MaxAngleValue, boneParameters.Angle.TheoreticalMaximum);
        }
    }

    private static float RotationToUpFacingValue(Quaternion rotation)
    {
        Vector3 forward = Vector3.Transform(Vector3.UnitZ, rotation);
		float dotProduct = Vector3.Dot(Vector3.UnitY, forward);
		float angle = MathF.Acos(Math.Clamp(dotProduct, -1, 1));
		angle = MathF.Min(angle, MathF.PI);
		return 1 - angle / MathF.PI;
    }
    private static float RotationsToAngleVelocity(Quaternion q1, Quaternion q2, float simulationStep)
    {
        Quaternion deltaRotation = Quaternion.Inverse(q1) * q2;
        float angle = 2 * (float)MathF.Acos(MathF.Min(1, MathF.Abs(deltaRotation.W)));
        return angle / simulationStep;
    }
}
