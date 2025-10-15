using System.Drawing;

namespace DungeonGenerator;

internal class RoomManager(Grid grid)
{
    private readonly Grid _grid = grid;
    private readonly List<Room> _rooms = [];

    public List<Room> Rooms => _rooms;

    public void CraveRooms(int min, int max, int numberOfAttempts)
    {
        for (int i = 0; i < numberOfAttempts; i++)
        {
            Room room = new(min, max, i, _grid);
            if (room.Carve())
            {
                _rooms.Add(room);
            }
        }
    }

    public void AddConnectors()
    {
        foreach (var room in _rooms)
        {
            foreach (var tile in room.GetRoomTilesWithConnectors())
            {
                if (tile.Type == Tile.TileType.Wall || tile.Type == Tile.TileType.RoomConnector)
                {
                    int posX = tile.Position.X;
                    int posY = tile.Position.Y;

                    int countFloor = _grid.CountNeighbours4(posX, posY, x => x.Type == Tile.TileType.Floor);
                    int countCorr = _grid.CountNeighbours4(posX, posY, x => x.Type == Tile.TileType.Corridor);
                    if ((countFloor == 1 && countCorr == 1) || countFloor == 2)
                    {
                        if (posX > 0 && posY > 0 && posX < _grid.Width - 1 && posY < _grid.Height - 1)
                        {
                            tile.Type = Tile.TileType.RoomConnector;
                            room.Connectors.Add(new Point(posX, posY));
                        }
                    }
                }
            }
        }
    }

    public void ConnectRooms()
    {
        // Select a random starting room
        Room currentRoom = _rooms.RandomElement();
        Stack<Room> visitedRooms = new();

        // Continue until all rooms are connected (merged)
        while (_rooms.Any(r => !r.Merged))
        {
            currentRoom.Merged = true;
            Point currentConnector = new();
            Point? targetConnector = null;
            var uncheckedConnectors = new List<Point>(currentRoom.Connectors);
            var path = new List<Point>();

            // Try all available connectors in this room until a connection is found
            while (uncheckedConnectors.Count > 0 && targetConnector == null)
            {
                // Pick a random connector from the remaining ones
                currentConnector = uncheckedConnectors.RandomElement();

                // Remove it from the list to avoid rechecking
                uncheckedConnectors.Remove(currentConnector);

                // Find the nearest connector of another unmerged room
                targetConnector = FindNearestConnector(currentConnector, path);
            }

            // No connection found — backtrack to the previous room
            if (targetConnector == null)
            {
                currentRoom = visitedRooms.Pop();
                continue;
            }

            // Mark the current room as visited (store it for possible backtracking)
            visitedRooms.Push(currentRoom);

            // Find the target room that owns the found connector
            Room targetRoom = _rooms.First(r => r.Connectors.Contains((Point)targetConnector));
            targetRoom.Merged = true;

            // Remove nearby connectors to avoid adjacent doors between rooms
            currentRoom.RemoveNearbyConnectors(currentConnector);
            targetRoom.RemoveNearbyConnectors((Point)targetConnector);

            // Remove excess connectors, but keep a few randomly for extra variety
            currentRoom.RemoveConnectors(20);

            // Carve the connecting corridor between the two rooms
            foreach (var point in path)
            {
                _grid.GetTile(point).Type = Tile.TileType.CorridorPath;
            }

            // Set the newly connected room as the current one for the next iteration
            currentRoom = targetRoom;
        }

        // Clean up remaining connectors from the last connected room
        currentRoom.RemoveConnectors(20);
    }

    public void ConnectLooseConnectors()
    {
        foreach (var room in _rooms)
        {
            // Copy the list because it may be modified during iteration
            var looseConnectors = new List<Point>(room.Connectors);

            foreach (var connector in looseConnectors)
            {
                var path = new List<Point>();
                var nearestCorridor = FindNearestCorridorPath(connector, path);

                // Skip if no corridor was found
                if (nearestCorridor is null)
                    continue;

                // Carve the corridor path from the connector to the nearest corridor
                foreach (var point in path)
                {
                    var tile = _grid.GetTile(point);
                    tile.Type = Tile.TileType.CorridorPath;
                }

                // Remove the connector after successful connection
                room.Connectors.Remove(connector);
            }
        }
    }

    private Point? FindNearestConnector(Point start, List<Point> path = null)
    {
        var queue = new Queue<Point>();
        var visited = new HashSet<Point>();
        var parent = new Dictionary<Point, Point>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var pos = queue.Dequeue();

            foreach (var neighbor in _grid.GetNeighbors4(pos))
            {
                if (visited.Contains(neighbor))
                    continue;

                var tile = _grid.GetTile(neighbor);

                if (IsWalkableTile(tile.Type))
                {
                    // Regular walkable tile
                    visited.Add(neighbor);
                    parent[neighbor] = pos;
                    queue.Enqueue(neighbor);
                }
                else if (tile.Type == Tile.TileType.RoomConnector && neighbor != start)
                {
                    // Found a target connector
                    var room = _rooms.FirstOrDefault(r => r.Connectors.Contains(neighbor));
                    var startRoom = _rooms.FirstOrDefault(r => r.Connectors.Contains(start));

                    if (room != null && room != startRoom && !room.Merged)
                    {
                        if (path != null)
                        {
                            path.Clear();
                            var current = pos; // Last walkable tile before the connector
                            while (current != start)
                            {
                                path.Add(current);
                                current = parent[current];
                            }
                            path.Add(start);
                            path.Reverse();

                            // Finally, add the target connector itself
                            path.Add(neighbor);
                        }
                        return neighbor;
                    }
                }
            }
        }
        return null;
    }

    private Point? FindNearestCorridorPath(Point start, List<Point> path = null)
    {
        var queue = new Queue<Point>();
        var visited = new HashSet<Point>();
        var parent = new Dictionary<Point, Point>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var pos = queue.Dequeue();

            foreach (var neighbor in _grid.GetNeighbors4(pos))
            {
                if (visited.Contains(neighbor))
                    continue;

                var tile = _grid.GetTile(neighbor);
                if (tile == null)
                    continue;

                // Check if we found a CorridorPath
                if (tile.Type == Tile.TileType.CorridorPath && !neighbor.Equals(start))
                {
                    // Reconstruct path
                    if (path != null)
                    {
                        path.Clear();
                        var current = pos; // Last walkable position before corridor

                        // Build path backwards from pos to start
                        while (parent.ContainsKey(current))
                        {
                            path.Add(current);
                            current = parent[current];
                        }
                        path.Add(start);
                        path.Reverse();

                        // Add the corridor tile at the end
                        path.Add(neighbor);
                    }

                    return neighbor;
                }

                // Continue walking through walkable tiles
                if (IsWalkableTile(tile.Type))
                {
                    visited.Add(neighbor);
                    parent[neighbor] = pos;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return null; // No corridor path found
    }

    private static bool IsWalkableTile(Tile.TileType type)
    {
        return type == Tile.TileType.CorridorPath ||
               type == Tile.TileType.Corridor;
    }
}
