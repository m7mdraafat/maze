# A* (A-Star) Search — Explained Simply

## The Big Idea
A\* is the **best of both worlds**: it combines Dijkstra's honesty about cost-so-far with Greedy's intuition about distance-to-goal.

> "Always expand the cell with the smallest **f = g + h**"
>
> - `g` = cost from Start to this cell (what Dijkstra uses)
> - `h` = estimated cost from this cell to Goal (what Greedy uses)

This single formula gives us **shortest paths** like Dijkstra, but **much faster** because the heuristic steers the search toward the goal.

## Real-World Analogy
You're walking to a friend's house. At every street corner you ask:
- "How far have I already walked?" (`g`)
- "How far do I think I still need to go?" (`h`)

You take the route that **minimizes the total trip estimate**. That's A\*.

## How It Works (Step by Step)
1. Put **Start** in a priority queue with priority `f = 0 + h(start, goal)`.
2. Pop the cell with the **smallest f**.
3. If it's the Goal → reconstruct path and stop.
4. Otherwise, for each neighbor:
   - Compute tentative cost: `g[current] + 1`.
   - If this is **cheaper** than what we knew before for this neighbor:
     - Update `g[neighbor]` and `parent[neighbor]`.
     - Push it into the queue with priority `g[neighbor] + h(neighbor, goal)`.
5. Repeat from step 2.

## The Magic Formula: f = g + h

| Component | Meaning | Effect |
|---|---|---|
| `g` only | Pure Dijkstra | Optimal but slow |
| `h` only | Pure Greedy | Fast but not optimal |
| `g + h` | **A\*** | **Optimal AND fast** |

## Why A\* Is Optimal: Admissible Heuristic
For A\* to guarantee the shortest path, the heuristic `h` must **never overestimate** the real remaining cost. We say it's **admissible**.

**Manhattan distance** `|Δrow| + |Δcol|` is admissible on a 4-neighbour grid because the true path can never be shorter than that — it always needs at least that many steps.

## Properties

| Question | Answer |
|---|---|
| Does it always find a path? | ✅ Yes |
| Does it find the **shortest** path? | ✅ **Yes** (with admissible h) |
| Uses a heuristic? | ✅ Yes (Manhattan) |
| Visualization shape | **Diamond stretched toward the goal** |

## Data Structures Used & Why

### 1. `PriorityQueue<Cell, int>` — the **open set**
- **What it does:** stores discovered cells ordered by `f = g + h`.
- **Why a priority queue?** We need the cell with the smallest `f` in O(log n) every iteration.

### 2. `Dictionary<Cell, int> g` — best known cost from Start
- **What it does:** records the cheapest known cost to reach each cell.
- **Why a dictionary?** O(1) lookup. We constantly need to check "is the new path I found cheaper than the old one?" If we used a list this would be O(n).

### 3. `Dictionary<Cell, Cell> parent` — path memory
- **What it does:** remembers, for each cell, who we came from on the best path.
- **Why?** To rebuild the final path by walking from Goal back to Start.
- **Difference from Greedy:** we **overwrite** `parent[n]` whenever we find a cheaper path — that's how A\* corrects itself and stays optimal.

### 4. `HashSet<Cell> closed` — finalized cells
- **What it does:** cells whose optimal `g` is locked in.
- **Why a HashSet?** O(1) membership. With an admissible heuristic, the **first** time A\* pops a cell its cost is provably optimal, so we never need to expand it again. The closed set enforces this and skips stale duplicates from the queue.

### 5. `HashSet<(int, int)> walls` — blocked cells
- **Why a HashSet?** O(1) "is this a wall?" check during neighbor expansion.

## Why "Re-Enqueue" Instead of Decrease-Key?
.NET's `PriorityQueue` doesn't support efficiently lowering an existing item's priority. So when we find a cheaper path to a neighbor, we just **push it again with the new lower `f`**. The `closed` check ensures we ignore the old, stale copy when it surfaces. Same result, simpler code.

## Complexity
- **Time:** O(E log V) — same big-O as Dijkstra.
- **Space:** O(V).
- **In practice:** A\* explores **far fewer** cells than Dijkstra because the heuristic prunes wrong directions.

## Strengths & Weaknesses
✅ **Optimal** like Dijkstra, **fast** like Greedy — the gold standard for grid pathfinding.
❌ Slightly more memory than Greedy (needs the `g` map).
❌ Quality depends on the heuristic — a bad `h` makes A\* slow; an inadmissible `h` breaks optimality.

## Comparison Recap

| | Dijkstra | Greedy | **A\*** |
|---|---|---|---|
| Priority | `g` | `h` | **`g + h`** |
| Optimal | ✅ | ❌ | ✅ |
| Speed | Slow | Fast | **Fast** |
| Memory | O(V) | O(V) | O(V) |
| Best use | No goal info | Quick & dirty | **General pathfinding** |

## Why A\* Wins Most of the Time
It's the only algorithm of the three that gives you **both** guarantees in the same package: *"You will get the shortest path, and you won't waste time looking in dumb directions."*
