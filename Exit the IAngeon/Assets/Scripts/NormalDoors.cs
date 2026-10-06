using UnityEngine;
using UnityEngine.InputSystem;

public class NormalDoor : MonoBehaviour
{
    private Collider2D doorCollider;
    private Renderer doorRenderer; // Para detectar el Tilemap
    private bool playerNear = false;

    void Start()
    {
        
        doorCollider = GetComponent<Collider2D>();   //Pilla el componente
        doorRenderer = GetComponent<Renderer>(); 
    }

    void Update()
    {
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            
            if (doorCollider != null)   //Quitamos la colision
                doorCollider.enabled = false;
                
            
            if (doorRenderer != null)   //Se desactiva la puerta
                doorRenderer.enabled = false; 
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