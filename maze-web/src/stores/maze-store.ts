/**
 * Global UI state for the maze visualizer (powered by Zustand).
 *
 * Why Zustand instead of Redux/Context?
 *   - Tiny API, no boilerplate.
 *   - Components subscribe only to the slices they read, so re-renders stay cheap
 *     even though hundreds of cells share the same store.
 *
 * The store holds:
 *   - Grid configuration (rows, cols, start, goal, walls)
 *   - The live cell states streamed from the backend (visited / frontier / path)
 *   - Algorithm selection + animation delay
 *   - Live metrics (visited count, frontier count, path length, elapsed time)
 */
import { create } from "zustand";
import type { Cell, CellState, StepEvent } from "../types";

// We store walls and cell states keyed by "row,col" strings because
// Set/Map don't do structural equality on objects.
const key = (r: number, c: number) => `${r},${c}`;

type State = {
  rows: number; cols: number;
  start: Cell; goal: Cell;
  walls: Set<string>;
  states: Map<string, CellState>;
  algorithm: string; delayMs: number; running: boolean;
  metrics: { visited: number; frontier: number; pathLength: number; elapsedMs: number };
  _startedAt: number;
  toggleWall: (r: number, c: number) => void;
  setStart: (c: Cell) => void;
  setGoal: (c: Cell) => void;
  applyStep: (s: StepEvent) => void;
  clearRun: () => void;
  clearWalls: () => void;
  randomizeWalls: (density?: number) => void;
  setAlgo: (a: string) => void;
  setDelay: (n: number) => void;
  setRunning: (b: boolean) => void;
};

export const useMazeStore = create<State>((set) => ({
  rows: 20, cols: 20,
  start: { row: 2, col: 2 },
  goal:  { row: 17, col: 17 },
  walls: new Set(),
  states: new Map(),
  algorithm: "astar", delayMs: 20, running: false,
  metrics: { visited: 0, frontier: 0, pathLength: 0, elapsedMs: 0 },
  _startedAt: 0,

  toggleWall: (r, c) => set(s => {
    // Add or remove a wall at (r,c).
    const w = new Set(s.walls); const k = key(r, c);
    if (w.has(k)) w.delete(k); else w.add(k);
    return { walls: w };
  }),
  setStart: (c) => set({ start: c }),
  setGoal:  (c) => set({ goal: c }),

  /**
   * Apply one StepEvent coming from the backend.
   * The backend sends 4 event types:
   *   - "frontier": cell discovered, painted yellow
   *   - "visit":    cell expanded,  painted cyan (overwrites frontier)
   *   - "path":     cell on the final path, painted purple
   *   - "done":     end of stream; stop the timer and freeze metrics
   */
  applyStep: (s) => set((state) => {
    const m = new Map(state.states);
    const k = key(s.cell.row, s.cell.col);
    const metrics = { ...state.metrics };

    if (s.type === "visit") { m.set(k, "visited"); metrics.visited++; }
    else if (s.type === "frontier") { if (!m.has(k)) { m.set(k, "frontier"); metrics.frontier++; } }
    else if (s.type === "path") { m.set(k, "path"); metrics.pathLength++; }

    if (s.type === "done") {
      // Wall-clock duration of the run (includes the artificial DelayMs throttle).
      metrics.elapsedMs = state._startedAt ? performance.now() - state._startedAt : 0;
      return { states: m, running: false, metrics };
    }
    return { states: m, metrics };
  }),

  // Reset the live run (keeps walls). Also resets metrics and starts the timer.
  clearRun: () => set({ states: new Map(), metrics: { visited: 0, frontier: 0, pathLength: 0, elapsedMs: 0 }, _startedAt: performance.now() }),

  // Remove all walls AND the previous run.
  clearWalls: () => set({ walls: new Set(), states: new Map() }),

  /**
   * Generate a random maze.
   * `density` is the probability that any non-start/goal cell becomes a wall (0..1).
   * Start and goal cells are always kept open.
   */
  randomizeWalls: (density = 0.28) => set(state => {
    const walls = new Set<string>();
    const sk = key(state.start.row, state.start.col);
    const gk = key(state.goal.row, state.goal.col);
    for (let r = 0; r < state.rows; r++)
      for (let c = 0; c < state.cols; c++) {
        const k = key(r, c);
        if (k === sk || k === gk) continue;
        if (Math.random() < density) walls.add(k);
      }
    return { walls, states: new Map() };
  }),
  setAlgo: (a) => set({ algorithm: a }),
  setDelay: (n) => set({ delayMs: n }),
  setRunning: (b) => set({ running: b }),
}));
