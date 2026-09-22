using UnityEngine;
using UnityEngine.InputSystem;

public class Door : MonoBehaviour
{
    public Transform exitPoint;

    private bool playerNear = false;

    void Update()
    {
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TeleportPlayer();
        }
    }

    public void PlayerNear()
    {
        playerNear = true;
    }

    public void PlayerFar()
    {
        playerNear = false;
    }

    void TeleportPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        player.transform.position = exitPoint.position;
    }
}
