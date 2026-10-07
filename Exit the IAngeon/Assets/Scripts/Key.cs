using UnityEngine;
using UnityEngine.InputSystem;

public class Key : MonoBehaviour
{
    private bool playerNear = false;
    private Player player;

    void Update()
    {
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (player != null)
            {
                player.hasKey = true; // Esto es para marcar que el jugador ha pillao la llave
                player.GetKey();
            }
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            player = other.GetComponent<Player>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            player = null;
        }
    }
}