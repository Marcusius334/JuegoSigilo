using UnityEngine;
using UnityEngine.InputSystem;

public class NormalDoor : MonoBehaviour
{
    private Collider2D doorCollider;
    private Renderer doorRenderer; // Para detectar el Tilemap
    private GameObject player;
    private Player playerScr;
    private bool playerNear = false;

    void Start()
    {
        
        doorCollider = GetComponent<Collider2D>();   //Pilla el componente
        doorRenderer = GetComponent<Renderer>(); 
        player = GameObject.FindGameObjectWithTag("Player");
        playerScr = player.GetComponent<Player>();
    }

    void Update()
    {
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            playerScr.OpenDoor();

            
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