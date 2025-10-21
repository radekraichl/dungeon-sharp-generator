namespace DungeonGenerator;

internal class Maze(Dungeon grid)
{
    private readonly Dungeon _dungeon = grid;
    private readonly List<Cell> _cells = [];

    public void Add()
    {
        // Create cells
        for (int y = 0; y < _dungeon.Height; y++)
        {
            for (int x = 0; x < _dungeon.Width; x++)
            {
                var tile = _dungeon.Grid[y, x];

                // Skip if the tile is floor
                if (tile.Type == Tile.TileType.Floor)
                    continue;

                // Skip if the tile has neighboring floor tiles (buffer around rooms)
                if (_dungeon.CountNeighbours8(x, y, t => t.Type == Tile.TileType.Floor) > 0)
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

            cell.North = (y > 2) ? _dungeon.Grid[y - 2, x].Cell : null;
            cell.South = (y < _dungeon.Height - 2) ? _dungeon.Grid[y + 2, x].Cell : null;
            cell.West = (x > 2) ? _dungeon.Grid[y, x - 2].Cell : null;
            cell.East = (x < _dungeon.Width - 2) ? _dungeon.Grid[y, x + 2].Cell : null;
        }

        // Remove all isolated cells (cells with no neighbors) and convert them to corridors
        _cells.RemoveAll(c =>
        {
            bool isolated = c.Neighbors.Count == 0;
            if (isolated)
                _dungeon.SetTile(c.Position, Tile.TileType.CorridorMaze); // convert isolated cell to corridor
            return isolated; // return true to remove the cell from the list
        });

        while (_cells.Any(c => c.Links.Count == 0))
        {
            RecursiveBacktracker.Maze(this);
        }

        for (int y = 0; y < _dungeon.Height; y++)
        {
            for (int x = 0; x < _dungeon.Width; x++)
            {
                var tile = _dungeon.Grid[y, x];

                if (tile.Cell != null && tile.Cell.Links.Count > 0)
                {
                    // Mark this cell as part of the maze (make it walkable)
                    _dungeon.Grid[y, x].Type = Tile.TileType.CorridorMaze;
                    // Iterate through all linked neighbors of this cell
                    foreach (var linkedCell in tile.Cell.Links)
                    {
                        // Calculate wall position between current and linked cell
                        int wallY = (tile.Cell.Position.Y + linkedCell.Position.Y) / 2;
                        int wallX = (tile.Cell.Position.X + linkedCell.Position.X) / 2;
                        // Carve the wall = make it a Floor
                        if (wallY >= 0 && wallY < _dungeon.Height && wallX >= 0 && wallX < _dungeon.Width)
                        {
                            _dungeon.Grid[wallY, wallX].Type = Tile.TileType.CorridorMaze;
                        }
                    }
                }
            }
        }
    }

    public Cell RandomCell()
    {
        if (_cells.Count == 0)
            return null;

        int index = Rand.GetInt(0, _cells.Count);
        return _cells[index];
    }
}
