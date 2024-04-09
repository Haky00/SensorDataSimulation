using System.Diagnostics;
using Newtonsoft.Json;
using GeneticSharp;
using SensorDataSimulation;

var selection = new EliteSelection();
var crossover = new UniformCrossover();
var mutation = new SimulationMutation();
var fitness = new SimulationFitness();
var chromosome = new SimulationChromosome(new WalkingTemplate(1, 1));
var population = new Population(60, 150, chromosome);

var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
{
    Termination = new GenerationNumberTermination(250),
    TaskExecutor = new ParallelTaskExecutor()
};

Console.WriteLine("GA running...");
Stopwatch sw = new();
sw.Start();
ga.Start();
sw.Stop();

FitnessScore bestFitness = SimulationFitness.EvaluateChromosome(ga.BestChromosome);
Console.WriteLine($"Best solution found (in {sw.ElapsedMilliseconds}ms) has {bestFitness.Score:0.000}/{bestFitness.MaxScore} fitness.");
foreach(var score in bestFitness.IndividialScores)
{
    Console.WriteLine($"{score.Key}: {score.Value.Score:0.000}/{score.Value.Max}");
}

if (ga.BestChromosome is not SimulationChromosome bestChromosome)
{
    throw new Exception("Wrong best chromosome type");
}

SimulationParameters bestParameters = bestChromosome.GetAsSimulationParameters();
File.WriteAllText(@"d:\Projects\SensorDataVisualisation\bestParameters.txt", JsonConvert.SerializeObject(bestParameters));