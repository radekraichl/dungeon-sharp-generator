using DungeonGenerator;
using System.Diagnostics;

int seed = 84;

while (true)
{
    var sw = Stopwatch.StartNew();

    Rand.Init(seed);
    Grid grid = new(101, 31);
    Maze maze = new(grid);
    RoomManager rooms = new(grid);
    rooms.CraveRooms(5, 11, 50);
    maze.Add();
    rooms.AddConnectors();
    rooms.ConnectRooms();
    sw.Stop();

    Console.WriteLine(grid);
    Console.WriteLine($"SEED: {seed}");
    Console.WriteLine($"Generation time: {sw.ElapsedMilliseconds} ms");
    //Console.WriteLine(rooms.debug);

    var key = Console.ReadKey(true);
    if (key.Key == ConsoleKey.RightArrow)
    {
        seed++;
    }
    else if (key.Key == ConsoleKey.LeftArrow)
    {
        seed--;
    }
    else if (key.Key == ConsoleKey.Enter)
    {
        break;
    }
    Console.Clear();
}
