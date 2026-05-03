using Microsoft.AspNetCore.SignalR;
using MazeApi.Algorithms;
using MazeApi.Models;

namespace MazeApi.Hubs;

/// <summary>
/// SignalR Hub that streams the search progress to the connected browser in real time.
///
/// Why SignalR (and not plain REST)?
///   - The algorithm produces many small events (one per cell visit/frontier).
///   - We need a PUSH channel so the UI can animate as soon as each step happens,
///     instead of waiting for the whole search to finish.
///   - SignalR uses WebSockets (with fallbacks) and gives us typed method invocations.
///
/// Flow:
///   1. The browser calls `Solve(GridRequest)` over the hub.
///   2. We pick the algorithm via a small factory (Open/Closed Principle).
///   3. The algorithm yields StepEvents one at a time (IAsyncEnumerable).
///   4. Each event is forwarded to the caller via `Clients.Caller.SendAsync("Step", ...)`.
///   5. An optional DelayMs throttles the stream so a human can watch the animation.
/// </summary>
public class MazeHub : Hub
{
    /// <summary>
    /// Runs the requested algorithm and streams every step back to the caller.
    /// </summary>
    public async Task Solve(GridRequest req)
    {
        // Pick the concrete algorithm based on the request ("astar" / "greedy" / "dijkstra").
        var algo = AlgorithmFactory.Create(req.Algorithm);

        // Iterate the async stream of steps; cancellation token aborts the search
        // automatically if the client disconnects.
        await foreach (var step in algo.Solve(req, Context.ConnectionAborted))
        {
            // Push this step to the browser. The client listens with: connection.on("Step", ...)
            await Clients.Caller.SendAsync("Step", step);

            // Throttle for visualization. 0 = run as fast as possible (useful for benchmarks).
            if (req.DelayMs > 0)
                await Task.Delay(req.DelayMs, Context.ConnectionAborted);
        }
    }
}