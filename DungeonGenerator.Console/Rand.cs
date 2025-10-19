namespace DungeonGenerator;

public static class Rand
{
    private static Random _rnd = null;
    
    static Rand() => _rnd ??= new Random();
    public static void Init(int seed) => _rnd = new Random(seed);
    public static int GetInt(int min, int max) => _rnd.Next(min, max);
    
    public static bool OneIn(int chance)
    {
        if (chance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chance), "Chance must be > 0");
        }
        return _rnd.Next(chance) == 0;
    }

    public static T RandomElement<T>(this List<T> list)
    {
        return list[_rnd.Next(list.Count)];
    }
}
