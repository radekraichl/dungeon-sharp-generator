using System.Drawing;
using System.Runtime.CompilerServices;

namespace Dungeon;

internal class RoomManager
{
    private readonly Grid _grid;
    private readonly List<Room> _rooms = [];
    private readonly Pathfinder _pathFinder;

    public List<Room> Rooms => _rooms;

    public RoomManager(Grid grid)
    {
        _grid = grid;
        _pathFinder = new(_grid);
    }

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

    private Point? FindNearestConnector(Point fromConnector)
    {
        Point? nearest = null;
        int bestDist = int.MaxValue;

        foreach (var room in _rooms)
        {
            if (room.Merged)
                continue;

            foreach (var connector in room.Connectors)
            {
                _grid.GetTile(connector).Type = Tile.TileType.Corridor;
                int dist = _pathFinder.FindPath(fromConnector, connector);
                _grid.GetTile(connector).Type = Tile.TileType.RoomConnector;

                // If the path does not exist, use int.MaxValue
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest = connector;
                }
            }
        }
        return nearest;
    }

    //private Point? FindNearestConnector2(Point fromConnector)
    //{
    //    var queue = new Queue<(Point pos, int dist)>();
    //    var visited = new HashSet<Point>();
    //    queue.Enqueue((fromConnector, 0));
    //    visited.Add(fromConnector);

    //    while (queue.Count > 0)
    //    {
    //        var (pos, dist) = queue.Dequeue();

    //        var tile = _grid.GetTile(pos);
    //        if (tile.Type == Tile.TileType.RoomConnector)
    //        {
    //            // zjistíme, zda connector patří do jiné místnosti než startovní
    //            var room = _rooms.FirstOrDefault(r => r.Connectors.Contains(pos));
    //            var startRoom = _rooms.FirstOrDefault(r => r.Connectors.Contains(fromConnector));

    //            if (room != null && room != startRoom && !room.Merged)
    //                return pos;
    //        }

    //        foreach (var neighbor in _grid.GetNeighbors4(pos))
    //        {
    //            if (!visited.Contains(neighbor) && IsWalkableTile(_grid.GetTile(neighbor).Type))
    //            {
    //                visited.Add(neighbor);
    //                queue.Enqueue((neighbor, dist + 1));
    //            }
    //        }
    //    }

    //    return null; // nic dostupného
    //}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsWalkableTile(Tile.TileType type)
    {
        return type == Tile.TileType.CorridorPath ||
               type == Tile.TileType.Corridor;
    }

    private Point? FindNearestConnector2(Point fromConnector, List<Point> path = null)
    {
        var queue = new Queue<Point>();
        var visited = new HashSet<Point>();
        var parent = new Dictionary<Point, Point>();

        queue.Enqueue(fromConnector);
        visited.Add(fromConnector);

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
                    // běžná průchozí dlaždice
                    visited.Add(neighbor);
                    parent[neighbor] = pos;
                    queue.Enqueue(neighbor);
                }
                else if (tile.Type == Tile.TileType.RoomConnector && neighbor != fromConnector)
                {
                    // našli jsme cílový konektor
                    var room = _rooms.FirstOrDefault(r => r.Connectors.Contains(neighbor));
                    var startRoom = _rooms.FirstOrDefault(r => r.Connectors.Contains(fromConnector));

                    if (room != null && room != startRoom && !room.Merged)
                    {
                        if (path != null)
                        {
                            path.Clear();
                            var current = pos; // poslední průchozí před konektorem
                            while (current != fromConnector)
                            {
                                path.Add(current);
                                current = parent[current];
                            }
                            path.Add(fromConnector);
                            path.Reverse();

                            // a nakonec přidáme i samotný cílový konektor
                            path.Add(neighbor);
                        }

                        return neighbor;
                    }
                }
            }
        }

        return null;
    }


    public void ConnectRooms()
    {
        // Vybereme náhodně místnost
        Room currentRoom = _rooms.RandomElement();
        Stack<Room> visitedRooms = new();

        // Smyčka dokud je co spojovat
        while (_rooms.Any(r => !r.Merged))
        {
            currentRoom.Merged = true;
            Point currentConnector = new();
            Point? nearestConnectorTest = null;
            var uncheckedConnectors = new List<Point>(currentRoom.Connectors);
            var path = new List<Point>();

            while (uncheckedConnectors.Count > 0 && nearestConnectorTest == null)
            {
                // Vezmeme náhodný konektor z nezkoušených
                currentConnector = uncheckedConnectors.RandomElement();
                // Odebereme ho, aby se už nezopakoval
                uncheckedConnectors.Remove(currentConnector);
                // Najdeme nejbližší konektor nějaké nemergnuté místnosti
                nearestConnectorTest = FindNearestConnector2(currentConnector, path);
            }

            // Když cesta neexistuje
            if (nearestConnectorTest == null)
            {
                currentRoom = visitedRooms.Pop();
                continue;
            }
            visitedRooms.Push(currentRoom);

            Point nearestConnector = (Point)nearestConnectorTest;

            // Najdi tu místnost, které konektor patří
            Room targetRoom = _rooms.First(r => r.Connectors.Contains(nearestConnector));
            targetRoom.Merged = true;

            // Smaž počáteční a koncový konektor
            //targetRoom.Connectors.Remove(currentConnector);
            //targetRoom.Connectors.Remove(nearestConnector);

            // TODO
            //targetRoom.RemoveConnectors(50);

            // Označ začátek i konec chodby
            //_grid.GetTile(currentConnector).Type = Tile.TileType.CorridorPath;
            //_grid.GetTile(nearestConnector).Type = Tile.TileType.CorridorPath;

            // Vygeneruj cestu pomocí pathfindingu
            //var path = new List<Point>();
            //FindNearestConnector2(currentConnector, path);
            //_pathFinder.FindPath(currentConnector, nearestConnector, path);


            Console.WriteLine($"Room   : {currentRoom.ID}");
            Console.WriteLine($"Current: {currentConnector}");
            Console.WriteLine($"Nearest: {nearestConnector}");

            foreach (var point in path)
            {
                _grid.GetTile(point).Type = Tile.TileType.CorridorPath;
            }

            // Z target room udelama current room
            currentRoom = targetRoom;
        }
    }
}
