using MazeApi.Models;
using System.Runtime.CompilerServices;

namespace MazeApi.Algorithms;

/// <summary>
/// Dijkstra's Algorithm (Uniform-Cost Search on a 1-cost grid).
///
/// Idea:
///   Always expand the node with the smallest g(n) (cost from Start so far).
///   No heuristic is used -- it explores uniformly outward like ripples in water.
///
/// Properties:
///   - Complete: yes.
///   - Optimal:  yes (guarantees shortest path with non-negative edge weights).
///   - Behavior: symmetric "diamond" expansion around the start.
///               On a uniform grid it is equivalent to BFS with a priority queue.
///
/// Why include it:
///   Acts as the BASELINE. Comparing it with A* visually shows the value
///   that a good heuristic adds (fewer cells explored, same shortest path).
/// </summary>
public class Dijkstra : IPathfinder
{
    public async IAsyncEnumerable<StepEvent> Solve(
        GridRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var walls = request.Walls.Select(w => (w.Row, w.Col)).ToHashSet();

        // Priority queue keyed on g (cost so far).
        var open = new PriorityQueue<Cell, int>();

        // g[n] = best known cost from Start to n.
        var g = new Dictionary<Cell, int> { [request.Start] = 0 };

        // Parent links for path reconstruction.
        var parent = new Dictionary<Cell, Cell>();

        // Closed set: a cell's first dequeue is provably optimal,
        // so it is never re-expanded.
        var closed = new HashSet<Cell>();

        open.Enqueue(request.Start, 0);

        while (open.Count > 0 && !ct.IsCancellationRequested)
        {
            var current = open.Dequeue();

            // Skip stale duplicates (we don't decrease-key, we re-enqueue).
            if (!closed.Add(current)) continue;

            yield return new StepEvent("visit", current, g[current], null, null);

            // Optimal goal cost is finalized as soon as the goal is dequeued.
            if (current == request.Goal)
            {
                foreach (var path in BuildPath(parent, current))
                    yield return new StepEvent("path", path);
                yield return new StepEvent("done", current);
                yield break;
            }

            // Relax outgoing edges.
            foreach (var n in GridHelpers.Neighbors(current, request.Rows, request.Cols, walls))
            {
                int tentative = g[current] + 1; // uniform edge cost = 1
                if (g.TryGetValue(n, out var existing) && tentative >= existing) continue;

                g[n] = tentative;
                parent[n] = current;
                open.Enqueue(n, tentative);
                yield return new StepEvent("frontier", n, tentative, null, null);
            }
        }

        yield return new StepEvent("done", request.Start);
    }

    private static IEnumerable<Cell> BuildPath(Dictionary<Cell, Cell> parent, Cell goal)
    {
        var stack = new Stack<Cell>();
        var cell = goal;
        stack.Push(cell);
        while (parent.TryGetValue(cell, out var parentCell)) { stack.Push(parentCell); cell = parentCell; }
        return stack;
    }
}

