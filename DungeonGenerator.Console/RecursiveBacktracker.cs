namespace DungeonGenerator;

internal class RecursiveBacktracker
{
    public static void Maze(Maze maze, Cell startAt = null)
    {
        startAt ??= maze.RandomCell();
        var stack = new Stack<Cell>();

        if (startAt == null)
            throw new InvalidOperationException("Start cell is null");

        stack.Push(startAt);

        while (stack.Count != 0)
        {
            Cell current = stack.Peek();
            var neighbors = current.Neighbors.Where(n => n.Links.Count == 0).ToList();
            if (neighbors.Count != 0)
            {
                Cell neighbor = neighbors[Rand.GetInt(0, neighbors.Count)];
                current.Link(neighbor);
                stack.Push(neighbor);
            }
            else
            {
                stack.Pop();
            }
        }
    }
}
