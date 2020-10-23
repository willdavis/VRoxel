using System.Collections.Generic;
using VRoxel.Core.Data;

using Unity.Collections;
using UnityEngine;

namespace VRoxel.Core
{
    /// <summary>
    /// A component to manage the blocks in a world
    /// </summary>
    public class BlockManager : MonoBehaviour
    {
        /// <summary>
        /// The list of all block configurations in the world
        /// </summary>
        public List<BlockConfiguration> blocks;

        /// <summary>
        /// A reference to an empty list to use when modifying
        /// the world and there are no blocks to be ignored
        /// </summary>
        public NativeArray<byte> emptyIgnoreList { get; private set; }

        /// <summary>
        /// Returns the index of a block configuration with the given name
        /// </summary>
        public byte IndexOf(string name)
        {
            return (byte)blocks.FindIndex(
                x => x.name.ToLower() == name.ToLower()
            );
        }

        void Awake()
        {
            emptyIgnoreList = new NativeArray<byte>(0, Allocator.Persistent);
        }

        void OnDestroy()
        {
            emptyIgnoreList.Dispose();
        }
    }
}
