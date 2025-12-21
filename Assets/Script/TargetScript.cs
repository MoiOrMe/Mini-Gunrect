using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

public class TargetScript : MonoBehaviour
{
    [Header("Tag du Projectile")]
    [SerializeField] private string projectileTag = "Projectile";

    [Header("Visual Effects (Addressables)")]
    [SerializeField] private string destructionAddress = "Impact_FX";

    [SerializeField] private Transform impactPoint;

    public int SpawnPointIndex { get; set; } = -1;

    private TargetGameManager gameManager;

    public int TargetID { get; set; }

    void Awake()
    {
        gameManager = FindObjectOfType<TargetGameManager>();

        if (gameManager == null)
        {
            Debug.LogError("TargetScript: TargetGameManager non trouvé dans la scène. La cible ne peut pas fonctionner !");
        }
    }

    public void HitAndReturnToPool()
    {
        if (gameManager != null)
        {
            gameManager.StartTargetHitFX(
                destructionAddress,
                impactPoint.position,
                transform.rotation
            );
        }

        gameManager.TargetHit(this);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(projectileTag))
        {
            HitAndReturnToPool();
        }
    }

    public void ResetTargetState(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        gameObject.SetActive(true);
    }
}