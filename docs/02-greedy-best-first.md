# Greedy Best-First Search — Explained Simply

## The Big Idea
Greedy looks at the goal on the map and **always moves toward whichever cell *looks* closest to it**, ignoring how much effort it took to get to where it currently is.

> "Always expand the cell with the smallest heuristic `h` (estimated distance to goal)."

It's the opposite of Dijkstra: Dijkstra cares only about cost-so-far, Greedy cares only about distance-to-go.

## Real-World Analogy
You're walking in a desert and you can see a tower (the goal) far away. You always step **toward the tower**, even if a sand dune blocks you. You might end up walking around the whole dune the long way — because you never reconsider, you just keep aiming at the tower.

## How It Works (Step by Step)
1. Put the **Start** cell in a priority queue with priority `h(start, goal)`.
2. Pop the cell with the **smallest h** (looks closest to goal).
3. If it's the Goal → reconstruct path and stop.
4. Otherwise, for each neighbor we haven't seen yet:
   - Record this cell as its parent (only the **first** time we see it — Greedy never reconsiders).
   - Push it into the queue with priority `h(neighbor, goal)`.
5. Repeat from step 2.

## Properties

| Question | Answer |
|---|---|
| Does it always find a path? | ✅ Yes |
| Does it find the **shortest** path? | ❌ **No** — can take detours |
| Uses a heuristic? | ✅ Yes (Manhattan distance) |
| Visualization shape | Narrow **stream beelining** toward goal |

## The Heuristic: Manhattan Distance
`h(cell, goal) = |Δrow| + |Δcol|`

The minimum number of steps if there were **no walls**. It's a fast guess at "how far am I from the goal?".

## Data Structures Used & Why

### 1. `PriorityQueue<Cell, int>` — the **open set**
- **What it does:** stores discovered cells ordered by `h` (distance to goal).
- **Why a priority queue?** Greedy needs the cell that *looks closest* to the goal in O(log n). A plain queue would give BFS; a list would be O(n) per step.

### 2. `Dictionary<Cell, Cell> parent` — path memory
- **What it does:** records who each cell was discovered from.
- **Why?** Same as Dijkstra — to walk back from Goal to Start once we arrive.
- **Important difference from A\*:** we set `parent[n]` only the **first** time we see `n` and never overwrite it. Greedy doesn't try to find a better path; it commits to whatever it found first.

### 3. `HashSet<Cell> visited` — already-expanded cells
- **What it does:** prevents re-expanding the same cell.
- **Why a HashSet?** O(1) check. Without it, the algorithm could pop the same cell many times and explode in time.

### 4. `HashSet<(int, int)> walls` — blocked cells
- **Why a HashSet?** O(1) "is this a wall?" check during neighbor expansion.

## Why No `g` Cost Map?
Because Greedy **does not care** about cost-so-far. Skipping that map makes Greedy slightly leaner than A* and Dijkstra in memory, but it's the very reason Greedy is **not optimal**.

## Complexity
- **Time:** O(E log V) worst case — same as A*/Dijkstra.
- **Space:** O(V) — slightly less than the others (no `g` dictionary).
- **Practical speed:** often the **fastest** of the three on open mazes — it makes a beeline.

## Strengths & Weaknesses
✅ **Very fast** in open spaces, simple, low memory.
❌ **Not optimal** — walls and obstacles can trick it into long detours because it never reconsiders.

## When to Use It
When you need a path *quickly* and you're OK with it being not-quite-shortest. Common in real-time games where AI must react in milliseconds.
