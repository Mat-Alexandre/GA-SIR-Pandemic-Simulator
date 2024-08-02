namespace GA_SIR;
public struct Individual
{
    public byte[] Beta { get; set; }
    public byte[] Gamma { get; set; }
    public float BetaScore { get; set; } = .0F;
    public float GammaScore { get; set; } = .0F;
    public float Score { get; set; } = .0F;
    public int Generation { get; set; } = 0;
    // TODO: This can be changed to array or it must be a List
    public List<uint>? Susceptible { get; set; }
    public List<uint>? Infected { get; set; }
    public List<uint>? Recovered { get; set; }

    public Individual(int bitSize)
    {
        Beta = new byte[bitSize];
        Gamma = new byte[bitSize];
        Beta[0] = Beta[1] = 0;
        Gamma[0] = Gamma[1] = 0;

        Random rng = new Random();

        for (int i = 2; i < bitSize; i++)
        {
            Beta[i] = (byte)rng.Next(2);
            Gamma[i] = (byte)rng.Next(2);
        }
    }

    public override string ToString()
    {
        return $"Score={Score}, BetaScore={BetaScore}, GammaScore={GammaScore}";
    }
}