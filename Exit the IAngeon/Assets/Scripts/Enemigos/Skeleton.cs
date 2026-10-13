
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Skeleton : Enemy
{
    void Start()
    {
        if (puntosDePatrulla != null && puntosDePatrulla.Length > 0)
            InicializarMaquinaEstados(Estado.Patrullando);
        else
            InicializarMaquinaEstados(Estado.Buscando);
    }

    void Update()
    {
        if (EstadoActual == Estado.Persiguiendo)
        {
            if (playerTrn == null)
            {
                if (tienePosicionSonido)
                    CambiarEstado(Estado.SeguirSonido);
                else
                    CambiarEstado(Estado.Buscando);

                velocity = Vector3.zero;
                return;
            }

            Pursue(playerTrn.position);
            ObstacleAvoidance();

            velocity += acceleration;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            transform.position += velocity * Time.deltaTime;

            //Rotación del cono de visión
            rotacionVision = velocity.normalized;

            if (rotacionVision != Vector3.zero && conoVision != null)
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
            tiempoSinVerJugador += Time.deltaTime;

            if (tiempoSinVerJugador >= tiempoDePersecucion)
            {
                tiempoSinVerJugador = 0f;
                velocity = Vector3.zero;
                ReiniciarCaminoAStar();

                // Si ha perdido de vista al jugador, investiga el último sonido.
                if (tienePosicionSonido)
                {
                    CambiarEstado(Estado.SeguirSonido);
                }
                else
                {
                    CambiarEstado(Estado.Buscando);
                }

                Debug.Log("Skeleton ha perdido al jugador");
            }
        }
        else if (EstadoActual == Estado.SeguirSonido)
        {
            // Seguir el camino calculado mediante A*
            if (tienePosicionSonido)
            {
                Vector3 siguienteNodo = ObtenerSiguienteNodoAStar(posicionSonido);
                Seek(siguienteNodo);
                /*
                Vector3 direccion = (siguienteNodo - transform.position).normalized;

                velocity = direccion * maxSpeed;
                */

                ObstacleAvoidance();

                velocity += acceleration;
                velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
                transform.position += velocity * Time.deltaTime;

                rotacionVision = velocity.normalized;

                if (rotacionVision != Vector3.zero && conoVision != null)
                {
                    float anguloRadianes = Mathf.Atan2(
                        rotacionVision.y,
                        rotacionVision.x
                    );

                    float anguloGrados = anguloRadianes * Mathf.Rad2Deg;

                    Quaternion rotacionObjetivo = Quaternion.Euler(
                        0f, 0f, anguloGrados
                    );

                    conoVision.rotation = Quaternion.Slerp(
                        conoVision.rotation,
                        rotacionObjetivo,
                        velocidadRotacion * Time.deltaTime
                    );
                }

                acceleration = Vector3.zero;

                // Si ha llegado a la posición del sonido, vuelve a buscar.
                if (Vector3.Distance(transform.position, posicionSonido) < distanciaNodoAStar)
                {
                    tienePosicionSonido = false;
                    CambiarEstado(Estado.Buscando);

                    ReiniciarCaminoAStar();
                    velocity = Vector3.zero;
                }
            }
            else
            {
                CambiarEstado(Estado.Buscando);
                ReiniciarCaminoAStar();
                velocity = Vector3.zero;
            }

            
        }
        else if (EstadoActual == Estado.Patrullando)
        {
            // Movimiento entre los waypoints de la patrulla
            Vector3 direccionRuta = CalcularDireccionPatrulla();

            velocity = Vector3.Lerp(velocity, direccionRuta, Time.deltaTime * 2f);

            ObstacleAvoidance();

            velocity += acceleration;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            transform.position += velocity * Time.deltaTime;

            //Rotación del cono de visión
            rotacionVision = velocity.normalized;

            if (rotacionVision != Vector3.zero && conoVision != null)
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
            firstPursue = true;
        }
        else
        {
            // Si no está patrullando, persiguiendo o siguiendo un sonido,
            // se queda quieto mientras está en estado Buscando.
            firstPursue = true;
            velocity = Vector3.zero;
            acceleration = Vector3.zero;
        }
    }

    public void FollowMode(Transform objetivo)
    {
        if (objetivo == null)
            return;

        // Guardamos el objetivo y reiniciamos el tiempo sin ver al jugador.
        playerTrn = objetivo;
        tiempoSinVerJugador = 0f;

        if (EstadoActual != Estado.Persiguiendo)
        {
            ReiniciarCaminoAStar();
            firstPursue = true;
            CambiarEstado(Estado.Persiguiendo);
        }

        Debug.Log("¡Jugador detectado!, entrando en modo persecucion");
    }

    public bool EstaPersiguiendo()
    {
        return EstadoActual == Estado.Persiguiendo;
    }

    public Transform PlayerTransform()
    {
        return playerTrn;
    }
}
