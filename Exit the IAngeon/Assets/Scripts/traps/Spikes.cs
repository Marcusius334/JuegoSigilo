
using UnityEngine;

public class Spikes : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private Collider2D colliderPinchos;
    [SerializeField] private Animator animator;

    public SceneLoader sceneLoader;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        // La hitbox de daño empieza desactivada.
        colliderPinchos.enabled = false;
    }

    // Se llama cuando el jugador entra en el detector.
    public void ActivarPinchos()
    {
        animator.SetBool("Activado", true);
        Debug.Log("Activando pinchos");
    }

    // Animation Event: cuando los pinchos empiezan a hacer daño.
    public void ActivarDaño()
    {
        colliderPinchos.enabled = true;
        Debug.Log("Hitbox ACTIVADA");
    }

    // Animation Event: cuando los pinchos dejan de hacer daño.
    public void DesactivarDaño()
    {
        colliderPinchos.enabled = false;
        Debug.Log("Hitbox DESACTIVADA");
    }

    // Animation Event: al terminar la animación.
    public void ReiniciarTrampa()
    {
        colliderPinchos.enabled = false;
        animator.SetBool("Activado", false);
        Debug.Log("Trampa reiniciada");
    }

    // Se llama cuando el jugador toca los pinchos activos.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && colliderPinchos.enabled)
        {
            if (sceneLoader != null) sceneLoader.LoadScene("Death");
        }
    }
}
