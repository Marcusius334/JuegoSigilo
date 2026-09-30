using UnityEngine;

public class SonidoJUgador : MonoBehaviour
{
    [SerializeField] private float walkNoiseRadius = 2f;
    [SerializeField] private float runNoiseRadius = 5f;
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
        audioSource.Play();

        float noiseRadius = player.IsRunning
            ? runNoiseRadius
            : walkNoiseRadius;

        Debug.Log("RUIDO DEL JUGADOR - Corriendo: " + player.IsRunning +
                  " - Radio: " + noiseRadius);

        NoiseManager.MakeNoise(
            transform.position,
            noiseRadius,
            player.IsRunning
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, runNoiseRadius);
    }
}