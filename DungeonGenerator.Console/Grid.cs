using System.Drawing;
using System.Text;

namespace Dungeon;

internal class Grid
{
    private readonly List<List<Tile>> _grid = [];
    private readonly List<Cell> _cells = [];

    public int Width { get; }
    public int Height { get; }

    public Grid(int width, int height)
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

    public void AddMaze()
    {
        // Create cells
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var tile = _grid[y][x];

                // Skip if the tile is floor
                if (tile.Type == Tile.TileType.Floor)
                    continue;

                // Skip if the tile has neighboring floor tiles (buffer around rooms)
                if (CountNeighbours8(x, y, t => t.Type == Tile.TileType.Floor) > 0)
                    continue;

                if (y % 2 == 1 && x % 2 == 1)
                {
                    // Create a cell
                    tile.Cell = new Cell(tile.Position);
                    _cells.Add(tile.Cell);
                }
            }
        }

        // Assign neighbors
        foreach (var cell in _cells)
        {
            int x = cell.Position.X;
            int y = cell.Position.Y;

            cell.North = (y > 2) ? _grid[y - 2][x].Cell : null;
            cell.South = (y < Height - 2) ? _grid[y + 2][x].Cell : null;
            cell.West = (x > 2) ? _grid[y][x - 2].Cell : null;
            cell.East = (x < Width - 2) ? _grid[y][x + 2].Cell : null;
        }

        // Remove all isolated cells (cells with no neighbors) and convert them to corridors
        _cells.RemoveAll(c =>
        {
            bool isolated = c.Neighbors.Count == 0;
            if (isolated)
                SetTile(c.Position, Tile.TileType.Corridor); // convert isolated cell to corridor
            return isolated; // return true to remove the cell from the list
        });

        while (_cells.Any(c => c.Links.Count == 0))
        {
            RecursiveBacktracker.Maze(this);
        }

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var tile = _grid[y][x];

                if (tile.Cell != null && tile.Cell.Links.Count > 0)
                {
                    // Mark this cell as part of the maze (make it walkable)
                    _grid[y][x].Type = Tile.TileType.Corridor;
                    // Iterate through all linked neighbors of this cell
                    foreach (var linkedCell in tile.Cell.Links)
                    {
                        // Calculate wall position between current and linked cell
                        int wallY = (tile.Cell.Position.Y + linkedCell.Position.Y) / 2;
                        int wallX = (tile.Cell.Position.X + linkedCell.Position.X) / 2;
                        // Carve the wall = make it a Floor
                        if (wallY >= 0 && wallY < Height && wallX >= 0 && wallX < Width)
                        {
                            _grid[wallY][wallX].Type = Tile.TileType.Corridor;
                        }
                    }
                }
            }
        }
    }
 
    public override string ToString()
    {
        bool printCells = false;
        bool printAllWalls = true;

        char[,] chars = new char[Height, Width];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                switch (_grid[y][x].Type)
                {
                    case Tile.TileType.Wall:
                        if (CountNeighbours8(x, y, t => t.Type == Tile.TileType.Floor) > 0 ||
                            CountNeighbours8(x, y, t => t.Type == Tile.TileType.Corridor) > 0 ||
                            CountNeighbours8(x, y, t => t.Type == Tile.TileType.CorridorPath) > 0)

                            chars[y, x] = '█';
                        else if (printAllWalls)
                            chars[y, x] = '█';
                        break;

                    case Tile.TileType.Floor:
                        chars[y, x] = ' ';
                        break;

                    case Tile.TileType.Door:
                        chars[y, x] = 'D';
                        break;

                    case Tile.TileType.CorridorPath:
                        chars[y, x] = '░';
                        break;

                    case Tile.TileType.Corridor:
                        chars[y, x] = ' ';
                        break;

                    case Tile.TileType.RoomConnector:
                        chars[y, x] = '█';
                        //chars[y, x] = '.';
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

    public Cell RandomCell()
    {
        if (_cells.Count == 0)
            return null;

        int index = Rand.GetInt(0, _cells.Count);
        return _cells[index];
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
}
