
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Skeleton : Enemy
{
    void Start()
    {
        if (puntosDePatrulla != null && puntosDePatrulla.Length > 0)
        {
            InicializarMaquinaEstados<PatrullandoState>(typeof(PatrullandoState), typeof(BuscandoState), typeof(PersiguiendoState), typeof(SeguirSonidoState));

        }
        else 
        {
            InicializarMaquinaEstados<BuscandoState>(typeof(PatrullandoState), typeof(BuscandoState), typeof(PersiguiendoState), typeof(SeguirSonidoState));
        }
            
    }

    void Update()
    {
        if (maquinaEstados == null)
            return;

        maquinaEstados.ActualizarEstado();

        // Orientar el cono de visión según el movimiento.
        ActualizarRotacionVision();
    }


    // =========================================
    // ESTADO: BUSCANDO
    // =========================================
    public override void EjecutarBuscando()
    {
        firstPursue = true;

        velocity = Vector3.zero;
        acceleration = Vector3.zero;
    }

    // =========================================
    // ESTADO: PATRULLANDO
    // =========================================
    public override void EjecutarPatrulla()
    {
        Vector3 direccionRuta = CalcularDireccionPatrulla();

        velocity = Vector3.Lerp(
            velocity,
            direccionRuta,
            Time.deltaTime * 2f
        );


        ObstacleAvoidance();

        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;

        acceleration = Vector3.zero;

        firstPursue = true;
    }

    // =========================================
    // ESTADO: PERSIGUIENDO
    // =========================================
    public override void EjecutarPersecucion()
    {
        if (playerTrn == null)
        {

            if (tienePosicionSonido)
            {
                LanzarEvento("noise_heard");
            }
            else
            {
                LanzarEvento("player_lost");
            }
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
            acceleration = Vector3.zero;

            ReiniciarCaminoAStar();

            // Conservar el comportamiento del código antiguo:
            // investigar el sonido si hay uno pendiente.
            if (tienePosicionSonido)
            {
                LanzarEvento("noise_heard");
            }
            else
            {
                LanzarEvento("player_lost");
            }

            Debug.Log("Skeleton ha perdido al jugador");
        }
    }

    // =========================================
    // ESTADO: SIGUIENDO UN SONIDO
    // =========================================
    public override void EjecutarSeguirSonido()
    {
        if (!tienePosicionSonido)
        {
            ReiniciarCaminoAStar();

            velocity = Vector3.zero;
            acceleration = Vector3.zero;

            LanzarEvento("sound_reached"); //cambia a estado buscando
            return;
        }
  
        Vector3 siguienteNodo = ObtenerSiguienteNodoAStar(posicionSonido);

        Seek(siguienteNodo);
        ObstacleAvoidance();

        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;

        rotacionVision = velocity.normalized;

        if (rotacionVision != Vector3.zero && conoVision != null)
        {
            ActualizarRotacionVision();
        }


        acceleration = Vector3.zero;

        if (Vector3.Distance(transform.position, posicionSonido)
            < distanciaNodoAStar)
        {
            tienePosicionSonido = false;

            ReiniciarCaminoAStar();

            velocity = Vector3.zero;
            acceleration = Vector3.zero;

            LanzarEvento("sound_reached");
        }
    }


    public void FollowMode(Transform objetivo)
    {
        if (objetivo == null)
            return;

        // Guardamos el objetivo y reiniciamos el tiempo sin ver al jugador.
        playerTrn = objetivo;
        tiempoSinVerJugador = 0f;

        if (!(EstadoActual is PersiguiendoState))
        {
            ReiniciarCaminoAStar();
            firstPursue = true;
            LanzarEvento("player_detected");
        }

        Debug.Log("¡Jugador detectado!, entrando en modo persecucion");
    }

    public bool EstaPersiguiendo()
    {
        return EstadoActual is PersiguiendoState;
    }

    public Transform PlayerTransform()
    {
        return playerTrn;
    }

    // =========================================
    // ROTACIÓN DEL CONO DE VISIÓN
    // =========================================
    private void ActualizarRotacionVision()
    {
        if (conoVision == null || velocity.sqrMagnitude < 0.0001f)
            return;

        rotacionVision = velocity.normalized;

        float anguloGrados = Mathf.Atan2(
            rotacionVision.y,
            rotacionVision.x
        ) * Mathf.Rad2Deg;

        Quaternion rotacionObjetivo = Quaternion.Euler(
            0f,
            0f,
            anguloGrados
        );

        conoVision.rotation = Quaternion.Slerp(
            conoVision.rotation,
            rotacionObjetivo,
            velocidadRotacion * Time.deltaTime
        );
    }
}
