using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public float waveStartDelay = 1f;
    public float timeBetweenAttacks = 2f;

    [Header("Attacks To Spawn")]
    public string[] attackNames;

    [Header("Spawn Positions")]
    public Transform[] spawnPoints; // empties to spawn attacks from
    private Transform _lastSpawn;

    private void Start()
    {
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        yield return new WaitForSeconds(waveStartDelay);
        while (true)
        {
            SpawnWave();
            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }
    private void SpawnWave()
    {
        string attackToSpawn = attackNames[Random.Range(0, attackNames.Length)];
        Transform spawn = GetRandomSpawnPoint();
        GameObject attackObj = AttackPoolManager.Instance.SpawnFromPool(attackToSpawn, spawn.position, Quaternion.identity);
        if (attackObj == null) return;
        
        if (attackObj.TryGetComponent(out EnemyAttackCore core))
        {
            core.SetPoolKey(attackToSpawn);
        }
        
        if (attackObj.TryGetComponent(out InstantLineAttack attack))
        {
            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
            attack.InitAttack(player);
        }
    }
    
    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Length == 1) return spawnPoints[0];
    
        Transform spawn;
        do
        {
            spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        } while (spawn == _lastSpawn);
    
        _lastSpawn = spawn;
        return spawn;
    }
}
