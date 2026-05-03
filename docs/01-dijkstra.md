# Dijkstra's Algorithm — Explained Simply

## The Big Idea
Imagine dropping a stone in still water. Ripples spread out **evenly in every direction**. Dijkstra works the same way: starting from the start cell, it explores outward by **cost**, always picking the cheapest unexplored cell next.

> "Always expand the cell with the smallest cost-so-far (`g`)."

It does **not** know where the goal is. It just keeps spreading until it bumps into it.

## Real-World Analogy
You're a delivery driver with a map but **no GPS**. To find the shortest route to a customer, you measure distance to *every* nearby house first, then to their neighbors, then theirs — until you reach the destination. Slow but guaranteed shortest.

## How It Works (Step by Step)
1. Put the **Start** cell in a priority queue with cost `0`.
2. Pop the cell with the **smallest cost**.
3. If it's the Goal → reconstruct path and stop.
4. Otherwise, look at its neighbors. For each one:
   - New cost = current cost + 1
   - If we found a cheaper way to reach this neighbor, update it and push it back into the queue.
5. Repeat from step 2.

## Properties

| Question | Answer |
|---|---|
| Does it always find a path? | ✅ Yes (if one exists) |
| Does it find the **shortest** path? | ✅ **Yes** — guaranteed optimal |
| Uses a heuristic? | ❌ No |
| Visualization shape | Symmetric **diamond** around the start |

## Data Structures Used & Why

### 1. `PriorityQueue<Cell, int>` — the **open set**
- **What it does:** stores discovered cells ordered by their cost `g`.
- **Why a priority queue?** Dijkstra's whole point is "always expand the cheapest". A regular queue would give us BFS (no costs); a list would force an O(V) scan every step. A priority queue (binary heap) does it in **O(log n)**.

### 2. `Dictionary<Cell, int> g` — best known cost to each cell
- **What it does:** records the cheapest cost found so far from Start to each cell.
- **Why a dictionary?** O(1) lookup by cell. We need to constantly check "did I already find a cheaper way to this cell?" — a list would be O(n).

### 3. `Dictionary<Cell, Cell> parent` — the path memory
- **What it does:** for each cell, remembers which cell we came from on the best path.
- **Why?** Once we reach the Goal, we walk this chain backward to rebuild the full path. Without it, we'd know the cost but not the route.

### 4. `HashSet<Cell> closed` — fully-processed cells
- **What it does:** marks cells whose optimal cost is finalized.
- **Why a HashSet?** O(1) membership check. We need to skip cells that were enqueued multiple times (with stale higher costs) — without this we'd re-process them and waste work.

### 5. `HashSet<(int, int)> walls` — blocked cells
- **Why a HashSet?** O(1) "is this a wall?" check. Walls are checked many times per neighbor expansion, so this matters.

## Why "Re-Enqueue" Instead of Decrease-Key?
Textbook Dijkstra uses *decrease-key* on the priority queue when it finds a cheaper path. .NET's built-in `PriorityQueue` doesn't support that efficiently, so we just **push the cell again with the new lower priority** and use the `closed` set to ignore the old (stale) copy when it pops up. Same correctness, simpler code.

## Complexity
- **Time:** O(E log V) — every edge can cause one heap push.
- **Space:** O(V) — for `g`, `parent`, `closed`.

## Strengths & Weaknesses
✅ **Always optimal**, simple, no heuristic needed.
❌ **Slow** — explores lots of cells in directions away from the goal because it has no clue where it is.
