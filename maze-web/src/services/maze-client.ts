/**
 * SignalR client wrapper.
 *
 * Responsibilities:
 *   1. Open a single persistent connection to the backend MazeHub.
 *   2. Listen for "Step" messages and forward them into the Zustand store.
 *   3. Expose `runSolve()` which sends the current grid configuration
 *      to the server and triggers the search.
 *
 * The connection is created once at module load. `withAutomaticReconnect`
 * handles transient network drops without us writing retry logic.
 */
import * as signalR from "@microsoft/signalr";
import { useMazeStore } from "../stores/maze-store";
import type { StepEvent } from "../types/step-event";

const conn = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7033/hub/maze")
    .withAutomaticReconnect()
    .build();

// Every "Step" pushed by the server lands here and updates the UI store.
// (See MazeHub.Solve which sends `Clients.Caller.SendAsync("Step", step)`.)
conn.on("Step", (s: StepEvent) =>  useMazeStore.getState().applyStep(s));

/**
 * Start a new search using the current store state (algorithm, walls, start, goal, delay).
 * Resets the previous run, then invokes the hub method `Solve`.
 */
export async function runSolve() {
    const state = useMazeStore.getState();

    // Lazy connect on first use.
    if (conn.state === signalR.HubConnectionState.Disconnected) await conn.start();

    // Clear the previous animation/metrics and start the timer.
    state.clearRun();
    state.setRunning(true);

    // Send the current grid to the backend. Walls are converted from
    // "row,col" strings back into Cell objects expected by the API.
    await conn.invoke("Solve", {
        rows: state.rows, cols: state.cols,
        start: state.start, goal: state.goal,
        walls: [...state.walls].map(k => {
            const [r, c] = k.split(",").map(Number);
            return { row: r, col: c};
        }),
        algorithm: state.algorithm,
        delayMs: state.delayMs
    });
}
