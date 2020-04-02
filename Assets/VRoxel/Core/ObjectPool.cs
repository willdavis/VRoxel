using System.Collections.Generic;
using UnityEngine;

namespace VRoxel.Core
{
    public abstract class ObjectPool<T> : MonoBehaviour where T : Component
    {
        [SerializeField]
        public T prefab;
        public bool resize;

        private Queue<T> objects = new Queue<T>();
        void Awake() { Instance = this; }

        /// <summary>
        /// A singleton instance of the ObjectPool
        /// </summary>
        public static ObjectPool<T> Instance { get; private set; }

        /// <summary>
        /// Grab an object from the pool
        /// </summary>
        public T Get()
        {
            if (objects.Count == 0 && resize)
                AddObjects(1);

            return objects.Dequeue();
        }

        /// <summary>
        /// Return an object to the pool so it can be reused later
        /// </summary>
        public void ReturnToPool(T objectToReturn)
        {
            objectToReturn.gameObject.SetActive(false);
            objects.Enqueue(objectToReturn);
        }

        /// <summary>
        /// Instantiate more objects and add them to the pool
        /// </summary>
        public void AddObjects(int count)
        {
            if (count <= 0) { return; }
            for (int i = 0; i < count; i++)
            {
                var newObject = Instantiate(prefab);
                newObject.gameObject.SetActive(false);
                objects.Enqueue(newObject);
            }
        }
    }
}