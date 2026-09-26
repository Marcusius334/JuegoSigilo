using UnityEngine;

public class SonidoJUgador : MonoBehaviour
{
    
    [SerializeField] private float noiseRadius = 5f;
    [SerializeField] private float stepInterval = 0.4f;

    private AudioSource audioSource;
    private Player player;

    private float stepTimer;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (player.IsMoving)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                HacerRuido();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void HacerRuido()
    {
        // Sonido que escucha el jugador
        audioSource.Play();

        NoiseManager.MakeNoise(transform.position, noiseRadius);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, noiseRadius);
    }
}
