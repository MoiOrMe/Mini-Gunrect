using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [Header("Propulsion")]
    [SerializeField] private float speed = 20f;

    private Rigidbody rb;

    public Vector3 StartPosition { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch()
    {
        if (rb == null) return;

        StartPosition = transform.position;

        gameObject.SetActive(true);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = transform.forward * speed;
    }

    public void ReturnToPool()
    {
        if (!gameObject.activeSelf) return;

        gameObject.SetActive(false);
        ObjectPoolManager.Instance.ReturnProjectile(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        ReturnToPool();
    }
}