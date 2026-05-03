using MazeApi.Models;

namespace MazeApi.Algorithms;

/// <summary>
/// Interface for pathfinding algorithms. Implementations will solve the maze based on the provided GridRequest and yield StepEvents to indicate the progress of the algorithm.
/// </summary>
public interface IPathfinder
{
    /// <summary>
    /// Solves the maze based on the provided GridRequest. The method returns an asynchronous stream of StepEvents, which can be used to visualize the algorithm's progress in real-time.
    /// </summary>
    /// <param name="request">The request containing the grid and other parameters for the pathfinding algorithm.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>An asynchronous stream of StepEvents representing the progress of the algorithm.</returns>
    IAsyncEnumerable<StepEvent> Solve(GridRequest request, CancellationToken ct);
}