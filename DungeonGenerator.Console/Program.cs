using DungeonGenerator;
using System.Diagnostics;

int seed = 0;

while (true)
{
    var sw = Stopwatch.StartNew();
    Rand.Init(seed);
    Dungeon dungeon = new(151, 41);
    dungeon.CraveRooms(5, 11, 400)
        .AddMaze()
        .AddConnectors()
        .ConnectRooms()
        .ConnectLooseConnectors()
        .SealUnusedCorridors();
    sw.Stop();
    Console.Clear();
    Console.WriteLine(dungeon);
    Console.WriteLine($"SEED: {seed}");
    Console.WriteLine($"Generation time: {sw.ElapsedMilliseconds} ms");
    Console.WriteLine("Press the left and right arrows to change the seed.");

    var key = Console.ReadKey(true);
    if (key.Key == ConsoleKey.RightArrow)
    {
        seed++;
    }
    else if (key.Key == ConsoleKey.LeftArrow)
    {
        seed--;
    }
    else if (key.Key == ConsoleKey.Escape)
    {
        break;
    }
}
