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

            ObstacleAvoidance();

            // Movimiento a VELOCIDAD CONSTANTE
            transform.Translate(moveDirection * maxSpeed * Time.deltaTime, Space.World);
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

        //Esto es para que el sprite flippee en direccion a donde mira
        if (EstadoActual == Estado.Persiguiendo)
        {
            if (velocity.x < 0)
                sprite.flipX = true;
            else 
                sprite.flipX = false;
        }
        else
        {
            if (moveDirection.x < 0)
                sprite.flipX = true;
            else
                sprite.flipX = false;
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