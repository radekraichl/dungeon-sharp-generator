using System.Drawing;

namespace DungeonGenerator;

public class Tile(Point position, Tile.TileType type)
{
    public Point Position { get; set; } = position;
    public TileType Type { get; set; } = type;
    public Cell Cell { get; set; } = null;
    public int RoomNumber { get; set; } = -1;

    public enum TileType
    {
        Wall,
        Floor,
        CorridorMaze,
        CorridorPath,
    }
}
