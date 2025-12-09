using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [Header("Bullet Pool Settings")]
    [SerializeField] private ProjectileScript projectilePrefab;
    [SerializeField] private int poolSize = 20;

    private Queue<ProjectileScript> projectilePool = new Queue<ProjectileScript>();

    [Header("Target Pool Settings")]
    [SerializeField] private TargetScript targetPrefab;
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
            InitializePool();
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            ProjectileScript newProjectile = Instantiate(projectilePrefab, transform);
            newProjectile.gameObject.SetActive(false);
            projectilePool.Enqueue(newProjectile);
        }

        for (int i = 0; i < targetPoolSize; i++)
        {
            TargetScript newTarget = Instantiate(targetPrefab, transform);
            newTarget.gameObject.SetActive(false);
            targetPool.Enqueue(newTarget);
        }
    }

    public ProjectileScript GetProjectile()
    {
        if (projectilePool.Count == 0)
        {
            Debug.LogWarning("Pool vide. Création d'une nouvelle instance.");
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
            TargetScript newTarget = Instantiate(targetPrefab, transform);
            newTarget.ResetTargetState(position, rotation);
            return newTarget;
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