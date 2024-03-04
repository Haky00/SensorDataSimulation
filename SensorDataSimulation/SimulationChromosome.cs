﻿using GeneticSharp;

namespace SensorDataSimulation;

public class SimulationChromosome : ChromosomeBase
{
	private enum FactorType { Angle, Amount, Roll }
	private enum ValueType { Constant, Amplitude, Phase, Frequency }

	public SimulationChromosome() : base(Settings.ChromosomeLength)
	{
		CreateGenes();
	}

	public override Gene GenerateGene(int geneIndex)
	{
		FactorType factorType = geneIndex switch
		{
			< Settings.ChromosomeLength / 3 => FactorType.Angle,
			< Settings.ChromosomeLength / 3 * 2 => FactorType.Amount,
			_ => FactorType.Roll
		};

		ValueType valueType = (geneIndex % (Settings.ChromosomeLength / 3) % Settings.BonesCount - 1) switch
		{
			-1 => ValueType.Constant,
			int innerIndex => (innerIndex % 3) switch
			{
				0 => ValueType.Amplitude,
				1 => ValueType.Phase,
				_ => ValueType.Frequency
			}
		};

		float boneIndex = geneIndex % (Settings.ChromosomeLength / 3) / Settings.BoneFactorLength;

		return factorType switch
		{
			FactorType.Angle => valueType switch
			{
				ValueType.Constant => new Gene(0f),
				ValueType.Amplitude => new Gene(0f),
				ValueType.Phase => new Gene(0f),
				ValueType.Frequency => new Gene(0f),
				_ => throw new NotImplementedException(),
			},
			FactorType.Amount => valueType switch
			{
				ValueType.Constant => new Gene(0f),
				ValueType.Amplitude => new Gene(0f),
				ValueType.Phase => new Gene(0f),
				ValueType.Frequency => new Gene(0f),
				_ => throw new NotImplementedException(),
			},
			FactorType.Roll => valueType switch
			{
				ValueType.Constant => new Gene(0f),
				ValueType.Amplitude => new Gene(0f),
				ValueType.Phase => new Gene(0f),
				ValueType.Frequency => new Gene(0f),
				_ => throw new NotImplementedException(),
			},
			_ => throw new NotImplementedException(),
		};
	}

	public override IChromosome CreateNew()
	{
		return new SimulationChromosome();
	}

	public List<BoneParameters> GetAsBoneParameters()
	{
		List<BoneParameters> boneParameters = [];
		float[] values = GetGenes().Select(gene => (float)gene.Value).ToArray();
		for (int iBone = 0; iBone < Settings.BonesCount; iBone++)
		{
			List<WaveParameters> angleWaves = [];
			List<WaveParameters> amountWaves = [];
			List<WaveParameters> rollWaves = [];
			int boneOffset = Settings.BoneFactorLength * iBone;
			int iAngle = 0 + boneOffset;
			int iAmount = Settings.ChromosomeLength / 3 + boneOffset;
			int iRoll = Settings.ChromosomeLength / 3 * 2 + boneOffset;
			for (int iWave = 0; iWave < Settings.WavesPerFactor; iWave++)
			{
				int iAngleWave = iAngle + 1 + 3 * iWave;
				int iAmountWave = iAmount + 1 + 3 * iWave;
				int iRollWave = iRoll + 1 + 3 * iWave;
				angleWaves.Add(new(values[iAngleWave], values[iAngleWave + 1], values[iAngleWave + 2]));
				amountWaves.Add(new(values[iAmountWave], values[iAmountWave + 1], values[iAmountWave + 2]));
				rollWaves.Add(new(values[iRollWave], values[iRollWave + 1], values[iRollWave + 2]));
				
			}
			SimulationFactor angle = new(values[iAngle], angleWaves);
			SimulationFactor amount = new(values[iAmount], amountWaves);
			SimulationFactor roll = new(values[iRoll], rollWaves);
			boneParameters.Add(new(Settings.BoneNames[iBone], angle, amount, roll));
		}
		return boneParameters;
	}
}