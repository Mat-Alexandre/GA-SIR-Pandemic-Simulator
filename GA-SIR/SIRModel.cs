using QuikGraph;

namespace GA_SIR;

public class SIRModel : DiffusionModel
{
    public List<int> Active { get; set; }
    public Dictionary<string, Dictionary<string, object?>> Parameters { get; set; }

    public SIRModel(AdjacencyGraph<int, Edge<int>> graph) : base(graph)
    {
        
        AvailableStatuses = new()
        {
            { "Susceptible", 0 },
            { "Infected", 1 },
            { "Removed", 2 }
        };

        Parameters = new()
        {
            { "model", new() 
                {
                    {  "beta", new TParam(description: "Infection rate", range: new() { 0, 1 }, optional: false, defaultValue: 0) },
                    {  "gamma", new TParam(description: "Recovery rate", range: new() { 0, 1 }, optional: false, defaultValue: 0) },
                    {  "tp_rate", new TParam(description: "Whether if the infection rate depends on the number of infected neighbors", range: new() { 0, 1 }, optional: true, defaultValue: 1) }
                }
            },
            { "nodes", new() },
            { "edges", new() }
        };

        Active = new();
        
        Name = "SIR";
    }

    override protected Dictionary<string, object> Iterations(int bunchSize)
    {
        // FIX: see if it is possible to simplify this function return
        // to accept only the node_count key.
        // Understand where else the other values are being used.

        // WARNING: The function signatures originally should accept a bool param.
        // For the usage of SIRModel, it wasn't necessary any longer
        CleanInitialStatus(AvailableStatuses.Values.ToArray());

        var actualStatus = new Dictionary<int, int>();
        foreach(var (key, value) in Status)
        {
            actualStatus.Add(key, value);
        }

        foreach (var status in Status.Keys)
        {
            Active.Add(status);
        }

        if (ActualIteration == 0)
        {
            ActualIteration += 1;

            var (delta, nodeCount, statusDelta) = StatusDelta(actualStatus);

            return new Dictionary<string, object>()
            {
                { "iteration", 0 },
                { "status", actualStatus }, // it needs to be a copy
                { "node_count", nodeCount }, // it needs to be a copy
                { "staus_delta", statusDelta}, // it needs to be a copy
            };
        }

        foreach(var u in Active)
        {
            var uStatus = Status[u];

            if (uStatus == 1)
            {
                Random rng = new Random();
                // This implementations doesn't accept directed graphs
                var susceptibleNeighbors = new List<int>();
                foreach (var neighbor in Graph.OutEdges(u))
                {
                    susceptibleNeighbors.Add(neighbor.Target);
                }

                foreach(var neighbor in susceptibleNeighbors)
                {
                    if((float)rng.NextDouble() < (float)Params["model"]!["beta"])
                    {
                        actualStatus[neighbor] = 1;
                    }
                }
                if ((float)rng.NextDouble() < (float)Params["model"]!["gamma"])
                {
                    actualStatus[u] = 2;
                }
            }
        }

        var (delta_, nodeCount_, statusDelta_) = StatusDelta(actualStatus);
        Status = actualStatus;
        ActualIteration++;

        return new Dictionary<string, object>()
        {
            { "iteration", ActualIteration-1 },
            { "status", delta_ }, // it needs to be a copy
            { "node_count", nodeCount_ }, // it needs to be a copy
            { "staus_delta", statusDelta_}, // it needs to be a copy
        };
    }
}

class TParam 
{
    public TParam(String description, List<int> range, bool optional, int defaultValue)
    {
        Description = description;
        Range = range;
        Optional = optional;
        DefaultValue = defaultValue;
    }

    public string Description { get; set; }
    public List<int>? Range { get; set; } = null;
    public bool Optional { get; set; }
    public int DefaultValue { get; set; }
}
