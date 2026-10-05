using UnityEngine;
using UnityEngine.InputSystem;

public class Door : MonoBehaviour
{
    public Transform exitPoint;

    private bool playerNear = false;
    private Player currentPlayer; // Guardamos la referencia del jugador que está cerquita

    void Update()
    {
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)   //Comprueba que el jugador esta al lado y ha pulsado E
        {
            if (currentPlayer != null && currentPlayer.hasKey)   //Comprobamos si el jugador existe y ha pillao la llave
            {
                TeleportPlayer();
            }
            else
            {
                Debug.Log("Skillisue tomto. La puerta del nivel está cerrada y necesitas la llave.");
            }
        }
    }

    public void PlayerNear(Player playerScript)   //Necesita el Script del jugador
    {
        playerNear = true;
        currentPlayer = playerScript;
    }

    public void PlayerFar()
    {
        playerNear = false;
        currentPlayer = null; // Limpiamos la referencia al salir del área
    }

    void TeleportPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        player.transform.position = exitPoint.position;
    }
}
