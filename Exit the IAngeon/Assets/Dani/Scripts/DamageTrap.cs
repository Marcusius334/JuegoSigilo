using UnityEngine;

public class DamageTrap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // el sonido que detecta el jugador
            Debug.Log("Jugador dañado");

        }
    }
}
