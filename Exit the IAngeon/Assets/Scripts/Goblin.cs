using UnityEngine;

public class Goblin : MonoBehaviour
{
    public enum Estado
    {
        Buscando,
        Persiguiendo
    }

    private Estado estadoActual;
    private Vector3 lastPosition;
    
    void Start()
    {
        //Empieza en busqueda:
        estadoActual = Estado.Buscando;
    }

    void Update()
    {
        if (estadoActual == Estado.Buscando)
        {
            //Movimiento con Wander:
            
        }   
    }

    public void FollowMode(Vector3 pos)
    {
        //Entra en modo persecución al encontrar un enemigo y almacena su posición:
        estadoActual = Estado.Persiguiendo;
        Debug.Log("¡Jugador detectado!, entrando en modo persecucion");
        lastPosition = pos;
    }
}
