using UnityEngine;
using UnityEngine.InputSystem;

public class NormalDoor : MonoBehaviour
{
    private Collider2D doorCollider;
    private Renderer doorRenderer; 
    private GameObject player;
    private Player playerScr;
    private bool playerNear = false;
    
    // Bandera para saber si el jugador ya dejó la puerta abierta permanentemente
    private bool abiertaPorJugador = false;

    void Start()
    {
        doorCollider = GetComponent<Collider2D>();   
        doorRenderer = GetComponent<Renderer>(); 
        player = GameObject.FindGameObjectWithTag("Player");
        playerScr = player.GetComponent<Player>();
    }

    void Update()
    {
        // Si el jugador no la había abierto aún y pulsa la E
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame && !abiertaPorJugador)
        {
            playerScr.OpenDoor();
            abiertaPorJugador = true; // Registramos que el jugador la abrió
            
            if (doorCollider != null) doorCollider.enabled = false;
            if (doorRenderer != null) doorRenderer.enabled = false; 
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

    // ==========================================
    // MÉTODOS PARA IA ENEMIGA
    // ==========================================
    
    public void AbrirParaEnemigo()
    {
        // Si el jugador ya la abrió, la IA no interfiere
        if (abiertaPorJugador) return;

        if (doorCollider != null) doorCollider.enabled = false;
        if (doorRenderer != null) doorRenderer.enabled = false;
    }

    public void CerrarParaEnemigo()
    {
        // Solo se cierra tras su paso si el jugador NO la había abierto
        if (abiertaPorJugador) return;

        if (doorCollider != null) doorCollider.enabled = true;
        if (doorRenderer != null) doorRenderer.enabled = true;
    }
}