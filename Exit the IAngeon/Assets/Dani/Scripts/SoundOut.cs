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
            // el sonido que detecta el jugador
            audioSource.Play();

            // Ruido que detectan los enemigos
            NoiseManager.MakeNoise(transform.position, noiseRadius);
        }
    }
}
