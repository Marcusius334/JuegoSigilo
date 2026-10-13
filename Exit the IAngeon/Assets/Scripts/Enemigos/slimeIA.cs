using UnityEngine;

public class slimeIA : Enemy
{
    private Estado estadoAnterior;

    [Header("Deteccion")]//Provisional
    public float radioDeteccion = 2f;
    
    void Start()
    {
        InicializarMaquinaEstados(Estado.Buscando);
        estadoAnterior = EstadoActual;
    }

    //===============================
    //UPDATE
    //===============================
    void Update()
    {
        if (EstadoActual != estadoAnterior)
        {
            if (EstadoActual == Estado.Persiguiendo)
            {
                ReiniciarCaminoAStar();
            }

            estadoAnterior = EstadoActual;
        }
        if (EstadoActual == Estado.Buscando)
        {
            // Aplicamos la variación de dirección ligera del Wander
            ApplyWanderDirect();

            // Convertimos esa dirección deseada en una fuerza de dirección (Steering)
            Vector3 desired = (Vector3)moveDirection * maxSpeed;
            Vector3 steer = desired - velocity;
            steer = Vector3.ClampMagnitude(steer, maxForce);
            applyForce(steer);

            if (velocity.magnitude < 0.01f && moveDirection != Vector2.zero)
            {
                velocity = (Vector3)moveDirection * 0.1f;
            }

            // 3. Calculamos la esquiva de obstáculos sobre la misma acumulación de fuerzas
            ObstacleAvoidance();
        }
        else if (EstadoActual == Estado.Persiguiendo) 
        {
            /*
            //A* global
            Vector3 siguienteNodo = ObtenerSiguienteNodoAStar(playerTrn.position);

            Vector3 direccion = (siguienteNodo - transform.position).normalized;

            velocity = direccion * maxSpeed;
            */

            Seek(playerTrn.position);
            ObstacleAvoidance();
            

            //aplicar aceleración a la velocidad -> this.velocity.add(this.acceleration);
            velocity += acceleration;
            //limita la velocidad actual a la velocidad máxima -> this.velocity.limit(this.maxspeed);
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

           

            //mover el goblin -> this.position.add(this.velocity);
            transform.position += velocity * Time.deltaTime;
            //resetear la aceleracion -> this.acceleration.mult(0);
            acceleration = Vector3.zero;


            tiempoSinVerJugador += Time.deltaTime;
            if (tiempoSinVerJugador >= tiempoDePersecucion)
            {
                InicializarMaquinaEstados(Estado.Buscando);
                //Debug.Log("Mucho tiempo sin ver al jugador, Estado: BUSCANDO");
                velocity = Vector3.zero;//Pasan cosas de buscar
            }
        }

        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        // Mover la posición según la velocidad calculada
        transform.position += velocity * Time.deltaTime;

        // Resetear la aceleración para el siguiente frame
        acceleration = Vector3.zero;

        //Esto es para que el sprite flippee en direccion a donde mira
        if (velocity.magnitude > 0f)
        {
            sprite.flipX = velocity.x < 0;
        }
    }

    public void HearNoise(Transform noisePosition) 
    {
        tiempoSinVerJugador = 0f;
        CambiarEstado(Estado.Persiguiendo);
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