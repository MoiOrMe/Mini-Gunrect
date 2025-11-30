using UnityEngine;

public class TargetScript : MonoBehaviour
{
    [Header("Tag du Projectile")]
    [SerializeField] private string projectileTag = "Projectile";

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem destructionEffect_PCVR;
    [SerializeField] private ParticleSystem destructionEffect_Quest;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(projectileTag))
        {
            ParticleSystem effectToPlay = null;

#if UNITY_ANDROID
                effectToPlay = destructionEffect_Quest;
#elif UNITY_STANDALONE
            effectToPlay = destructionEffect_PCVR;
#else
                // En éditeur, préférer la version lourde pour un meilleur visuel de test
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

            Destroy(gameObject);
        }
    }
}