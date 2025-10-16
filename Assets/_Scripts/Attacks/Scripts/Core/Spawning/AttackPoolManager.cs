using System.Collections.Generic;
using UnityEngine;

public class AttackPoolManager : MonoBehaviour
{
    public static AttackPoolManager Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string key;
        public GameObject prefab;
        public int initialSize = 5;
    }

    [SerializeField] private List<Pool> pools = new();
    private readonly Dictionary<string, Queue<GameObject>> _poolDict = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        foreach (var pool in pools)
        {
            var objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.initialSize; i++)
            {
                var obj = Instantiate(pool.prefab, Vector3.zero, Quaternion.identity, null);
                obj.transform.SetParent(null); // make sure it's not parented under manager
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            _poolDict[pool.key] = objectPool;
        }
    }

    public GameObject SpawnFromPool(string key, Vector3 pos, Quaternion rot)
    {
        if (!_poolDict.TryGetValue(key, out var queue) || queue.Count == 0)
        {
            Debug.LogWarning($"{key} is empty");
            return null;
        }
        
        var spawnObj = _poolDict[key].Dequeue();
        spawnObj.transform.SetParent(null);
        if (spawnObj.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = pos;
            rb.rotation = rot.eulerAngles.z;
        }
        spawnObj.transform.SetPositionAndRotation(pos,rot);
        spawnObj.SetActive(true);
        return spawnObj;
    }

    public void ReturnToPool(string key, GameObject obj)
    {
        if (!_poolDict.TryGetValue(key, out var val))
        {
            Destroy(obj); // fallback
        }

        obj.SetActive(false);
        val.Enqueue(obj);
    }
}