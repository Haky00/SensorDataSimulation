# Sensor Data Simulation

This project is a part of a master's thesis "Movement simulation of a handheld device". Related repositories can be found [here](https://github.com/stars/Haky00/lists/dp).

## About

A .NET console app used to generate parameters for sine waves used to generate human-like movement of a skeleton. Generated parameters are used in [SensorDataVisualisation](https://github.com/Haky00/SensorDataVisualisation) and the [altered version of JShelter extension](https://github.com/Haky00/jsrestrictor-sensors).

Parameter sets are generated using a genetic algorithm. [GeneticSharp](https://github.com/giacomelli/GeneticSharp) is used to provide the base of the genetic algorithm.

Currently, 3 modes of generated "movement" are available: walking, sitting and stationary device (on a table).

Usage:`.\SensorDataSimulation.exe numberOfSetsToGenerate mode outputFolderPath`

Names of available modes: `walking` `sitting` `onTable`

After generating a set of parameters, fitness values for different metrics of that set are shown.