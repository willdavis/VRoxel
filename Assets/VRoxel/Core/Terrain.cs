public class Terrain
{
    private int _seed;
    private FastNoise _noise;

    public Terrain(int seed)
    {
        _seed = seed;
        _noise = new FastNoise(seed);
    }

    /// <summary>
    /// Calculates the height of the terrain at (x,z) with the given noise scale
    /// </summary>
    public int GetHeight(int x, int z, float scale)
    {
        return (int)_noise.GetPerlin(x / scale, z / scale);
    }
}
