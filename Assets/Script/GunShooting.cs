using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(XRGrabInteractable))]
public class GunShooting : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private Transform firePoint;

    [Header("Visual Effects (Addressables)")]
    [SerializeField] private string muzzleFlashAddress = "MuzzleFlash_FX";

    private ParticleSystem currentMuzzleFlashInstance;
    private bool isLoaded = false;
    private AsyncOperationHandle<GameObject> loadHandle;

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
        loadHandle = Addressables.LoadAssetAsync<GameObject>(muzzleFlashAddress);
        loadHandle.Completed += OnMuzzleFlashLoaded;
    }

    private void OnMuzzleFlashLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject muzzleFlashObject = Instantiate(handle.Result, firePoint);
            currentMuzzleFlashInstance = muzzleFlashObject.GetComponent<ParticleSystem>();

            if (currentMuzzleFlashInstance == null)
            {
                Debug.LogError("Le Prefab chargé ne contient pas de ParticleSystem !");
            }
            isLoaded = true;
        }
        else
        {
            Debug.LogError($"Échec du chargement du Muzzle Flash Addressable : {handle.OperationException}");
        }
    }

    void OnDestroy()
    {
        if (isLoaded)
        {
            Destroy(currentMuzzleFlashInstance.gameObject);
            Addressables.Release(loadHandle);
        }
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

            if (isLoaded && currentMuzzleFlashInstance != null)
            {
                currentMuzzleFlashInstance.Play();
            }

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