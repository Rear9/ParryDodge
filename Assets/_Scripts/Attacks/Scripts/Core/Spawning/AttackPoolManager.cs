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

    [SerializeField] private List<Pool> pools = new List<Pool>();
    private readonly Dictionary<string, Queue<GameObject>> _poolDict = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize each pool
        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            _poolDict.Add(pool.key, objectPool);
        }
    }

    public GameObject SpawnFromPool(string key, Vector3 position, Quaternion rotation)
    {
        if (_poolDict[key].Count == 0)
        {
            Debug.LogWarning($"Pool '{key}' is empty, make sure objects are being returned to the pool.");
            return null;
        }
        
        GameObject objectToSpawn = _poolDict[key].Dequeue();
        
        if (objectToSpawn.TryGetComponent(out Rigidbody2D rb))
        {
            rb.position = position;
            rb.rotation = rotation.eulerAngles.z;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        objectToSpawn.transform.SetPositionAndRotation(position,rotation);
        objectToSpawn.SetActive(true);
        return objectToSpawn;
    }

    public void ReturnToPool(string key, GameObject obj)
    {
        obj.SetActive(false);
        if (_poolDict.TryGetValue(key, out var value))
        {
            value.Enqueue(obj);
        }
        else
        {
            Destroy(obj); // fallback
        }
    }
}