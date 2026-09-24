using UnityEngine;

public class slimeIA : MonoBehaviour
{
    public enum Estado
    {
        Buscando,
        Persiguiendo
    }

    public SpriteRenderer sprite;
    private Estado estadoActual;

    //Wander
    private Vector2 wanderTarget;
    private Vector2 moveDirection; // Dirección actual estandarizada

    //Seek
    private Vector3 velocity;
    private Vector3 acceleration;
    private Transform jugador;
    private float tiempoSinVerJugador;



    [Header("Parámetros de Wander")]
    public float wanderRadius = 2.0f;
    public float wanderDistance = 4.0f;
    public float wanderJitter = 0.5f;

    [Header("Parámetros de Evasión / Rebote")]
    public float avoidDistance = 1.5f; // Distancia para detectar la pared antes del choque
    public LayerMask obstacleMask;

    [Header("Movimiento Constante")]
    public float maxSpeed = 5.0f;

    [Header("Seek")]//Esto es para el seek
    public float maxForce = 0.2f;
    public float tiempoDePersecucion = 5f;

    [Header("Deteccion")]//Provisional
    public float radioDeteccion = 2f;
    

    void Start()
    {
        estadoActual = Estado.Buscando;

        // Inicializamos una dirección aleatoria y la normalizamos
        moveDirection = Random.insideUnitCircle.normalized;
        // Punto inicial dentro del círculo de wander
        wanderTarget = Random.insideUnitCircle * wanderRadius;

        //Inicialización para el Seek
        velocity = Vector3.zero;
        acceleration = Vector3.zero;

        //Porvisional
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (jugador != null) 
        {
            DetectarPLayer();
        }

        if (estadoActual == Estado.Buscando)
        {
            // 1. Primero comprobamos si hay una pared al frente para rebotar
            CheckWallCollision();

            // 2. Si no rebotó, aplicamos la variación de dirección ligera del Wander
            ApplyWanderDirect();

            // 3. Movimiento a VELOCIDAD CONSTANTE
            transform.Translate(moveDirection * maxSpeed * Time.deltaTime, Space.World);
        }
        else if (estadoActual == Estado.Persiguiendo) 
        {
            Seek(jugador.position);
            

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
    //SEEK
    //===============================
    void applyForce(Vector3 force)
    {
        acceleration += force; // this.acceleration.add(force);
    }
    void Seek(Vector3 target)
    {
        //direccion desde el Goblin hacia el objetivo -> let desired = p5.Vector.sub(target, this.position);
        Vector3 desired = target - transform.position;

        //Si el goblin todavia NO está sobre el jugador -> desired.setMag(this.maxspeed);
        if (desired.magnitude > 0.01f) { desired = desired.normalized * maxSpeed; } //lo que hace esq el vector desired tenga una longitud igual a maxSpeed

        //fuerza de dirección -> let steer = p5.Vector.sub(desired, this.velocity);
        Vector3 steer = desired - velocity;

        //limitar la fuerza maxima -> steer.limit(this.maxforce);
        steer = Vector3.ClampMagnitude(steer, maxForce);

        //aplicar fuerza -> this.applyForce(steer);
        applyForce(steer);
    }

    //===============================
    //DETECTAR PLAYER (provisional) Esta funcion se tiene que cambiar para que, en lugar de chocar con los slimes, que detecten las vibraciones
    //===============================
    private void DetectarPLayer()
    {
        float distancia = Vector3.Distance(transform.position, jugador.position);
        if (distancia <= radioDeteccion) 
        {
            estadoActual = Estado.Persiguiendo;
            tiempoSinVerJugador = 0f;
        }

    }



    //===============================
    //SEEK
    //===============================
    void CheckWallCollision()
    {
        // 3 Direcciones de rayos en abanico
        Vector2 forward = moveDirection;
        Vector2 left = Quaternion.Euler(0, 0, 35) * forward;
        Vector2 right = Quaternion.Euler(0, 0, -35) * forward;

        Vector2[] rayDirections = new Vector2[] { forward, left, right };

        foreach (var dir in rayDirections)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, avoidDistance, obstacleMask);

            if (hit.collider != null)
            {
                // Reflejamos la dirección de movimiento según la normal del muro
                if (hit.distance < avoidDistance) moveDirection *= -1;
                //moveDirection = Vector2.Reflect(moveDirection, hit.normal).normalized;

                // Reiniciamos el objetivo de Wander para que apunte en la nueva dirección
                wanderTarget = Random.insideUnitCircle * wanderRadius;

                // Salimos del bucle para no procesar múltiples rebotes en el mismo frame
                break;
            }
        }
    }

    void ApplyWanderDirect()
    {
        // Añadimos pequeña variación aleatoria
        wanderTarget += new Vector2(
            Random.Range(-1f, 1f) * wanderJitter,
            Random.Range(-1f, 1f) * wanderJitter
        );

        wanderTarget = wanderTarget.normalized * wanderRadius;

        // Proyectamos el objetivo hacia adelante en base a la dirección actual
        Vector2 circleCenter = moveDirection * wanderDistance;
        
        // Obtenemos la dirección deseada
        Vector2 targetWorldDirection = (circleCenter + wanderTarget).normalized;

        // Rotamos suavemente la dirección actual hacia la dirección del Wander
        moveDirection = Vector2.Lerp(moveDirection, targetWorldDirection, Time.deltaTime * 3.0f).normalized;
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