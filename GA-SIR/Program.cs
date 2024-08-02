using GA_SIR;

var (susPerDay, infPerDay, recPerDay) = Utils.ReadFile(args[0]);

// Setting up simulation variables
int popSize = 20, bitSize = 10, numGenerations = 2, days = susPerDay.Length;
float reprodutionRate = 0.3F, crossoverRate = 0.4F, mutationRate = 0.05F;
uint totalNodes = susPerDay[0] + infPerDay[0] + recPerDay[0];

// Instantiating the GA class and running the 
var ag = new GeneticAlgorithm(popSize, totalNodes, infPerDay, recPerDay, new Random(), 7);

ag.Run(reprodutionRate, crossoverRate, mutationRate, numGenerations, bitSize, days);
