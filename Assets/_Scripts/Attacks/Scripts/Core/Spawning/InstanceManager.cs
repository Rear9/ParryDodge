using UnityEngine;
using System.Collections;
public class InstanceManager : MonoBehaviour
{
    public static InstanceManager Instance { get; private set; }

    [Header("Registered Attack Prefabs")]
    [Tooltip("All attack prefabs that can be spawned in waves.")]
    public GameObject[] attackPrefabs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    // Spawns an attack prefab from the registered list.
    public GameObject SpawnAttack(int index, Vector2 position, Quaternion rotation)
    {
        if (index < 0 || index >= attackPrefabs.Length)
        {
            Debug.LogWarning($"Attack index {index} out of range!");
            return null;
        }

        return Instantiate(attackPrefabs[index], position, rotation);
    }


    // Spawns a specific attack prefab.
    public GameObject SpawnAttack(GameObject attackPrefab, Vector2 position, Quaternion rotation)
    {
        if (!attackPrefab)
        {
            Debug.LogWarning("Null attack prefab!");
            return null;
        }

        return Instantiate(attackPrefab, position, rotation);
    }
}
