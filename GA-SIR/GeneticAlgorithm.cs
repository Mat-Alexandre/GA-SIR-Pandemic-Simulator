using QuikGraph;
using QuikGraph.Algorithms;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace GA_SIR;

public class GeneticAlgorithm
{
    /// <summary>
    /// It represents how many nodes (vertices) the generated graph has.
    /// </summary>
    public uint TotalNodes { get; set; }
    /// <summary>
    /// It represents the population size of a G.A. instance.
    /// </summary>
    public int PopSize { get; set; }
    /// <summary>
    /// It holds a copy of the current best solution.
    /// </summary>
    public Individual BestSolution { get; set; }
    /// <summary>
    /// It contains a list of solutions.
    /// </summary>
    public List<Individual>? Populations { get; set; }
    /// <summary>
    /// It represents the amount of infected nodes by day.
    /// It has the length of rows (e.g. recorded days) read from the .csv dataset.
    /// </summary>
    public uint[] InfectedPerDay { get; set; }
    /// <summary>
    /// It represents the amount of recovered nodes by day.
    /// It has the length of rows (e.g. recorded days) read from the .csv dataset.
    /// </summary>
    public uint[] RecoveredPerDay { get; set; }
    /// <summary>
    /// The graph that represents the network of interactions of the simulation.
    /// </summary>
    public AdjacencyGraph<int, Edge<int>> Graph { get; set; }
    /// <summary>
    /// It represents a sample of infected nodes gathered from the graph.
    /// </summary>
    public int[] InfectedNodes { get; set; }
    /// <summary>
    /// It represents a sample of recovered nodes gathered from the graph.
    /// </summary>
    public int[] RecoveredNodes { get; set; }
    public Random RandomGenerator { get; set; }

    public GeneticAlgorithm(
        int popSize,
        uint totalNodes,
        uint[] infPerDay,
        uint[] recPerDay,
        Random randomGenerator,
        int maxEdgeCount
    )
    {
        PopSize = popSize;
        TotalNodes = totalNodes;
        InfectedPerDay = infPerDay;
        RecoveredPerDay = recPerDay;
        RandomGenerator = randomGenerator;

        // Equivalent to an Erdös-Renyi generated graph
        Graph = new AdjacencyGraph<int, Edge<int>>();
        int v = 0;
        RandomGraphFactory.Create(
            graph: Graph,
            vertexFactory: () => ++v,
            edgeFactory: (source, target) => new EquatableEdge<int>(source, target),
            rng: RandomGenerator,
            vertexCount: (int)TotalNodes,
            edgeCount: RandomGenerator.Next(1, maxEdgeCount),
            selfEdges: false
        );

        uint sampleSize = infPerDay[0] + recPerDay[0];
        var aux = Utils.GetRandomElements(Graph.Vertices, (int)sampleSize, RandomGenerator);
        InfectedNodes = Utils.GetRandomElements(aux, (int)infPerDay[0], RandomGenerator).ToArray();
        RecoveredNodes = aux.Except(InfectedNodes).ToArray();
    }

    // TODO: Documentate it
    public void Run(float reprodutionRate, float crossoverRate, float mutationRate, int numGenerations, int bitSize, int days)
    {
        GenerateInitialPopulation(bitSize);

        // Keeping track of best solution
        Individual solution = Populations![0];
        Fitness(ref solution, bitSize, days);
        BestSolution = solution;

        // Iterating over the generations 
        for (int gen = 0; gen < numGenerations; gen++)
        {
            Console.WriteLine($"========================= Generation {gen} =========================");
            // Evaluating all candidates in the population
            for (int i = 0; i < Populations!.Count; i++)
            {
                solution = Populations[i];
                Fitness(ref solution, bitSize, days);
                Populations[i] = solution;
                // Checking for new best solution
                if (solution.Score < BestSolution.Score)
                    BestSolution = solution;
                Console.WriteLine($"{i,2} | Score {solution.Score,7} | BetaScore {solution.BetaScore,9} | GammaScore {solution.GammaScore,9}");
            }
            Console.WriteLine();

            var nextGeneration = new List<Individual>
            {
                Populations[0],
                Populations[1]
            };

            for (int i = 0; i < (PopSize / 2) - 1; i++)
            {
                // Selecting individual candidates to be the parents
                var (p1, p2) = Selection();
                var offspring1 = Populations[p1];
                var offspring2 = Populations[p2];

                if (RandomGenerator.NextDouble() >= reprodutionRate)
                {
                    // Applying the crossover
                    if (RandomGenerator.NextDouble() < crossoverRate)
                        (offspring1, offspring2) = Crossover(offspring1, offspring2);
                    // Mutating both the offsprings at the same rate
                    offspring1 = Mutate(offspring1, mutationRate);
                    offspring2 = Mutate(offspring2, mutationRate);
                }
                nextGeneration.Add(offspring1);
                nextGeneration.Add(offspring2);
            }

            Console.WriteLine($"Next Gen Size: {nextGeneration.Count}");

            Populations = nextGeneration;
        }

        // At this point, the BestSolution should be the final result
        PrintGAResult();
    }

    // TODO: Documentate it
    public void GenerateInitialPopulation(int bitSize)
    {
        Populations = new List<Individual>();
        for (int i = 0; i < PopSize; i++)
        {
            Populations.Add(new Individual(bitSize));
        }

        BestSolution = Populations[0];
    }

    // TODO: Documentate it
    public void Fitness(ref Individual individual, int days)
    {
        const float ONE_MILLION = 1_000_000;
        int bitSize = individual.Gamma.Length;
        var maxValue = (2 << bitSize);

        individual.Susceptible = new();
        individual.Infected = new();
        individual.Recovered = new();

        int betaToDecimal = Utils.BinaryArrayToDecimal(individual.Beta);
        var beta = Utils.Interpolate(betaToDecimal, 0, (maxValue - 1));

        int gammaToDecimal = Utils.BinaryArrayToDecimal(individual.Gamma);
        var gamma = Utils.Interpolate(gammaToDecimal, 0, (maxValue - 1));

        // Instantiate a DifusionModel with the created graph
        var model = new SIRModel(Graph);

        var cfg = new DiffulsionModelConfiguration();
        cfg.AddModelParameter("beta", beta);
        cfg.AddModelParameter("gamma", gamma);

        cfg.AddModelInitialConfiguration("Infected", InfectedNodes);
        cfg.AddModelInitialConfiguration("Removed", RecoveredNodes);

        model.SetInitialStatus(cfg);

        foreach (var iteration in model.IterationBunch(days))
        {
            var value = (Dictionary<int, int>)iteration["node_count"];

            individual.Susceptible.Add((ushort)value[0]);
            individual.Infected.Add((ushort)value[1]);
            individual.Recovered.Add((ushort)value[2]);
        }

        var betaScore = (float)Math.Round(Utils.MeanSquaredError(InfectedPerDay, individual.Infected), 5);
        individual.BetaScore = betaScore;

        var gammaScore = (float)Math.Round(Utils.MeanSquaredError(RecoveredPerDay, individual.Recovered), 5);
        individual.GammaScore = gammaScore;

        // TODO: Validate the scores (check if it is not overflowed).
        var mean = (individual.BetaScore + individual.GammaScore) / 2;
        individual.Score = (float)Math.Round(ONE_MILLION / mean, 5);
    }

    public (Individual, Individual) Crossover(Individual chromossome1, Individual chromossome2)
    {
        // For simplification, this function split the chromossome in half
        var bitSize = chromossome1.Beta.Count();
        int crossOverPoint = bitSize / 2;

        for (int i = crossOverPoint; i < bitSize; i++)
        {
            //// Crossovering for the beta
            (chromossome1.Beta[i], chromossome2.Beta[i]) = (chromossome2.Beta[i], chromossome1.Beta[i]);

            //// Crossovering for the beta
            (chromossome1.Gamma[i], chromossome2.Gamma[i]) = (chromossome2.Gamma[i], chromossome1.Gamma[i]);
        }
        return (chromossome1, chromossome2);
    }

    public Individual Mutate(Individual offspring, float mutationRate)
    {
        for (int i = 0; i < offspring.Beta.Count(); i++)
        {
            if (RandomGenerator.NextDouble() > mutationRate)
            {
                offspring.Beta[i] = (byte)Math.Abs(offspring.Beta[i] - 1);
                offspring.Gamma[i] = (byte)Math.Abs(offspring.Gamma[i] - 1);
            }
        }
        return offspring;
    }

    public void PrintGAResult()
    {
        Console.WriteLine("The best found solution:");
        Console.WriteLine($"{BestSolution.Score}");
    }

    public (int, int) Selection()
    {
        // TODO: Must keep track of the already selected individuals
        int parent1, parent2;
        var candidates = Enumerable.Range(0, PopSize).OrderBy(x => RandomGenerator.Next()).Take(4).ToList();
        // Tournament for the two first parents
        if (Populations![candidates[0]].Score < Populations[candidates[1]].Score)
            parent1 = candidates[1];
        else
            parent1 = candidates[0];

        // Tournament for the two last parents
        if (Populations[candidates[2]].Score < Populations[candidates[3]].Score)
            parent2 = candidates[3];
        else
            parent2 = candidates[2];

        return (parent1, parent2);
    }
}