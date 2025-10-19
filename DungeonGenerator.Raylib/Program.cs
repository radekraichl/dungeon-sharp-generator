using Raylib_cs;
using DungeonGenerator;
using System.Diagnostics;

Raylib.InitWindow(1328, 800, "Dungeon Generator");
Raylib.SetTargetFPS(60);

Texture2D floorTex = Raylib.LoadTexture("img/floor.png");
Texture2D wallTex = Raylib.LoadTexture("img/wall.png");
Texture2D corridorTex = Raylib.LoadTexture("img/corridor.png");

int tileSize = 16;
int margin = 16;
int seed = 0;

Dungeon dungeon;
TimeSpan generationTime = TimeSpan.Zero;

// Metoda pro generování dungeonu a měření času
void GenerateDungeon()
{
    var stopwatch = Stopwatch.StartNew();

    Rand.Init(seed);
    dungeon = new Dungeon(81, 41);
    dungeon.CraveRooms(5, 11, 400)
           .AddMaze()
           .AddConnectors()
           .ConnectRooms()
           .ConnectLooseConnectors()
           .SealUnusedCorridors();

    stopwatch.Stop();
    generationTime = stopwatch.Elapsed;
}

GenerateDungeon();

while (!Raylib.WindowShouldClose())
{
    if (Raylib.IsKeyPressed(KeyboardKey.Left))
    {
        seed--;
        GenerateDungeon();
    }
    if (Raylib.IsKeyPressed(KeyboardKey.Right))
    {
        seed++;
        GenerateDungeon();
    }

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);

    for (int y = 0; y < dungeon.Height; y++)
    {
        for (int x = 0; x < dungeon.Width; x++)
        {
            var tile = dungeon.Grid[y][x];

            int drawX = x * tileSize + margin;
            int drawY = y * tileSize + margin;

            if (tile.Type == Tile.TileType.Floor)
                Raylib.DrawTexture(floorTex, drawX, drawY, Color.White);

            if (tile.Type == Tile.TileType.CorridorPath)
                Raylib.DrawTexture(corridorTex, drawX, drawY, Color.White);

            if (tile.Type == Tile.TileType.Wall)
            {
                int floorOrCorridorCount = dungeon.CountNeighbours8(x, y,
                    t => t.Type == Tile.TileType.Floor || t.Type == Tile.TileType.CorridorPath);

                if (floorOrCorridorCount > 0)
                    Raylib.DrawTexture(wallTex, drawX, drawY, Color.White);
            }
        }
    }

    int textY = dungeon.Height * tileSize + 2 * margin;
    Raylib.DrawText($"Seed: {seed}", margin, textY, 24, Color.Brown);
    Raylib.DrawText($"Generation time: {generationTime.TotalMilliseconds:F1} ms", margin, textY + 30, 24, Color.Brown);
    Raylib.DrawText("Press the left and right arrows to change the seed", margin, textY + 60, 24, Color.Brown);

    Raylib.EndDrawing();
}

Raylib.CloseWindow();
