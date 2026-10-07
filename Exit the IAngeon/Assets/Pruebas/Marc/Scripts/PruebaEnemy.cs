using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PruebaEnemy : Enemy
{
    
    void Start()
    {
        InicializarMaquinaEstados(Estado.Buscando);
   
    }

    //===============================
    //UPDATE
    //===============================
    void Update()
    {
        if (EstadoActual == Estado.Persiguiendo) 
        {

            acceleration = Vector3.zero;

            tiempoSinVerJugador += Time.deltaTime;

            if (tiempoSinVerJugador >= tiempoDePersecucion)
            {
                CambiarEstado(Estado.Buscando);
                tiempoSinVerJugador = 0f;
                velocity = Vector3.zero;
                ReiniciarCaminoAStar();

                Debug.Log("Skeleton ha perdido al jugador");
            }
        }
        else if (EstadoActual == Estado.Patrullando)
        {
            Vector3 direccionRuta = CalcularDireccionPatrulla();
            velocity = Vector3.Lerp(velocity, direccionRuta, Time.deltaTime * 2f);
            
            ObstacleAvoidance();

            
            velocity += acceleration; 
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            transform.position += velocity * Time.deltaTime;
            
            rotacionVision = velocity.normalized;
            if (rotacionVision != Vector3.zero)
            {
                float anguloRadianes = Mathf.Atan2(rotacionVision.y, rotacionVision.x);
                float anguloGrados = anguloRadianes * Mathf.Rad2Deg;
                Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, anguloGrados);
                conoVision.rotation = Quaternion.Slerp(
                    conoVision.rotation, 
                    rotacionObjetivo, 
                    velocidadRotacion * Time.deltaTime
                );
            }
            acceleration = Vector3.zero;
            
            firstPursue = true; // Prepara el pursue por si pasa a perseguir
        }
        else
        {
            //Al tener otro estado, prepara el pursue para que el if funcione
            firstPursue = true;
            velocity = Vector3.zero; // Nos aseguramos de que no se deslice si esta buscando
        }

        //Debug.Log("Velocity: " + velocity);

    }

    //===============================
    //FOLLOW MODE
    //===============================
    public void FollowMode(Transform objetivo)
    {
        CambiarEstado(Estado.Persiguiendo);
        playerTrn = objetivo;

        tiempoSinVerJugador = 0f;
        ReiniciarCaminoAStar();

        Debug.Log("¡Jugador detectado!, entrando en modo persecucion");
    }

    //===============================
    //COSAS VISION HITBOX
    //===============================
    public bool EstaPersiguiendo()
    {
        return EstadoActual == Estado.Persiguiendo;
    }
    public Transform PlayerTransform()//para que lo de la vision hitbox vaya bien
    {
        return playerTrn;
    }
}
