import { useMazeStore } from "../stores/maze-store";
import { runSolve } from "../services/maze-client";

export function Controls() {
  const { algorithm, delayMs, running, metrics, setAlgo, setDelay, clearRun, clearWalls, randomizeWalls } = useMazeStore();

  return (
    <div className="absolute top-4 left-4 backdrop-blur bg-white/10 border border-white/20
                    rounded-2xl p-4 w-72 space-y-3 shadow-xl">
      <h1 className="text-lg font-semibold">Heuristic Search</h1>

      <label className="block text-sm">Algorithm
        <select value={algorithm} onChange={e => setAlgo(e.target.value)}
                className="mt-1 w-full bg-slate-800 rounded p-1">
          <option value="astar">A*</option>
          <option value="greedy">Greedy Best-First</option>
          <option value="dijkstra">Dijkstra</option>
        </select>
      </label>

      <label className="block text-sm">Delay: {delayMs}ms
        <input type="range" min={0} max={150} value={delayMs}
               onChange={e => setDelay(+e.target.value)} className="w-full" />
      </label>

      <div className="flex gap-2">
        <button disabled={running} onClick={() => runSolve()}
                className="flex-1 bg-emerald-500 hover:bg-emerald-400 disabled:opacity-50
                           rounded px-3 py-1.5">Run</button>
      </div>

      <div className="flex gap-2">
        <button onClick={() => randomizeWalls()}
                className="flex-1 bg-amber-500 hover:bg-amber-400 rounded px-3 py-1.5">
          Randomize
        </button>
        <button onClick={clearWalls}
                className="flex-1 bg-slate-600 hover:bg-slate-500 rounded px-3 py-1.5">
          Clear
        </button>
        <button onClick={clearRun}
                className="flex-1 bg-slate-700 hover:bg-slate-600 rounded px-3 py-1.5">
          Reset
        </button>
      </div>

      <p className="text-xs text-white/70">
        Click: wall · Shift+Click: start · Alt+Click: goal · Drag: orbit
      </p>

      <div className="grid grid-cols-2 gap-2 pt-2 border-t border-white/15 text-sm">
        <Metric label="Visited"  value={metrics.visited} color="text-cyan-300" />
        <Metric label="Frontier" value={metrics.frontier} color="text-amber-300" />
        <Metric label="Path"     value={metrics.pathLength} color="text-purple-300" />
        <Metric label="Time"     value={`${metrics.elapsedMs.toFixed(0)} ms`} color="text-emerald-300" />
      </div>
    </div>
  );
}

function Metric({ label, value, color }: { label: string; value: number | string; color: string }) {
  return (
    <div className="bg-black/30 rounded-lg px-2 py-1.5">
      <div className="text-[10px] uppercase tracking-wide text-white/50">{label}</div>
      <div className={`font-mono ${color}`}>{value}</div>
    </div>
  );
}
