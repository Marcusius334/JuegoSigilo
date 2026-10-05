using UnityEngine;

public class NormalDoorInteraction : MonoBehaviour
{
    public NormalDoor door;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            door.PlayerNear();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            door.PlayerFar();
        }
    }
}
