using UnityEngine;

public class Goblin : MonoBehaviour
{
    public enum Estado{
        Buscando,
        Persiguiendo
    }

    private Estado estadoActual;
    private Vector3 lastPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        estadoActual = Estado.Buscando;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FollowMode(Vector3 pos)
    {
        estadoActual = Estado.Persiguiendo;
        Debug.Log("¡Jugador detectado!, entrando en modo persecucion");
        lastPosition = pos;
    }
}
