using UnityEngine;

public class vision_hitbox : MonoBehaviour
{
    [SerializeField] private LayerMask capaMuros;

    public Goblin goblin;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Algo ha entrado en la visión: " + other.name);
        if (!other.CompareTag("Player")) return;
        Debug.Log("¡Jugador detectado dentro de la visión!");

        Vector2 direccion = other.transform.position - transform.position;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direccion.normalized,
            direccion.magnitude,
            capaMuros
        );

        if (hit.collider == null)
        {
            Debug.Log("No hay ningún muro. ¡Jugador detectado!");
            goblin.FollowMode(other.transform);
        }
        else
        {
            Debug.Log("Hay un muro entre el goblin y el jugador.");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
