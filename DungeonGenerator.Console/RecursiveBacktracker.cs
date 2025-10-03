namespace Dungeon;

internal class RecursiveBacktracker
{
    public static Grid Maze(Grid grid, Cell startAt = null)
    {
        startAt ??= grid.RandomCell();
        var stack = new Stack<Cell>();

        if (startAt == null)
            throw new InvalidOperationException("Start cell is null");

        stack.Push(startAt);

        while (stack.Count != 0)
        {
            var current = stack.Peek();
            var neighbors = current.Neighbors.Where(n => n.Links.Count == 0).ToList();
            if (neighbors.Count != 0)
            {
                var neighbor = neighbors[Rand.GetInt(0, neighbors.Count)];
                current.Link(neighbor);
                stack.Push(neighbor);
            }
            else
            {
                stack.Pop();
            }
        }

        return grid;
    }
}
