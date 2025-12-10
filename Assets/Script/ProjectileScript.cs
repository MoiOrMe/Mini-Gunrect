using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [Header("Propulsion")]
    [SerializeField] private float speed = 20f;

    private Rigidbody rb;

    [Header("Destruction")]
    [SerializeField] private float lifetime = 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch()
    {
        if (rb == null) return;

        gameObject.SetActive(true);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.linearVelocity = transform.forward * speed;

        Invoke(nameof(ReturnToPool), lifetime);
    }

    public void ReturnToPool()
    {
        gameObject.SetActive(false);

        ObjectPoolManager.Instance.ReturnProjectile(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        ReturnToPool();
    }
}