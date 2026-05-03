namespace MazeApi.Models;

/// <summary>
/// Payload sent from the browser describing the grid and the algorithm to run.
/// </summary>
public record GridRequest(
    int Rows,
    int Cols,
    Cell Start,
    Cell Goal,
    List<Cell> Walls,
    string Algorithm,
    int DelayMs
);
