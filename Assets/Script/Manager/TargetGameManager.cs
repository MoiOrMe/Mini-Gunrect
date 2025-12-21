using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TargetGameManager : MonoBehaviour
{
    [Header("Paramètres Spawner")]
    [SerializeField] private float spawnInterval = 1.5f;

    private Transform[] spawnPoints;

    private bool[] occupiedSpawnPoints;

    IEnumerator Start()
    {
        Debug.Log("TargetGameManager : Attente du chargement du Pool...");
        while (ObjectPoolManager.Instance == null || !ObjectPoolManager.Instance.IsReady)
        {
            yield return null;
        }

        spawnPoints = ObjectPoolManager.Instance.GetSpawnPoints();

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("TargetGameManager : Aucun point de spawn assigné !");
            yield break;
        }

        occupiedSpawnPoints = new bool[spawnPoints.Length];

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (GameplayManager.Instance != null)
            {
                int current = GameplayManager.Instance.CurrentTargets;
                int max = GameplayManager.Instance.GetMaxTargets();

                if (current < max)
                {
                    TrySpawnTargetAtFreeSpot();
                }
            }
        }
    }

    private void TrySpawnTargetAtFreeSpot()
    {
        List<int> freeIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (occupiedSpawnPoints[i] == false)
            {
                freeIndices.Add(i);
            }
        }

        if (freeIndices.Count == 0) return;

        int randomRef = Random.Range(0, freeIndices.Count);
        int finalSpawnIndex = freeIndices[randomRef];

        Transform spawnPoint = spawnPoints[finalSpawnIndex];
        TargetScript newTarget = ObjectPoolManager.Instance.GetTarget(
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (newTarget != null)
        {
            occupiedSpawnPoints[finalSpawnIndex] = true;

            newTarget.SpawnPointIndex = finalSpawnIndex;

            GameplayManager.Instance.RegisterTargetSpawn();
        }
    }


    public void TargetHit(TargetScript target)
    {
        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.AddScore(10);
            GameplayManager.Instance.RegisterTargetDespawn();
        }

        if (target.SpawnPointIndex != -1 && target.SpawnPointIndex < occupiedSpawnPoints.Length)
        {
            occupiedSpawnPoints[target.SpawnPointIndex] = false;
        }

        ObjectPoolManager.Instance.ReturnTarget(target);
    }

    public void StartTargetHitFX(string address, Vector3 position, Quaternion rotation)
    {
        StartCoroutine(LoadAndPlayEffect(address, position, rotation));
    }

    private IEnumerator LoadAndPlayEffect(string address, Vector3 position, Quaternion rotation)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, position, rotation);

        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject effectInstance = handle.Result;

            ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
        }
        else
        {
            Debug.LogError($"TargetGameManager: Impossible de charger le FX {address}");
        }
    }
}