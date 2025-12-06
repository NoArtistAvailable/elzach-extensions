using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace elZach.Common
{
    public class Pool : MonoBehaviour
    {
        public static Pool Instance;

        Dictionary<GameObject, List<GameObject>> pools = new Dictionary<GameObject, List<GameObject>>();
        Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();

        static bool isQuitting;
        
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void InitOnPlayMode()
        {
            UnityEditor.EditorApplication.playModeStateChanged += (state) =>
            {
                isQuitting = state switch
                {
                    UnityEditor.PlayModeStateChange.ExitingPlayMode => true,
                    UnityEditor.PlayModeStateChange.EnteredPlayMode => false,
                    _ => isQuitting
                };
            };
        }        
#endif

        private void Awake()
        {
            if (Instance)
            {
                Debug.LogWarning("[Pool] Multiple Instances of object pool in scene.");
                return;
            }
            Instance = this;
            Application.quitting += () =>
            {
                isQuitting = true;
                pools.Clear();
                spawnedObjects.Clear();
                Instance = null;
            };
        }


        public static GameObject Spawn(GameObject prefab, Transform parent)
        {
            if (!Instance)
            {
                Debug.Log("[Pool] No Objectpool in active Scene, but pooling was called -> creating pool.");
                GameObject poolGo = new GameObject("ObjectPool");
                Instance = poolGo.AddComponent<Pool>();
                //return Instantiate(prefab, parent);
            }
            GameObject clone;
            if (!Instance.pools.ContainsKey(prefab))
                Instance.pools.Add(prefab, new List<GameObject>());
            List<GameObject> cache = Instance.pools[prefab];
            if (cache.Count == 0)
            {
                clone = GameObject.Instantiate(prefab, parent);
                Instance.spawnedObjects.Add(clone, prefab);
            }
            else
            {
                clone = cache[0];
                clone.transform.SetParent(parent);
                clone.SetActive(true);
                cache.RemoveAt(0);
            }
            return clone;
        }

        public static T SpawnAndStart<T>(T prefab, Transform parent = null)
        {
            var monoBehaviour = prefab as MonoBehaviour;
            return SpawnAndStart(monoBehaviour.gameObject, parent).GetComponent<T>();
        }

        public static GameObject SpawnAndStart(GameObject prefab, Transform parent = null)
        {
            GameObject clone = Spawn(prefab, parent);
            clone.SendMessage("Start");
            return clone;
        }

        public static GameObject SpawnAndReset(GameObject prefab, Transform parent, Component[] toReset)
        {
            GameObject clone = Spawn(prefab, parent);
            foreach (Component component in toReset)
            {
                var cloneComponent = clone.GetComponent(component.GetType());
                cloneComponent.SendMessage("Reset", component);
            }
            clone.SendMessage("Start");
            return clone;
        }

        public static GameObject SpawnAndReset(GameObject prefab, Transform parent, Component toReset)
        {
            GameObject clone = Spawn(prefab, parent);
            var cloneComponent = clone.GetComponent(toReset.GetType());
            cloneComponent.SendMessage("Reset", toReset);
            clone.SendMessage("Start");
            return clone;
        }

        public static void Despawn(GameObject go)
        {
            if ((!Instance || !Instance.spawnedObjects.ContainsKey(go) || isQuitting) && Application.isPlaying)
            {
                Destroy(go);
                return;
            }
            List<GameObject> cache = Instance.pools[Instance.spawnedObjects[go]];
            cache.Add(go);
            go.transform.SetParent(Instance.transform);
            go.SetActive(false);
        }
    }
}
