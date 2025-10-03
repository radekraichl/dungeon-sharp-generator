using System.Drawing;

namespace Dungeon;

internal class Cell(Point position)
{
    // Position in the maze
    public Point Position { get; } = position;

    // Neighboring cells
    public Cell North { get; set; }
    public Cell South { get; set; }
    public Cell East { get; set; }
    public Cell West { get; set; }

    public List<Cell> Neighbors => [.. new[] { North, South, East, West }.OfType<Cell>()];

    // Cells that are linked to this cell
    private readonly Dictionary<Cell, bool> links = [];
    public List<Cell> Links => [.. links.Keys];

    public void Link(Cell cell, bool bidirectional = true)
    {
        links[cell] = true;
        if (bidirectional)
        {
            cell.Link(this, false);
        }
    }

    public void Unlink(Cell cell, bool bidirectional = true)
    {
        links.Remove(cell);
        if (bidirectional)
        {
            cell.Unlink(this, false);
        }
    }

    public bool IsLinked(Cell cell)
    {
        if (cell == null)
        {
            return false;
        }
        return links.ContainsKey(cell);
    }
}
