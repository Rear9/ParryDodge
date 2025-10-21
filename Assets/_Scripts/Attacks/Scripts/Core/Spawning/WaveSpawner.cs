using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

[System.Serializable]
public class Wave // wave class with assignable attacks
{
    public string waveName;
    public List<WaveEntry> waveAttacks = new();
    public float waveDelay = 2f;
}

[System.Serializable]
public class WaveEntry // variables for each assignable attack
{
    public string attackName;
    public int attackCount = 1;
    public float attackDelay = 0.5f;
    public Transform spawnPoint;
}

public class WaveSpawner : MonoBehaviour
{
    [Header("Startup")]
    public float waveStartDelay = 3f;
    public Transform[] spawnPoints; // empties to spawn attacks from

    [Header("Waves")] 
    public List<Wave> waves = new();
    private Transform _lastSpawn;
    
    private void Start() { StartCoroutine(WaveRoutine()); }
    
    private IEnumerator WaveRoutine()
    {
        yield return new WaitForSeconds(waveStartDelay);

        foreach (var wave in waves)
        {
            Debug.Log($"Wave: {wave.waveName}");

            foreach (var entry in wave.waveAttacks)
            {
                for (int i = 0; i < entry.attackCount; i++)
                {
                    SpawnAttack(entry);
                    yield return new WaitForSeconds(entry.attackDelay);
                }
            }

            yield return new WaitForSeconds(wave.waveDelay);
        }
        Debug.Log("Waves complete");
    }
    private void SpawnAttack(WaveEntry entry)
    {
        if (waves.Count == 0 || spawnPoints.Length == 0) return;
        Debug.Log($"Waves active: {waves.Count}");

        Transform spawn = entry.spawnPoint != null ? entry.spawnPoint : GetRandomSpawnPoint();
        GameObject attackObj = AttackPoolManager.Instance.SpawnFromPool(entry.attackName, spawn.position, Quaternion.identity);
        if (!attackObj) return;
        
        if (attackObj.TryGetComponent(out EnemyAttackCore core))
        {
            core.SetPoolKey(entry.attackName);
        }
        
        if (attackObj.TryGetComponent(out IEnemyAttack attack))
        {
            attack.InitAttack(GameObject.FindGameObjectWithTag("Player")?.transform);
        }
    }
    
    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned in WaveSpawner!");
            return transform; // fallback to self
        }

        if (spawnPoints.Length == 1) return spawnPoints[0];
    
        Transform spawn;
        do
        {
            spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            print(spawn);
        } while (spawn == _lastSpawn);
    
        _lastSpawn = spawn;
        return spawn;
    }
}
