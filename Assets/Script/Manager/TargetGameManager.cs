using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TargetGameManager : MonoBehaviour
{
    [Header("Réapparition")]
    [SerializeField] private float respawnDelay = 3f;

    private List<TargetScript> activeTargets = new List<TargetScript>();
    private List<TargetScript> hitTargets = new List<TargetScript>();

    private Transform[] spawnPoints;
    private bool isRespawning = false;

    IEnumerator Start()
    {
        Debug.Log("TargetGameManager : Attente du chargement du Pool...");
        while (ObjectPoolManager.Instance == null || !ObjectPoolManager.Instance.IsReady)
        {
            yield return null;
        }

        spawnPoints = ObjectPoolManager.Instance.GetSpawnPoints();

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                TargetScript targetToSpawn = ObjectPoolManager.Instance.GetTarget(
                    spawnPoints[i].position,
                    spawnPoints[i].rotation
                );

                if (targetToSpawn != null)
                {
                    targetToSpawn.TargetID = i;
                    activeTargets.Add(targetToSpawn);
                }
            }
        }
        else
        {
            Debug.LogError("TargetGameManager : Aucun point de spawn assigné ! Vérifiez l'ObjectPoolManager.");
        }
    }

    public void TargetHit(TargetScript target)
    {
        activeTargets.Remove(target);
        hitTargets.Add(target);

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.AddScore(10);
            GameplayManager.Instance.RegisterTargetDespawn();
        }

        ObjectPoolManager.Instance.ReturnTarget(target);

        if (activeTargets.Count == 0 && !isRespawning)
        {
            isRespawning = true;
            StartCoroutine(RespawnTargetsDelayed());
        }
    }

    private IEnumerator RespawnTargetsDelayed()
    {
        Debug.Log("Toutes les cibles sont touchées. Attente de " + respawnDelay + " secondes.");
        yield return new WaitForSeconds(respawnDelay);

        for (int i = 0; i < hitTargets.Count; i++)
        {
            if (i < spawnPoints.Length)
            {
                TargetScript targetToRespawn = ObjectPoolManager.Instance.GetTarget(
                    spawnPoints[i].position,
                    spawnPoints[i].rotation
                );

                targetToRespawn.TargetID = i;
                activeTargets.Add(targetToRespawn);
            }
        }

        hitTargets.Clear();
        isRespawning = false;
        Debug.Log("Cibles réapparues !");
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
                float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
                StartCoroutine(ReleaseEffectInstance(handle, lifetime));
            }
            else
            {
                Addressables.ReleaseInstance(handle);
            }
        }
        else
        {
            Debug.LogError($"TargetGameManager: Échec du chargement de l'impact Addressable : {handle.OperationException}");
        }
    }

    private IEnumerator ReleaseEffectInstance(AsyncOperationHandle<GameObject> handle, float delay)
    {
        yield return new WaitForSeconds(delay);
        Addressables.ReleaseInstance(handle);
    }
}