using Dungeon;
using System.Diagnostics;

int seed = 18;

while (true)
{
    var sw = Stopwatch.StartNew();

    Rand.Init(seed);
    Grid grid = new(161, 41);
    //Grid grid = new(31, 21);
    RoomManager rooms = new(grid);
    rooms.CraveRooms(5, 11, 100);
    //rooms.CraveRooms(5, 7, 4);
    grid.AddMaze();
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

    //seed++;
    //Thread.Sleep(100);
}
