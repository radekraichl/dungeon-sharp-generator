using System.Drawing;
using System.Runtime.CompilerServices;

namespace Dungeon;

internal class Pathfinder
{
    private readonly int _width;
    private readonly int _height;
    private readonly Grid _grid;

    private readonly bool[,] _visited;
    private readonly Point?[,] _parent;

    // Fronta jako circular buffer
    private readonly (Point pos, int dist)[] _queue;
    private int _qHead, _qTail;

    private static readonly Point[] Directions =
    [
        new(1, 0),
        new(-1, 0),
        new(0, 1),
        new(0, -1)
    ];

    public Pathfinder(Grid grid)
    {
        _grid = grid;
        _width = grid.Width;
        _height = grid.Height;

        _visited = new bool[_height, _width];
        _parent = new Point?[_height, _width];
        _queue = new (Point pos, int dist)[_width * _height];
    }

    public int FindPath(Point start, Point goal, List<Point> path = null)
    {
        // Early bounds check
        if (start.X < 0 || start.X >= _width || start.Y < 0 || start.Y >= _height ||
            goal.X < 0 || goal.X >= _width || goal.Y < 0 || goal.Y >= _height)
            return int.MaxValue;

        var startTile = _grid[start.Y, start.X];
        var goalTile = _grid[goal.Y, goal.X];
        if (startTile == null || goalTile == null)
            return int.MaxValue;

        path?.Clear();

        if (start.X == goal.X && start.Y == goal.Y)
        {
            path?.Add(start);
            return 0;
        }

        // Reset visited + parent
        Array.Clear(_visited, 0, _visited.Length);
        Array.Clear(_parent, 0, _parent.Length);

        // Reset queue
        _qHead = 0;
        _qTail = 0;

        // Init BFS
        Enqueue(start, 0);
        _visited[start.Y, start.X] = true;

        while (_qHead != _qTail)
        {
            var (pos, dist) = Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int newX = pos.X + Directions[i].X;
                int newY = pos.Y + Directions[i].Y;

                // Bounds check
                if (newX < 0 || newX >= _width || newY < 0 || newY >= _height)
                    continue;

                if (_visited[newY, newX])
                    continue;

                if (!IsWalkableTile(_grid[newY, newX].Type))
                    continue;

                // Goal check
                if (newX == goal.X && newY == goal.Y)
                {
                    if (path != null)
                    {
                        var reversePath = new List<Point>
                        {
                            goal,
                            pos
                        };

                        var current = pos;
                        while (_parent[current.Y, current.X] is Point p)
                        {
                            current = p;
                            reversePath.Add(current);
                        }

                        reversePath.Reverse();
                        path.AddRange(reversePath);
                    }

                    return dist + 1;
                }

                _visited[newY, newX] = true;
                _parent[newY, newX] = pos;
                Enqueue(new Point(newX, newY), dist + 1);
            }
        }

        return int.MaxValue;
    }

    private void Enqueue(Point pos, int dist)
    {
        _queue[_qTail] = (pos, dist);
        _qTail = (_qTail + 1) % _queue.Length;
    }

    private (Point pos, int dist) Dequeue()
    {
        var item = _queue[_qHead];
        _qHead = (_qHead + 1) % _queue.Length;
        return item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWalkableTile(Tile.TileType type)
    {
        return type == Tile.TileType.CorridorPath || type == Tile.TileType.Corridor;
    }
}
