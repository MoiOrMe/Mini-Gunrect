using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets;

public class AutoDisableFX : MonoBehaviour
{
    [Tooltip("Temps avant la disparition de l'effet")]
    [SerializeField] private float lifetime = 2f;

    void OnEnable()
    {
        StartCoroutine(DisableRoutine());
    }

    private IEnumerator DisableRoutine()
    {
        yield return new WaitForSeconds(lifetime);

        Addressables.ReleaseInstance(gameObject);
    }
}