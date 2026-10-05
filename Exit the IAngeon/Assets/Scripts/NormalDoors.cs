using UnityEngine;
using UnityEngine.InputSystem;

public class NormalDoor : MonoBehaviour
{
    private Collider2D doorCollider;
    private bool playerNear = false;

    void Start()
    {
        doorCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            doorCollider.enabled = false;
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
}
