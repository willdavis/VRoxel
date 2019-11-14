namespace VRoxel.Terrain
{
    public class Generator
    {
        private int _seed;
        private float _noise;
        private float _scale;
        private float _offset;
        private FastNoise _fastNoise;

        public Generator(int seed, float noise, float scale, float offset)
        {
            _seed = seed;
            _noise = noise;
            _scale = scale;
            _offset = offset;
            _fastNoise = new FastNoise(seed);
        }

        /// <summary>
        /// Calculates the height of the terrain at (x,z)
        /// </summary>
        public int GetHeight(int x, int z)
        {
            return (int)(_fastNoise.GetPerlin(x / _noise, z / _noise) * _scale + _offset);
        }
    }
}