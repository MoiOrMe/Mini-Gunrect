using UnityEngine;

public class TargetScript : MonoBehaviour
{
    [Header("Tag du Projectile")]
    [SerializeField] private string projectileTag = "Projectile";

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem destructionEffect_PCVR;
    [SerializeField] private ParticleSystem destructionEffect_Quest;

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
        ParticleSystem effectToPlay = null;

#if UNITY_ANDROID
            effectToPlay = destructionEffect_Quest;
#elif UNITY_STANDALONE
        effectToPlay = destructionEffect_PCVR;
#else
            effectToPlay = (destructionEffect_PCVR != null) ? destructionEffect_PCVR : destructionEffect_Quest;
#endif

        if (effectToPlay != null)
        {
            Transform effectTransform = effectToPlay.transform;
            effectTransform.SetParent(null);
            effectTransform.gameObject.SetActive(true);
            effectToPlay.Play();
            Destroy(effectTransform.gameObject, effectToPlay.main.duration);
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