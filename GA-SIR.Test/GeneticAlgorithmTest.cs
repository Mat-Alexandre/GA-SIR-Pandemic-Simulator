namespace GA_SIR.Test;

public class GeneticAlgorithmTest
{
    private GeneticAlgorithm ga;
    private Random rng = new Random(12345);
    private Individual chromossome1;
    private Individual chromossome2;

    public GeneticAlgorithmTest()
    {
        ga = new GeneticAlgorithm(10, 50, [2, 5, 4], [8, 6, 8], rng, 5);
        chromossome1 = new Individual(5);
        chromossome2 = new Individual(5);

        chromossome1.Beta = [1, 0, 0, 1, 0];
        chromossome2.Beta = [1, 1, 0, 1, 1];

        chromossome1.Gamma = [1, 1, 0, 0, 1];
        chromossome2.Gamma = [0, 0, 0, 1, 1];
    }

    [Theory]
    [InlineData(5, 4, 39, 0)]
    public void GenerateInitialPopulation_ShouldInstantiate(int bitSize, int vertexSrc, int vertexDst, int index)
    {
        ga.GenerateInitialPopulation(bitSize);

        var actualSolutions = ga.Populations;
        Assert.NotNull(actualSolutions);

        var actualInfectedNodes = ga.InfectedNodes;
        Assert.NotNull(actualInfectedNodes);

        var actualRecoveredNodes = ga.RecoveredNodes;
        Assert.NotNull(actualRecoveredNodes);

        // This first best solutions is the solurion at Populations[0]
        var actualBestSolution = ga.BestSolution;
        var expectedBestSolution = ga.Populations![0];
        Assert.Equal(expectedBestSolution, actualBestSolution);

        Assert.True(actualBestSolution.Beta.Count() == bitSize);
        Assert.True(actualBestSolution.Gamma.Count() == bitSize);

        Assert.True(ga.Graph.OutEdge(vertexSrc, index).Target == vertexDst);
    }

    [Theory]
    [InlineData(5)]
    public void Fitness_ShouldImproveCurrentIndividual(int bitSize)
    {
        Individual actualIndividual = new Individual(bitSize);
        ga.Fitness(ref actualIndividual, bitSize, ga.InfectedPerDay.Count());

        Assert.False(actualIndividual.Score == 0);
    }

    [Fact]
    public void Crossover_ShouldSplitTheChromossomeCorrectlyInHalf()
    {
        var (actualC1, actualC2) = ga.Crossover(chromossome1, chromossome2);

        // Asserting the beta value
        Assert.Equal([1, 0, 0, 1, 1], actualC1.Beta);
        Assert.Equal([1, 1, 0, 1, 0], actualC2.Beta);

        // Asserting the Gamma value
        Assert.Equal([1, 1, 0, 1, 1], actualC1.Gamma);
        Assert.Equal([0, 0, 0, 0, 1], actualC2.Gamma);
    }

    [Fact]
    public void Mutate_ShouldChangeGeneRandomly()
    {
        var actual = ga.Mutate(chromossome1, 0.5F);

        // Asserting the beta value
        Assert.Equal([0, 0, 0, 0, 0], actual.Beta);

        // Asserting the Gamma value
        Assert.Equal([0, 1, 0, 1, 1], actual.Gamma);
    }
}
