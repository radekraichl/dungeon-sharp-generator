using System.Drawing;
using System.Text;

namespace DungeonGenerator;

internal class Dungeon
{
    private readonly List<List<Tile>> _grid = [];
    private readonly Maze _maze;
    public readonly RoomManager _roomManager;

    public int Width { get; }
    public int Height { get; }

    public Dungeon(int width, int height)
    {
        Width = width;
        Height = height;

        for (int y = 0; y < height; y++)    // rows
        {
            var row = new List<Tile>();
            for (int x = 0; x < width; x++) // cols
            {
                row.Add(new Tile(new Point(x, y), Tile.TileType.Wall));
            }
            _grid.Add(row);
        }

        _maze = new(this);
        _roomManager = new RoomManager(this);
    }

    // Indexer
    public Tile this[int row, int column]
    {
        get
        {
            if (row < 0 || row >= Height)
            {
                return null;
            }
            if (column < 0 || column >= Width)
            {
                return null;
            }
            return _grid[row][column];
        }
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
        _grid[position.Y][position.X].Type = type;
        _grid[position.Y][position.X].RoomNumber = roomNumber;
    }

    public Tile GetTile(Point position)
    {
        return _grid[position.Y][position.X];
    }

    public IEnumerable<Tile> GetTilesByType(Tile.TileType tileType)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var tile = _grid[y][x];
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

            if (predicate(_grid[ny][nx]))
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

                if (predicate(_grid[ny][nx]))
                    count++;
            }
        }

        return count;
    }

    public override string ToString()
    {
        bool printCells = false;
        bool printAllWalls = false;

        char[,] chars = new char[Height, Width];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                switch (_grid[y][x].Type)
                {
                    case Tile.TileType.Wall:
                        if (CountNeighbours8(x, y, t => t.Type == Tile.TileType.Floor) > 0 ||
                            CountNeighbours8(x, y, t => t.Type == Tile.TileType.CorridorMaze) > 0 ||
                            CountNeighbours8(x, y, t => t.Type == Tile.TileType.CorridorPath) > 0)

                            chars[y, x] = '█';
                        else if (printAllWalls)
                            chars[y, x] = '█';
                        else
                            chars[y, x] = ' ';
                        break;

                    case Tile.TileType.Floor:
                        chars[y, x] = ' ';
                        break;

                    case Tile.TileType.CorridorPath:
                        chars[y, x] = '░';
                        break;

                    case Tile.TileType.CorridorMaze:
                        chars[y, x] = ' ';
                        break;

                    case Tile.TileType.RoomConnector:
                        //chars[y, x] = '█';
                        chars[y, x] = '.';
                        break;

                    default:
                        chars[y, x] = '?';
                        break;
                }

                if (_grid[y][x].Cell != null && printCells)
                {
                    chars[y, x] = 'c';
                }

                if (_grid[y][x].RoomNumber != -1)
                {
                    chars[y, x] = (char)('0' + _grid[y][x].RoomNumber);
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
