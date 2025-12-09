using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class GunShooting : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private Transform firePoint;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem muzzleFlash_PCVR;
    [SerializeField] private ParticleSystem muzzleFlash_Quest;

    [Header("Rate of Fire")]
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime = 0f;

    private XRGrabInteractable grabInteractable;

    private Collider gunCollider;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        gunCollider = GetComponent<Collider>();
    }

    void Start()
    {
        #if UNITY_ANDROID
            if (muzzleFlash_PCVR != null) muzzleFlash_PCVR.gameObject.SetActive(false);
        #elif UNITY_STANDALONE
            if (muzzleFlash_Quest != null) muzzleFlash_Quest.gameObject.SetActive(false);
        #endif
    }

    void OnEnable()
    {
        grabInteractable.activated.AddListener(OnShoot);
    }

    void OnDisable()
    {
        grabInteractable.activated.RemoveListener(OnShoot);
    }

    private void OnShoot(ActivateEventArgs arg)
    {
        if (Time.time > nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            if (muzzleFlash_Quest != null) muzzleFlash_Quest.Play();
            if (muzzleFlash_PCVR != null) muzzleFlash_PCVR.Play();

            ProjectileScript bullet = ObjectPoolManager.Instance.GetProjectile();

            if (bullet != null)
            {
                bullet.transform.position = firePoint.position;
                bullet.transform.rotation = firePoint.rotation;

                bullet.Launch();
            }
            else
            {
                Debug.LogError("Impossible d'obtenir un projectile du pool !");
            }
        }
    }
}