using UnityEngine;


public class SoundOut : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] public float noiseRadius = 5f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // lo que escuha el jug
            audioSource.Play();

            // Escuchan los enemigos
            NoiseManager.MakeNoise(transform.position, noiseRadius);
            Debug.Log("Sonido.MP5");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, noiseRadius);
    }
}
