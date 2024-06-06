using BIA601_HW.ViewModel;

public class OptimizationService
{
    private readonly ILogger<OptimizationService> _logger;

    public OptimizationService(ILogger<OptimizationService> logger)
    {
        _logger = logger;
    }

    // Main function to process transport data
    public string ProcessTransportData(TransportInputViewModel model)
    {
        _logger.LogInformation("Starting to process transport data...");

        // Dictionary to hold distributed cargos per truck
        var cargoDistribution = new Dictionary<string, List<CargoInput>>();
        foreach (var truck in model.Trucks)
        {
            // Distribute cargos based on truck capacity using Knapsack Algorithm
            var distributedCargos = KnapsackAlgorithm(model.Cargos, truck.TruckCapacity);
            cargoDistribution.Add(truck.TruckName, distributedCargos);
            _logger.LogInformation("Truck {TruckName} Distributed Cargos: {@distributedCargos}", truck.TruckName, distributedCargos);
            model.Cargos = model.Cargos.Except(distributedCargos).ToList();
        }

        // Define start and end addresses
        int startAddress = model.Addresses.First().AddressNumber;
        int endAddress = model.Addresses.Last().AddressNumber;

        // Build graph from model addresses and distances
        var graph = BuildGraph(model);

        // Execute Dijkstra's Algorithm to find the shortest paths
        var previous = Dijkstra(graph, startAddress);

        // Find optimal route using the shortest path from start to end
        var optimalRoute = GetShortestPath(previous, startAddress, endAddress);

        // Assign distributed cargos and optimal route to each truck
        foreach (var truck in model.Trucks)
        {
            truck.Cargos = cargoDistribution.ContainsKey(truck.TruckName) ? cargoDistribution[truck.TruckName] : new List<CargoInput>();
            truck.Route = optimalRoute;
        }

        // Generate result string
        return GenerateResult(model);
    }

    // Generate the result string
    private string GenerateResult(TransportInputViewModel model)
    {
        var result = "Distributed Cargos and Optimal Routes:\n";
        foreach (var truck in model.Trucks)
        {
            var cargoNames = truck.Cargos.Select(c => c.CargoName).ToList();
            result += $"Truck {truck.TruckName}:\n";
            result += $"  Cargos: {string.Join(", ", cargoNames)}\n";
            result += $"  Route: {string.Join(" -> ", truck.Route)}\n";
        }

        return result;
    }

    // Get the shortest path from start to end using the previous node map
    private List<int> GetShortestPath(Dictionary<int, int> previous, int start, int end)
    {
        var path = new List<int>();
        var currentNode = end;

        // Trace the path from end to start using the previous node map
        while (currentNode != start)
        {
            if (!previous.ContainsKey(currentNode))
            {
                throw new InvalidOperationException($"No path found from {start} to {end}. Current node: {currentNode}, Path so far: {string.Join(" -> ", path)}");
            }

            path.Add(currentNode);
            currentNode = previous[currentNode];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    // Dijkstra's Algorithm to find the shortest path from the start node to all other nodes
    private Dictionary<int, int> Dijkstra(Dictionary<int, List<Tuple<int, int>>> graph, int start)
    {
        var distances = graph.Keys.ToDictionary(v => v, v => int.MaxValue);
        var previous = new Dictionary<int, int>();
        var priorityQueue = new SortedSet<Tuple<int, int>>(Comparer<Tuple<int, int>>.Create((a, b) =>
            a.Item1 == b.Item1 ? a.Item2.CompareTo(b.Item2) : a.Item1.CompareTo(b.Item1)));

        // Set the distance for the start node to 0 and add it to the priority queue
        distances[start] = 0;
        priorityQueue.Add(Tuple.Create(0, start));

        // Process the nodes in the priority queue
        while (priorityQueue.Any())
        {
            var (currentDistance, currentNode) = priorityQueue.Min;
            priorityQueue.Remove(priorityQueue.Min);

            // Evaluate the neighbors of the current node
            foreach (var (neighbor, weight) in graph[currentNode])
            {
                var distance = currentDistance + weight;

                // Update the distance to the neighbor if a shorter path is found
                if (distance < distances[neighbor])
                {
                    if (distances[neighbor] != int.MaxValue)
                    {
                        priorityQueue.Remove(Tuple.Create(distances[neighbor], neighbor));
                    }
                    distances[neighbor] = distance;
                    previous[neighbor] = currentNode;
                    priorityQueue.Add(Tuple.Create(distance, neighbor));
                }
            }
        }

        return previous;
    }

    // Build graph from transport input model
    private Dictionary<int, List<Tuple<int, int>>> BuildGraph(TransportInputViewModel model)
    {
        var graph = new Dictionary<int, List<Tuple<int, int>>>();

        // Populate the graph with nodes and edges
        foreach (var address in model.Addresses)
        {
            if (!graph.ContainsKey(address.AddressNumber))
                graph[address.AddressNumber] = new List<Tuple<int, int>>();

            foreach (var distance in address.Distances)
            {
                if (distance.Distance > 0)
                {
                    graph[address.AddressNumber].Add(Tuple.Create(distance.ToAddress, (int)distance.Distance));
                }
            }
        }

        return graph;
    }

    // Implementation of Knapsack Algorithm to distribute cargos
    private List<CargoInput> KnapsackAlgorithm(List<CargoInput> cargos, double truckCapacity)
    {
        int n = cargos.Count;
        int W = (int)truckCapacity;

        double[,] dp = new double[n + 1, W + 1];

        // Populate the dp table using dynamic programming
        for (int i = 1; i <= n; i++)
        {
            for (int w = 0; w <= W; w++)
            {
                if (cargos[i - 1].CargoWeight <= w)
                {
                    dp[i, w] = Math.Max(dp[i - 1, w], dp[i - 1, (int)(w - cargos[i - 1].CargoWeight)] + cargos[i - 1].CargoValue);
                }
                else
                {
                    dp[i, w] = dp[i - 1, w];
                }
            }
        }

        // Trace back to find the items included in the optimal solution
        List<CargoInput> result = new List<CargoInput>();
        int remainingWeight = W;
        for (int i = n; i > 0 && remainingWeight > 0; i--)
        {
            if (dp[i, remainingWeight] != dp[i - 1, remainingWeight])
            {
                result.Add(cargos[i - 1]);
                remainingWeight -= (int)cargos[i - 1].CargoWeight;
            }
        }

        result.Reverse();
        return result;
    }
}
