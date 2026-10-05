using UnityEngine;

public class slimeIA : Enemy
{
    [Header("Deteccion")]//Provisional
    public float radioDeteccion = 2f;
    
    void Start()
    {
        estadoActual = Estado.Buscando;
    }

    //===============================
    //UPDATE
    //===============================
    void Update()
    {
        if (estadoActual == Estado.Buscando)
        {
            // Aplicamos la variación de dirección ligera del Wander
            ApplyWanderDirect();

            ObstacleAvoidance();

            // Movimiento a VELOCIDAD CONSTANTE
            transform.Translate(moveDirection * maxSpeed * Time.deltaTime, Space.World);
        }
        else if (estadoActual == Estado.Persiguiendo) 
        {
            Seek(playerTrn.position);
            

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
                estadoActual = Estado.Buscando;
                Debug.Log("Mucho tiempo sin ver al jugador, Estado: BUSCANDO");
                velocity = Vector3.zero;//Pasan cosas de buscar
            }
        }

        //Esto es para que el sprite flippee en direccion a donde mira
        if (estadoActual == Estado.Persiguiendo)
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
    
    //===============================
    //DIBUJAR EL AREA DE DETECCION DEL SLIME
    //===============================
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}