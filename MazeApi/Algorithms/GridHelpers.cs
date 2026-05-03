using MazeApi.Models;

namespace MazeApi.Algorithms;

/// <summary>
/// Helper methods for working with the grid, such as finding neighbors and calculating Manhattan distance.
/// </summary>
public static class GridHelpers
{
    private static readonly (int dr, int dc)[] Directions =
        { (-1, 0), (1, 0), (0, -1), (0, 1) };

    /// <summary>
    /// Returns the valid neighboring cells of a given cell, considering the grid boundaries and walls.
    /// </summary>
    /// <param name="cell">The cell for which to find neighbors.</param>
    /// <param name="rows">The number of rows in the grid.</param>
    /// <param name="cols">The number of columns in the grid.</param>
    /// <param name="walls">A set of cells representing walls.</param>
    /// <returns>An enumerable of valid neighboring cells.</returns>
    public static IEnumerable<Cell> Neighbors(Cell cell, int rows, int cols, HashSet<(int, int)> walls)
    {
        foreach (var (deltaRow, deltaCol) in Directions)
        {
            int nextRow = cell.Row + deltaRow;
            int nextCol = cell.Col + deltaCol;
            if (nextRow < 0 || nextRow >= rows || nextCol < 0 || nextCol >= cols) continue;
            if (walls.Contains((nextRow, nextCol))) continue;
            yield return new Cell(nextRow, nextCol);
        }
    }

    public static int Manhattan(Cell a, Cell b) =>
        Math.Abs(a.Row - b.Row) + Math.Abs(a.Col - b.Col);
}