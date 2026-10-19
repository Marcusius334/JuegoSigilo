using UnityEngine;

public class slimeIA : Enemy
{
    //private Estado estadoAnterior;

    [Header("Deteccion")]//Provisional
    public float radioDeteccion = 2f;
    
    void Start()
    {
        InicializarMaquinaEstados<BuscandoState>(typeof(BuscandoState),
        typeof(PersiguiendoState),
        typeof(SeguirSonidoState));
        //estadoAnterior = EstadoActual;
    }

    //===============================
    //UPDATE
    //===============================
    void Update()
    {
        if (maquinaEstados == null)
            return;

        // La máquina ejecuta el comportamiento del estado actual.
        maquinaEstados.ActualizarEstado();

        // Orientar el sprite según la dirección del movimiento.
        if (sprite != null && velocity.magnitude > 0.01f)
        {
            sprite.flipX = velocity.x < 0;
        }
    }

    //===============================
    //ESTADO BUSCANDO
    //===============================
    public override void EjecutarBuscando()
    {
        // Aplicar variación ligera de dirección.
        ApplyWanderDirect();

        // Calcular la fuerza de dirección.
        Vector3 desired = (Vector3)moveDirection * maxSpeed;
        Vector3 steer = desired - velocity;
        steer = Vector3.ClampMagnitude(steer, maxForce);

        applyForce(steer);

        if (velocity.magnitude < 0.01f && moveDirection != Vector2.zero)
        {
            velocity = (Vector3)moveDirection * 0.1f;
        }

        // Esquivar obstáculos.
        ObstacleAvoidance();

        // Aplicar aceleración y mover una sola vez.
        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;

        acceleration = Vector3.zero;
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

            LanzarEvento("sound_reached");
            return;
        }
        if (tienePosicionSonido) 
        {
            Vector3 siguienteNodo = ObtenerSiguienteNodoAStar(posicionSonido);
            Seek(siguienteNodo);
            ObstacleAvoidance();

            velocity += acceleration;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            transform.position += velocity * Time.deltaTime;
            acceleration = Vector3.zero;

        }

        // Comprobar si ha llegado a la posición del sonido.
        if (Vector3.Distance(transform.position, posicionSonido) < distanciaNodoAStar)
        {
            tienePosicionSonido = false;
            ReiniciarCaminoAStar();
            velocity = Vector3.zero;
            acceleration = Vector3.zero;

            LanzarEvento("sound_reached");
        }
    }

    //===============================
    //ESTADO PERSIGUIENDO
    //===============================
    public override void EjecutarPersecucion()
    {
        if (playerTrn == null)
        {
            velocity = Vector3.zero;
            acceleration = Vector3.zero;

            LanzarEvento("player_lost");
            return;
        }

        Seek(playerTrn.position);
        ObstacleAvoidance();

        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;

        acceleration = Vector3.zero;

        // El slime deja de perseguir cuando se agota el tiempo.
        tiempoSinVerJugador += Time.deltaTime;

        if (tiempoSinVerJugador >= tiempoDePersecucion)
        {
            velocity = Vector3.zero;
            acceleration = Vector3.zero;
            playerTrn = null;

            LanzarEvento("player_lost");
        }
    }

    


    public void HearNoise(Transform noisePosition)
    {
        float distancia = Vector3.Distance(transform.position, noisePosition.position);

        tiempoSinVerJugador = 0f;

        if (distancia <= radioDeteccion)
        {
            playerTrn = noisePosition;
            LanzarEvento("player_detected");
        }
        else
        {
            posicionSonido = noisePosition.position;
            tienePosicionSonido = true;

            ReiniciarCaminoAStar();
            LanzarEvento("noise_heard");
        }
    }
    
    //===============================
    //DIBUJAR EL AREA DE DETECCION DEL SLIME
    //===============================
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}