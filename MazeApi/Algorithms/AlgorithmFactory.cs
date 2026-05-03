namespace MazeApi.Algorithms;

public static class AlgorithmFactory
{
    public static IPathfinder Create(string name) => name.ToLowerInvariant() switch
    {
        "astar"    => new AStar(),
        "greedy"   => new GreedyBestFirst(),
        "dijkstra" => new Dijkstra(),
        _ => throw new ArgumentException($"Unknown algorithm: {name}")
    };
}