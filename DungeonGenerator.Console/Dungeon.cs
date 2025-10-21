using System.Drawing;
using System.Text;

namespace DungeonGenerator;

public class Dungeon
{
    public Tile[,] Grid => _grid;
    private readonly Tile[,] _grid;
    private readonly Maze _maze;
    private readonly RoomManager _roomManager;

    public int Width { get; }
    public int Height { get; }

    public Dungeon(int width, int height)
    {
        Width = width;
        Height = height;

        _grid = new Tile[height, width]; // 2D pole (řádky, sloupce)

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                _grid[y, x] = new Tile(new Point(x, y), Tile.TileType.Wall);
            }
        }

        _maze = new Maze(this);
        _roomManager = new RoomManager(this);
    }

    public Dungeon AddMaze()
    {
        _maze.Add();
        return this;
    }

    public Dungeon CraveRooms(int min, int max, int numberOfAttempts)
    {
        _roomManager.CraveRooms(min, max, numberOfAttempts);
        return this;
    }

    public Dungeon AddConnectors()
    {
        _roomManager.AddConnectors();
        return this;
    }

    public Dungeon ConnectRooms()
    {
        _roomManager.ConnectRooms();
        return this;
    }

    public Dungeon ConnectLooseConnectors()
    {
        _roomManager.ConnectLooseConnectors();
        return this;
    }

    public void SetTile(Point position, Tile.TileType type, int roomNumber = -1)
    {
        _grid[position.Y, position.X].Type = type;
        _grid[position.Y, position.X].RoomNumber = roomNumber;
    }

    public Tile GetTile(Point position)
    {
        return _grid[position.Y, position.X];
    }

    public Tile GetTile(int x, int y)
    {
        return _grid[y, x];
    }

    public void SealUnusedCorridors()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var tile = GetTile(x, y);
                if (tile.Type == Tile.TileType.CorridorMaze)
                {
                    tile.Type = Tile.TileType.Wall;
                }
            }
        }
    }

    public IEnumerable<Tile> GetTilesByType(Tile.TileType tileType)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var tile = _grid[y, x];
                if (tile.Type == tileType)
                {
                    yield return tile;
                }
            }
        }
    }

    public IEnumerable<Point> GetNeighbors4(Point position)
    {
        int x = position.X;
        int y = position.Y;

        if (y > 0)
            yield return new Point(x, y - 1);       // North
        if (y < Height - 1)
            yield return new Point(x, y + 1);       // South
        if (x > 0)
            yield return new Point(x - 1, y);       // West
        if (x < Width - 1)
            yield return new Point(x + 1, y);       // East
    }

    public int CountNeighbours4(int x, int y, Func<Tile, bool> predicate)
    {
        int count = 0;
        var offsets = new (int dx, int dy)[] { (0, -1), (0, 1), (-1, 0), (1, 0) };

        foreach (var (dx, dy) in offsets)
        {
            int nx = x + dx;
            int ny = y + dy;

            if (nx < 0 || nx >= Width || ny < 0 || ny >= Height)
                continue;

            if (predicate(_grid[ny, nx]))
                count++;
        }
        return count;
    }

    public int CountNeighbours8(int x, int y, Func<Tile, bool> predicate)
    {
        int count = 0;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || nx >= Width || ny < 0 || ny >= Height)
                    continue;

                if (predicate(_grid[ny, nx]))
                    count++;
            }
        }
        return count;
    }

    public override string ToString()
    {
        char[,] chars = new char[Height, Width];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                switch (_grid[y, x].Type)
                {
                    case Tile.TileType.Wall:
                        chars[y, x] = ' ';
                        break;

                    case Tile.TileType.Floor:
                        chars[y, x] = '█';
                        break;

                    case Tile.TileType.CorridorPath:
                        chars[y, x] = '▒';
                        break;

                    case Tile.TileType.CorridorMaze:
                        chars[y, x] = ' ';
                        break;

                    default:
                        chars[y, x] = '?';
                        break;
                }
            }
        }

        StringBuilder sb = new();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                sb.Append(chars[y, x]);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
