namespace GA_SIR;

public class DiffulsionModelConfiguration
{
    public Dictionary<string, Dictionary<string, object>> Config { get; set; }

    public DiffulsionModelConfiguration()
    {
        Config = new ()
        {
            { "nodes", new () { } },
            { "edges", new () { } },
            { "model", new () { } },
            { "status", new () { } },
        };
    }

    public void AddModelParameter(string parameterName, float parameterValue)
    {
        Config["model"].Add(parameterName, parameterValue);
    }

    public void AddModelInitialConfiguration(string statusName, int[] nodes) 
    {
        Config["status"].Add(statusName, nodes);
    }

    public Dictionary<string, object> GetModelConfiguration()
    {
        return Config["status"];
    }

    public Dictionary<string, object> GetModelParameters()
    {
        return Config["model"];
    }
}
