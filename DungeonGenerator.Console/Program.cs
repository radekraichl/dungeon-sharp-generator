using DungeonGenerator;
using System.Diagnostics;

int seed = 58;

while (true)
{
    var sw = Stopwatch.StartNew();
    Rand.Init(seed);
    Dungeon dungeon = new(131, 41);
    dungeon.CraveRooms(5, 11, 300);
    dungeon.AddMaze();
    dungeon.AddConnectors();
    dungeon.ConnectRooms();
    dungeon.ConnectLooseConnectors();
    dungeon.SealUnusedCorridors();
    sw.Stop();
    Console.WriteLine(dungeon);
    Console.WriteLine($"SEED: {seed}");
    Console.WriteLine($"Generation time: {sw.ElapsedMilliseconds} ms");
    Console.WriteLine(dungeon._roomManager.debug);
    //Console.WriteLine((char)('0' + 28));

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
