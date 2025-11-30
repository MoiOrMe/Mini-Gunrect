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

        Destroy(gameObject, lifetime);
    }

    public void Launch()
    {
        if (rb == null) return;

        rb.linearVelocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}