using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private float refreshRate = 0.2f;

    void Start()
    {
        if (scoreText == null)
        {
            Debug.LogError("ScoreUIManager: TextMeshPro non assigné !");
            return;
        }

        StartCoroutine(UpdateScoreRoutine());
    }

    private IEnumerator UpdateScoreRoutine()
    {
        while (true)
        {
            if (GameplayManager.Instance != null)
            {
                scoreText.text = "Score: " + GameplayManager.Instance.GetScore().ToString();
            }

            yield return new WaitForSeconds(refreshRate);
        }
    }
}