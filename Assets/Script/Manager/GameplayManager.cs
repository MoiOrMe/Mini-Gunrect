using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private int currentScore = 0;

    [Header("Target Settings")]
    [SerializeField] private int maxTargets = 4;
    public int CurrentTargets { get; private set; } = 0;

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

    public void AddScore(int amount)
    {
        currentScore += amount;
    }

    public int GetScore()
    {
        return currentScore;
    }

    public void RegisterTargetSpawn()
    {
        CurrentTargets++;
    }

    public void RegisterTargetDespawn()
    {
        CurrentTargets--;
        if (CurrentTargets < 0) CurrentTargets = 0;
    }

    public int GetMaxTargets()
    {
        return maxTargets;
    }
}