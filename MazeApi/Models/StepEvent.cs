namespace MazeApi.Models;

/// <summary>
/// One incremental update streamed from the algorithm to the UI.
/// </summary>
/// <param name="Type">"frontier" | "visit" | "path" | "done"</param>
/// <param name="Cell">Which cell this event refers to.</param>
/// <param name="G">Cost from start (when applicable).</param>
/// <param name="H">Heuristic estimate to goal (when applicable).</param>
/// <param name="F">f = g + h (when applicable).</param>
public record StepEvent(string Type, Cell Cell, int? G = null, int? H = null, int? F = null);
