using System.Runtime.CompilerServices;
using MazeApi.Models;

namespace MazeApi.Algorithms;

/// <summary>
/// A* (A-Star) Search.
///
/// Idea:
///   Always expand the node with the smallest f(n) = g(n) + h(n) where:
///     g(n) = exact cost from Start to n (so far)
///     h(n) = heuristic estimate from n to Goal (Manhattan distance here)
///
/// Properties:
///   - Complete: yes (finds a path if one exists)
///   - Optimal:  yes, if h is admissible (never overestimates).
///               Manhattan distance on a 4-neighbour grid is admissible.
///   - Behavior: balances "cost so far" with "estimated cost remaining",
///               so it explores fewer cells than Dijkstra while still
///               guaranteeing the shortest path.
///
/// Complexity (worst case):
///   Time:  O(E log V) using a binary heap (PriorityQueue&lt;TElement,TPriority&gt;).
///   Space: O(V) for g, parent and closed sets.
/// </summary>
public class AStar : IPathfinder
{
    /// <summary>
    /// Streams the search step-by-step. Each <see cref="StepEvent"/> is sent to the UI
    /// in real time through SignalR so the frontend can animate the search.
    /// </summary>
    public async IAsyncEnumerable<StepEvent> Solve(
        GridRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        // Convert wall list to a HashSet of (row,col) tuples for O(1) lookup.
        var walls = request.Walls.Select(w => (w.Row, w.Col)).ToHashSet();

        // Open set: cells discovered but not yet expanded, ordered by f = g + h.
        var open = new PriorityQueue<Cell, int>();

        // g[n] = best known cost from Start to n.
        var g = new Dictionary<Cell, int> { [request.Start] = 0 };

        // parent[n] = the cell we came from on the best known path to n.
        // Used to reconstruct the final path once we reach the Goal.
        var parent = new Dictionary<Cell, Cell>();

        // Closed set: cells already expanded; their best g is finalized.
        var closed = new HashSet<Cell>();

        // Seed the open set with the Start cell.
        open.Enqueue(request.Start, GridHelpers.Manhattan(request.Start, request.Goal));

        while (open.Count > 0 && !ct.IsCancellationRequested)
        {
            // Pop the cell with the smallest f-score.
            var current = open.Dequeue();

            // A cell can be queued multiple times (we don't decrease-key);
            // skip duplicates that we've already finalized.
            if (!closed.Add(current)) continue;

            // Tell the UI: "I am now expanding this cell".
            yield return new StepEvent("visit", current, g[current],
                GridHelpers.Manhattan(current, request.Goal),
                g[current] + GridHelpers.Manhattan(current, request.Goal));

            // Goal test: for an admissible heuristic, popping the goal
            // means the optimal path has been found.
            if (current == request.Goal)
            {
                foreach (var path in BuildPath(parent, current))
                    yield return new StepEvent("path", path);
                yield return new StepEvent("done", current);
                yield break;
            }

            // Edge relaxation for each walkable neighbour.
            foreach (var n in GridHelpers.Neighbors(current, request.Rows, request.Cols, walls))
            {
                int tentative = g[current] + 1; // step cost is 1 on a uniform grid

                // If we already have a cheaper or equal path to n, skip.
                if (g.TryGetValue(n, out var existing) && tentative >= existing) continue;

                // Otherwise this is a better path; record it.
                g[n] = tentative;
                parent[n] = current;

                int h = GridHelpers.Manhattan(n, request.Goal);
                open.Enqueue(n, tentative + h);

                // Tell the UI: "this cell is now in the frontier".
                yield return new StepEvent("frontier", n, tentative, h, tentative + h);
            }
        }

        // Open set exhausted without reaching the Goal => no path.
        yield return new StepEvent("done", request.Start);
    }

    /// <summary>
    /// Walks the parent chain from goal back to start and returns the cells in start -> goal order.
    /// </summary>
    private static IEnumerable<Cell> BuildPath(Dictionary<Cell, Cell> parent, Cell goal)
    {
        var stack = new Stack<Cell>();
        var cell = goal;
        stack.Push(cell);
        while (parent.TryGetValue(cell, out var parentCell)) { stack.Push(parentCell); cell = parentCell; }
        return stack;
    }
}
