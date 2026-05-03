# Heuristic Search Visualizer

A full-stack educational project that visualizes three classic pathfinding algorithms — **A\***, **Greedy Best-First Search**, and **Dijkstra** — in real time on a 3D grid.

The backend (.NET 8) runs the algorithm and **streams every step** to the browser. The frontend (React + TypeScript + Three.js) animates each step on a 3D maze with smooth height transitions and glowing cells.

---

## Architecture

```
┌──────────────────────────┐        SignalR (WebSocket)        ┌─────────────────────────────┐
│  React + TypeScript      │  ◄── "Step" events (real-time) ──  │  ASP.NET Core 8 + SignalR  │
│  Three.js (R3F + drei)   │                                    │                              │
│  Zustand store           │  ── invoke("Solve", request) ─►    │  MazeHub  →  IPathfinder    │
│                          │                                    │                              │
│  Grid3D, Controls, UI    │                                    │  AStar / Greedy / Dijkstra  │
└──────────────────────────┘                                    └─────────────────────────────┘
```

- **Why SignalR?** The algorithm produces many small events. We need a **push** channel so the UI animates as soon as each cell is visited, instead of waiting for the whole search to finish.
- **Why `IAsyncEnumerable<StepEvent>`?** It lets the algorithm *yield* each step lazily. The hub awaits the stream and forwards every event to the caller — no buffering, no blocking.
- **Why Zustand?** Tiny global state with selective subscriptions, so updating one cell doesn't re-render the whole 600-cell grid.

---

## Project Layout

```
Maze/
├── MazeApi/                       # .NET 8 Web API + SignalR
│   ├── Models/MazeModels.cs       # Cell, GridRequest, StepEvent
│   ├── Algorithms/
│   │   ├── IPathfinder.cs         # Common interface (one Solve method)
│   │   ├── GridHelpers.cs         # Neighbors() + Manhattan distance
│   │   ├── AStar.cs               # A* search (optimal, heuristic-guided)
│   │   ├── GreedyBestFirst.cs     # Greedy BFS (fast, NOT optimal)
│   │   ├── Dijkstra.cs            # Dijkstra (optimal, no heuristic)
│   │   └── AlgorithmFactory.cs    # Maps name → concrete algorithm
│   ├── Hubs/MazeHub.cs            # Streams steps over SignalR
│   └── Program.cs                 # CORS + SignalR wiring
│
└── maze-web/                      # React + TS + Vite
    └── src/
        ├── types/                 # Cell, CellState, StepEvent
        ├── stores/maze-store.ts   # Zustand global state + metrics
        ├── services/mazeClient.ts # SignalR client + runSolve()
        ├── scene/Grid3D.tsx       # 3D scene (R3F)
        ├── ui/Controls.tsx        # Glassmorphism control panel
        └── App.tsx
```

---

## The Three Algorithms

| Property | **Dijkstra** | **A\*** | **Greedy BFS** |
|---|---|---|---|
| Priority | `g` (cost so far) | `f = g + h` | `h` (estimate to goal) |
| Uses heuristic? | No | Yes (Manhattan) | Yes (Manhattan) |
| Complete? | Yes | Yes | Yes |
| Optimal? | **Yes** | **Yes** (with admissible h) | **No** |
| Visualization | Symmetric diamond | Diamond stretched toward goal | Beelines toward goal |
| When best | All edges equal weight, no goal info | Best general-purpose | Open space, accept suboptimal |

**Manhattan distance** is admissible on a 4-neighbour grid because it never overestimates the real cost — that is what makes A\* optimal here.

---

## Color Legend (3D Scene)

| Color | Meaning |
|---|---|
| 🟩 Green | Start |
| 🟥 Red | Goal |
| ⬛ Dark | Wall |
| 🟨 Yellow | Frontier (discovered, not yet expanded) |
| 🟦 Cyan | Visited (already expanded) |
| 🟪 Purple | Final path |

---

## Live Metrics

Shown in the control panel:

- **Visited** — number of cells the algorithm fully expanded.
- **Frontier** — cells discovered but never expanded.
- **Path** — length of the final path.
- **Time** — wall-clock time (includes the artificial `DelayMs` throttle).

To compare algorithms fairly: set **Delay = 0**, click **Randomize** once, then switch the algorithm dropdown and hit **Run** — same maze, three results.

---

## Controls

| Action | Effect |
|---|---|
| Click a cell | Toggle wall |
| Shift + Click | Move start |
| Alt + Click | Move goal |
| Drag | Orbit camera |
| Scroll | Zoom |
| **Run** | Solve current maze |
| **Randomize** | Generate new random maze |
| **Clear** | Remove all walls |
| **Reset** | Clear the run animation only |

---

## Running Locally

```powershell
# Terminal 1 — backend
cd MazeApi
dotnet run

# Terminal 2 — frontend
cd maze-web
npm install
npm run dev
```

Open <http://localhost:5173>.

> If you see TLS errors on the SignalR connection, run once: `dotnet dev-certs https --trust`.

---

## Likely Discussion Questions (مناقشة)

**Q1. Why is A\* optimal but Greedy is not?**
A\* uses `f = g + h`. The `g` part keeps it honest about the cost paid so far, so when an admissible `h` is added it never commits to a worse path. Greedy uses only `h`, so once it picks a direction it never reconsiders — a wall can force a long detour that A\* would have avoided.

**Q2. What is an admissible heuristic?**
One that **never overestimates** the true remaining cost. Manhattan distance on a 4-neighbour grid is admissible because the real path needs *at least* `|Δrow| + |Δcol|` steps.

**Q3. Why a `closed` set in A\* and Dijkstra but not Greedy?**
In Dijkstra/A\* the *first* time a cell is dequeued its cost is provably optimal, so we lock it. In Greedy there is no cost guarantee at all; we just track `visited` to avoid infinite loops.

**Q4. Why `IAsyncEnumerable` instead of returning a list of steps?**
It lets us **stream** results. The algorithm yields one step, the hub forwards it immediately, the browser animates it — all before the search finishes. No memory buildup for huge grids.

**Q5. Why SignalR and not REST polling?**
Polling would be slow and chatty. SignalR holds a single WebSocket and pushes events the instant they happen — perfect for hundreds of micro-events per second.

**Q6. What is the time complexity?**
For all three with a binary-heap priority queue: **O(E log V)** where V is the number of cells and E is the number of edges (≤ 4V). Space: **O(V)**.

**Q7. Why use `PriorityQueue<TElement, TPriority>` and not decrease-key?**
.NET's built-in `PriorityQueue` does not support decrease-key efficiently. We instead **re-enqueue** a cell with a new lower priority and skip stale entries when we pop them (the `closed` check). This is standard practice and asymptotically equivalent.

**Q8. Why is the frontend 3D?**
Cell **height** carries extra information at a glance: walls are tall, the path "rises" above visited cells, and the algorithm's expansion looks like a wave. It makes the difference between the algorithms visually obvious during the demo.

**Q9. How would you extend this?**
- Add diagonal movement (heuristic becomes Chebyshev/octile).
- Weighted cells (mud/water) — A\* and Dijkstra adapt naturally; Greedy still ignores cost.
- Bidirectional A\* for further speed-ups.
- Maze generators (recursive backtracking, Prim's) instead of pure random walls.

**Q10. Why Zustand and not Redux/Context?**
Redux is heavy boilerplate; Context re-renders every consumer on every change. Zustand has a one-line store and **selector-based** subscriptions, so animating one cell doesn't re-render the other 599.
