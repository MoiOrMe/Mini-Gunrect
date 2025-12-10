using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    public bool IsReady { get; private set; } = false;

    [Header("Bullet Pool Settings")]
    [SerializeField] private ProjectileScript projectilePrefab;
    [SerializeField] private int poolSize = 20;

    private Queue<ProjectileScript> projectilePool = new Queue<ProjectileScript>();

    [Header("Target Pool Settings")]
    [SerializeField] private string targetAddress = "Target_Prefab";
    [SerializeField] private int targetPoolSize = 5;
    [SerializeField] private Transform[] targetSpawnPoints;

    private Queue<TargetScript> targetPool = new Queue<TargetScript>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    IEnumerator Start()
    {
        InitializeProjectilePool();

        yield return StartCoroutine(InitializeTargetPoolAsync());

        IsReady = true;
        Debug.Log("ObjectPoolManager : Pools initialisés et prêts.");
    }

    private void InitializeProjectilePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            ProjectileScript newProjectile = Instantiate(projectilePrefab, transform);
            newProjectile.gameObject.SetActive(false);
            projectilePool.Enqueue(newProjectile);
        }
    }

    private IEnumerator InitializeTargetPoolAsync()
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(targetAddress);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedPrefab = handle.Result;
            Debug.Log($"ObjectPoolManager : Prefab Target chargé ({loadedPrefab.name}) pour la plateforme.");

            for (int i = 0; i < targetPoolSize; i++)
            {
                GameObject newObj = Instantiate(loadedPrefab, transform);
                TargetScript newTarget = newObj.GetComponent<TargetScript>();

                if (newTarget != null)
                {
                    newObj.SetActive(false);
                    targetPool.Enqueue(newTarget);
                }
                else
                {
                    Debug.LogError("Le prefab chargé ne contient pas le composant TargetScript !");
                }
            }
        }
        else
        {
            Debug.LogError("ObjectPoolManager : Impossible de charger le prefab Target via Addressables !");
        }
    }

    public ProjectileScript GetProjectile()
    {
        if (projectilePool.Count == 0)
        {
            ProjectileScript newProjectile = Instantiate(projectilePrefab, transform);
            return newProjectile;
        }

        ProjectileScript projectile = projectilePool.Dequeue();
        return projectile;
    }

    public void ReturnProjectile(ProjectileScript projectile)
    {
        projectile.transform.SetParent(transform);
        projectilePool.Enqueue(projectile);
    }

    public TargetScript GetTarget(Vector3 position, Quaternion rotation)
    {
        if (targetPool.Count == 0)
        {
            Debug.LogWarning("Target Pool vide ! Augmentez la taille du pool dans l'inspecteur.");
            return null;
        }

        TargetScript target = targetPool.Dequeue();
        target.ResetTargetState(position, rotation);
        return target;
    }

    public void ReturnTarget(TargetScript target)
    {
        target.gameObject.SetActive(false);
        target.transform.SetParent(transform);
        targetPool.Enqueue(target);
    }

    public Transform[] GetSpawnPoints()
    {
        return targetSpawnPoints;
    }
}