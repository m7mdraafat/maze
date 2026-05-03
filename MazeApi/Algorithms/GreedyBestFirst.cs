using System.Runtime.CompilerServices;
using MazeApi.Models;

namespace MazeApi.Algorithms;

/// <summary>
/// Greedy Best-First Search (GBFS).
///
/// Idea:
///   Always expand the node that LOOKS closest to the goal,
///   judged purely by the heuristic h(n) (Manhattan distance).
///   It ignores the cost already paid to get there (g).
///
/// Properties:
///   - Complete: yes on a finite grid.
///   - Optimal:  NO. It can take detours because it never reconsiders cost.
///   - Behavior: "tunnel vision" toward the goal -> very fast in open space,
///               but easily fooled by walls.
///
/// Why include it:
///   Side-by-side with A* and Dijkstra it makes the value of a heuristic
///   AND the cost of ignoring g(n) very visible.
/// </summary>
public class GreedyBestFirst : IPathfinder
{
    public async IAsyncEnumerable<StepEvent> Solve(
        GridRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var walls = request.Walls.Select(w => (w.Row, w.Col)).ToHashSet();

        // Priority queue keyed on h only (no g component).
        var open = new PriorityQueue<Cell, int>();

        // parent[n] = how we reached n; used to rebuild the path.
        var parent = new Dictionary<Cell, Cell>();

        // Cells that have been expanded already.
        var visited = new HashSet<Cell>();

        // Seed with start using h(start, goal).
        open.Enqueue(request.Start, GridHelpers.Manhattan(request.Start, request.Goal));

        while (open.Count > 0 && !ct.IsCancellationRequested)
        {
            var current = open.Dequeue();
            if (!visited.Add(current)) continue;

            int h = GridHelpers.Manhattan(current, request.Goal);
            yield return new StepEvent("visit", current, G: null, H: h, F: null);

            // First time we pop the goal we accept the path we have, even
            // though it may not be optimal -- that is the trade-off of greedy.
            if (current == request.Goal)
            {
                foreach (var path in BuildPath(parent, current))
                    yield return new StepEvent("path", path);
                yield return new StepEvent("done", current);
                yield break;
            }

            foreach (var n in GridHelpers.Neighbors(current, request.Rows, request.Cols, walls))
            {
                // Greedy never reconsiders a cell, so the FIRST parent it sees
                // is the one we keep. (Compare: A* keeps the cheapest parent.)
                if (visited.Contains(n) || parent.ContainsKey(n)) continue;

                parent[n] = current;
                int hn = GridHelpers.Manhattan(n, request.Goal);
                open.Enqueue(n, hn);
                yield return new StepEvent("frontier", n, G: null, H: hn, F: hn);
            }
        }

        // No path found.
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

