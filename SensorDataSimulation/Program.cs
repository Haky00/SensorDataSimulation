using System.Diagnostics;
using Newtonsoft.Json;
using GeneticSharp;
using SensorDataSimulation;

var selection = new EliteSelection();
var crossover = new UniformCrossover();
var mutation = new SimulationMutation();
var fitness = new SimulationFitness();
var chromosome = new SimulationChromosome();
var population = new Population(200, 400, chromosome);

var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
{
    Termination = new GenerationNumberTermination(1000),
    TaskExecutor = new ParallelTaskExecutor()
};

Console.WriteLine("GA running...");
Stopwatch sw = new();
sw.Start();
ga.Start();
sw.Stop();

Console.WriteLine("Best solution found has {0} fitness.", ga.BestChromosome.Fitness);
Console.WriteLine(sw.ElapsedMilliseconds + "ms");

if (ga.BestChromosome is not SimulationChromosome bestChromosome)
{
    throw new Exception("Wrong best chromosome type");
}

List<BoneParameters> bestParameters = bestChromosome.GetAsBoneParameters();
File.WriteAllText("bestParameters.txt", JsonConvert.SerializeObject(bestParameters));