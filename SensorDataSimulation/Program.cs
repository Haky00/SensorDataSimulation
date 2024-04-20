using System.Diagnostics;
using Newtonsoft.Json;
using GeneticSharp;
using SensorDataSimulation;
using SensorDataSimulation.MovementTemplates;

string mode = "onTable";

IChromosomeTemplate? template = null;
Random rnd = new();

switch (mode)
{
    case "walking":
    {
        float walkingSpeed = 1.1f + (float)rnd.NextDouble() * 0.4f; // 1.1 - 1.5 m/s range
        float stepTime = 0.5f + (float)rnd.NextDouble() * 0.1f; // 0.5 - 0.6 s/step range
        template = new WalkingTemplate(walkingSpeed, stepTime);
        Console.WriteLine($"Using WalkingTemplate with walking speed {walkingSpeed:0.000} m/s and step time {stepTime:0.000} s");
        break;
    }
    case "sitting":
    {
        float direction = -MathF.PI + (float)rnd.NextDouble() * MathF.PI * 2; // 360° direction range
        float breathTime = 3.4f + (float)rnd.NextDouble() * 1.6f; // 3.4 - 5.0 s/breath range
        template = new SittingTemplate(direction, breathTime);
        Console.WriteLine($"Using SittingTemplate with direction {direction:0.000} and breath time {breathTime:0.000} s");
        break;
    }
    case "onTable":
    {
        float direction = -MathF.PI + (float)rnd.NextDouble() * MathF.PI * 2; // 360° direction range
        float upFacing = 0.96f + (float)rnd.NextDouble() * 0.4f; // 0.96 - 1.00 up facing value
        template = new OnTableTemplate(direction, upFacing);
        Console.WriteLine($"Using OnTableTemplate with direction {direction:0.000} and target upFacingValue {upFacing:0.000}");
        break;
    }
}

if (template is null)
{
    return;
}

var selection = new EliteSelection(5);
var crossover = new UniformCrossover();
var mutation = new SimulationMutation();
var fitness = new SimulationFitness();
var chromosome = new SimulationChromosome(template);
var population = new Population(80, 150, chromosome);

var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
{
    Termination = new GenerationNumberTermination(400),
    TaskExecutor = new ParallelTaskExecutor()
};

Console.WriteLine("GA running...");
Stopwatch sw = new();
sw.Start();
ga.Start();
sw.Stop();

FitnessScore bestFitness = SimulationFitness.EvaluateChromosome(ga.BestChromosome);
Console.WriteLine($"Best solution found (in {sw.ElapsedMilliseconds}ms) has {bestFitness.Score:0.000}/{bestFitness.MaxScore} fitness.");
foreach (var score in bestFitness.IndividialScores)
{
    Console.WriteLine($"{score.Key}: {score.Value.Score:0.000}/{score.Value.Max}");
}

if (ga.BestChromosome is not SimulationChromosome bestChromosome)
{
    throw new Exception("Wrong best chromosome type");
}

SimulationParameters bestParameters = bestChromosome.GetAsSimulationParameters();
bestParameters.Name = $"{template.Name}-{DateTime.Now:yyyy-dd-M--HH-mm-ss}";
File.WriteAllText(@$"d:\Projects\SensorDataVisualisation\Parameters\{bestParameters.Name}.json", JsonConvert.SerializeObject(bestParameters, Formatting.Indented));