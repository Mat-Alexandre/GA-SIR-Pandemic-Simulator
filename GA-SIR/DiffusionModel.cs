using QuikGraph;

namespace GA_SIR;

public abstract class DiffusionModel
{
    public string Name { get; set; }
    public bool DiscreteState { get; set; }
    public Dictionary<string, Dictionary<string, object>?> Params { get; set; }
    public Dictionary<string, sbyte> AvailableStatuses { get; set; }
    public int ActualIteration { get; set; }
    public AdjacencyGraph<int, Edge<int>> Graph { get; set; }
    public Dictionary<string, Dictionary<string, object>?> InitialStatus { get; set; }
    public Dictionary<int, int> Status { get; set; }

    public DiffusionModel(AdjacencyGraph<int, Edge<int>> graph)
    {
        DiscreteState = true;

        Params = new ()
        {
            { "nodes", new() },
            { "edges", new() },
            { "model", new() },
            { "status", new() },
        };

        AvailableStatuses = new()
        {
            { "Susceptible", 0 },
            { "Infected", 1 },
            { "Recovered", 2 },
            { "Blocked", -1 }
        };

        Name = string.Empty;

        ActualIteration = 0;

        Graph = graph;

        Status = new();
        foreach (var n in graph.Vertices)
        {
            Status.Add(n, n);
        }

        InitialStatus = new();
    }
    
    public void SetInitialStatus(DiffulsionModelConfiguration configuration)
    {
        // Set initial status
        var modelStatus = configuration.GetModelConfiguration();

        foreach (var (param, nodes) in modelStatus)
        {
            Params["status"]!.TryAdd(param, nodes);
            foreach(var node in (int[]) nodes)
            {
                Status[node] = AvailableStatuses[param];
            }
        }

        // Set model additional information
        var modelParams = configuration.GetModelParameters();

        foreach (var (param, val) in modelParams)
        {
            Params["model"]!.Add(param, val);
        }

        if (!Params["status"]!.ContainsKey("Infected"))
        {
            if (Params["model"]!.ContainsKey("percentage_infected"))
            {
                Params["model"]!["fraction_infected"] = Params["model"]!["percentage_infected"];
            }
            if (Params["model"]!.ContainsKey("fraction_infected"))
            {
                var numberOfInitialInfected = Graph.VertexCount * (float)Params["model"]!["fraction_infected"];
                if (numberOfInitialInfected < 1)
                {
                    // TODO: Generate a warning
                    numberOfInitialInfected = 1;
                }

                List<int> availableNodes = new ();
                foreach(var (n, _) in Status)
                {
                    if (Status[n] == 0)
                    {
                        availableNodes.Add(n);
                    }
                }
                var sampledNodes = Utils.GetRandomElements(availableNodes, (int)numberOfInitialInfected, new Random());
                foreach (var k in sampledNodes)
                {
                    Status[k] = AvailableStatuses["Infected"];
                }
            }
        }

        //InitialStatus = Status;
    }
    
    protected (Dictionary<int, int>, Dictionary<int, int>, Dictionary<int, int>) StatusDelta(Dictionary<int, int> actualStatus)
    {
        var actualStatusCount = new Dictionary<int, int>();
        var oldStatusCount = new Dictionary<int, int>();
        var delta = new Dictionary<int, int>();

        foreach (var (n, v) in Status)
        {
            if(v != actualStatus[n])
            {
                delta[n] = actualStatus[n];
            }
        }

        foreach (var st in AvailableStatuses.Values)
        {
            var actualStatusLength = 0;
            var statusLength = 0;
            foreach (var val in actualStatus.Values)
            {
                if (val == st)
                {
                    actualStatusLength++;
                }
            }
            foreach (var val in Status.Values)
            {
                if (val == st)
                {
                    statusLength++;
                }
            }
            actualStatusCount.Add(st, actualStatusLength);
            oldStatusCount.Add(st, actualStatusLength);
        }

        var statusDelta = new Dictionary<int, int>();
        foreach(var key in actualStatusCount.Keys)
        {
            statusDelta.Add(key, actualStatusCount[key] - oldStatusCount[key]);
        }

        return (delta, actualStatusCount, statusDelta);
    }
    
    protected abstract Dictionary<string, object> Iterations(int bunchSize);
    
    public IEnumerable<Dictionary<string, object>> IterationBunch(int bunchSize)
    {
        var systemStatus = new List<Dictionary<string, object>>();

        for(int i = 0; i < bunchSize; i++)
        {
            systemStatus.Add(Iterations(bunchSize));
        }

        return systemStatus;
    }

    public void CleanInitialStatus(IEnumerable<sbyte> validStatus)
    {
        foreach(var (key, val) in Status)
        {
            if(!validStatus.Contains((sbyte)val))
            {
                Status[key] = 0;
            }
        }
    }
}
