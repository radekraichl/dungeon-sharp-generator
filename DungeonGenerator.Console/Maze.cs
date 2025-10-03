namespace DungeonGenerator;

internal class Maze(Grid grid)
{
    private readonly Grid _grid = grid;
    private readonly List<Cell> _cells = [];

    public void Add()
    {
        // Create cells
        for (int y = 0; y < _grid.Height; y++)
        {
            for (int x = 0; x < _grid.Width; x++)
            {
                var tile = _grid[y, x];

                // Skip if the tile is floor
                if (tile.Type == Tile.TileType.Floor)
                    continue;

                // Skip if the tile has neighboring floor tiles (buffer around rooms)
                if (_grid.CountNeighbours8(x, y, t => t.Type == Tile.TileType.Floor) > 0)
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

            cell.North = (y > 2) ? _grid[y - 2, x].Cell : null;
            cell.South = (y < _grid.Height - 2) ? _grid[y + 2, x].Cell : null;
            cell.West = (x > 2) ? _grid[y, x - 2].Cell : null;
            cell.East = (x < _grid.Width - 2) ? _grid[y, x + 2].Cell : null;
        }

        // Remove all isolated cells (cells with no neighbors) and convert them to corridors
        _cells.RemoveAll(c =>
        {
            bool isolated = c.Neighbors.Count == 0;
            if (isolated)
                _grid.SetTile(c.Position, Tile.TileType.Corridor); // convert isolated cell to corridor
            return isolated; // return true to remove the cell from the list
        });

        while (_cells.Any(c => c.Links.Count == 0))
        {
            RecursiveBacktracker.Maze(this);
        }

        for (int y = 0; y < _grid.Height; y++)
        {
            for (int x = 0; x < _grid.Width; x++)
            {
                var tile = _grid[y, x];

                if (tile.Cell != null && tile.Cell.Links.Count > 0)
                {
                    // Mark this cell as part of the maze (make it walkable)
                    _grid[y, x].Type = Tile.TileType.Corridor;
                    // Iterate through all linked neighbors of this cell
                    foreach (var linkedCell in tile.Cell.Links)
                    {
                        // Calculate wall position between current and linked cell
                        int wallY = (tile.Cell.Position.Y + linkedCell.Position.Y) / 2;
                        int wallX = (tile.Cell.Position.X + linkedCell.Position.X) / 2;
                        // Carve the wall = make it a Floor
                        if (wallY >= 0 && wallY < _grid.Height && wallX >= 0 && wallX < _grid.Width)
                        {
                            _grid[wallY, wallX].Type = Tile.TileType.Corridor;
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
