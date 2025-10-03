using System.Drawing;

namespace DungeonGenerator;

internal class Room(int min, int max, int id, Grid grid)
{
    private const int _mapMargin = 1;
    private const int _roomsMargin = 1;

    private readonly Grid _grid = grid ?? throw new ArgumentNullException(nameof(grid));

    public int ID { get; } = id;
    public bool Merged { get; set; } = false;
    public Point Position { get; private set; }
    public List<Point> Connectors { get; set; } = [];
    public int Width { get; private set; }
    public int Height { get; private set; }

    public int Left => Position.X;
    public int Right => Position.X + Width - 1;
    public int Top => Position.Y;
    public int Bottom => Position.Y + Height - 1;
    public Point Center => new(Position.X + Width / 2, Position.Y + Height / 2);

    public IEnumerable<Tile> GetRoomTilesWithConnectors()
    {
        for (int i = Top - 1; i < Top + Height + 1; i++)
        {
            for (int j = Left - 1; j < Left + Width + 1; j++)
            {
                yield return _grid[i, j];
            }
        }
    }

    public bool Carve()
    {
        Width = Rand.GetInt(min, max);
        Height = Rand.GetInt(min, max);
        if (Width % 2 == 0) Width--;
        if (Height % 2 == 0) Height--;

        int minX = _mapMargin;
        int minY = _mapMargin;
        int maxX = _grid.Width - Width - _mapMargin + 1;
        int maxY = _grid.Height - Height - _mapMargin + 1;

        int posX = Rand.GetInt(minX, maxX);
        int posY = Rand.GetInt(minY, maxY);
        if (posX % 2 == 0) posX--;
        if (posY % 2 == 0) posY--;
        Position = new Point(posX, posY);

        if (maxX <= 1 || maxY <= 1)
        {
            return false;
        }

        if (CheckCollision(Position.X, Position.Y, Width, Height))
        {
            return false;
        }

        for (int y = Top; y <= Bottom; y++)
        {
            for (int x = Left; x <= Right; x++)
            {
                _grid.SetTile(new Point(x, y), Tile.TileType.Floor, ID);
            }
        }

        return true;
    }

    bool CheckCollision(int col, int row, int width, int height)
    {
        int m = _roomsMargin;

        for (int i = row - m; i < row + height + m; i++)
        {
            for (int j = col - m; j < col + width + m; j++)
            {
                if (i < 0 || j < 0 || i > _grid.Height - 1 || j > _grid.Width - 1)
                {
                    return true;
                }

                if (_grid[i, j].Type == Tile.TileType.Floor)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void RemoveConnectors(int chance = 1)
    {
        var survivors = new List<Point>();

        foreach (var connector in Connectors)
        {
            if (Rand.OneIn(chance))
            {
                survivors.Add(connector);
            }
            else
            {
                _grid.SetTile(connector, Tile.TileType.Wall);
            }
        }

        Connectors.Clear();
        Connectors.AddRange(survivors);
    }

    public bool Intersects(Room other)
    {
        return !(Right < other.Left || Left > other.Right || Bottom < other.Top || Top > other.Bottom);
    }
}
