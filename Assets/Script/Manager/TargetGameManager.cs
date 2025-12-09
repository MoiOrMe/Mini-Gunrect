using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TargetGameManager : MonoBehaviour
{
    [Header("Réapparition")]
    [SerializeField] private float respawnDelay = 3f;

    private List<TargetScript> activeTargets = new List<TargetScript>();
    private List<TargetScript> hitTargets = new List<TargetScript>();

    private Transform[] spawnPoints;
    private bool isRespawning = false;

    void Start()
    {
        spawnPoints = ObjectPoolManager.Instance.GetSpawnPoints();

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                TargetScript targetToSpawn = ObjectPoolManager.Instance.GetTarget(
                    spawnPoints[i].position,
                    spawnPoints[i].rotation
                );

                targetToSpawn.TargetID = i;
                activeTargets.Add(targetToSpawn);
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
}