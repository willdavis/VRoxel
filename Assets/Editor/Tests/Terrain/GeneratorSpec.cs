using NUnit.Framework;
using VRoxel.Terrain;

namespace TerrainSpecs
{
    public class GeneratorSpec
    {
        [Test]
        public void CanGetHeight()
        {
            int seed = 0;
            Generator terrain = new Generator(seed,1,1,1);
            Assert.AreEqual(1, terrain.GetHeight(0,0));
        }
    }
}
