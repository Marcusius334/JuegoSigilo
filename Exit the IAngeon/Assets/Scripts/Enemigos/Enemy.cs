using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour, IHearing
{
    //===============================
    //VARIABLES SEPARADAS EN HEADERS PARA QUE VAYA BIEN
    //===============================
    [Header("Generic")]
    protected GameObject player;
    protected Transform playerTrn;
    protected Player playerScr;
    protected SpriteRenderer sprite;
    public float tiempoDePersecucion = 5f;
    public float tiempoSinVerJugador;

    [Header("Sound")]
    public AudioSource audioSourceMovement;
    public AudioSource audioSourceExtra;
    public float noiseRadius = 15f;
    
    [Header("Rotation")]
    protected Vector3 rotacionVision;
    public Transform conoVision;
    public float velocidadRotacion = 15f;

    [Header("Seek")]
    protected Vector3 velocity;
    protected Vector3 acceleration;
    public float maxSpeed = 3f;
    public float maxForce = 0.2f;

    [Header("Pursue")]
    protected Vector3 positionAnteriorPlayer;
    protected bool firstPursue;
    protected float playerSpeed;
    protected float playerRunSpeed;

    [Header("Wander")]
    protected Vector2 wanderTarget;
    protected Vector2 moveDirection;
    public float wanderRadius = 2.0f;
    public float wanderDistance = 4.0f;
    public float wanderJitter = 0.5f;

    [Header("Obstacle avoidance")]
    protected int avoidanceSide;
    public LayerMask obstacleLayer;
    public float obstacleDistance = 3f;
    public float emergencyDistance = 0.5f;

    [Header("Patrulla (Waypoints)")]
    public Transform[] puntosDePatrulla;
    protected int indicePatrulla = 0;
    public float distanciaCambioPunto = 0.5f;

    [Header("State Machine")]
    [SerializeField] protected StateMachine maquinaEstados;
    public Estado EstadoActual => maquinaEstados.EstadoActual;

    //===============================
    // A*
    //===============================
    protected AStarPathfinding pathfinding;

    protected List<AStarNode> caminoAStar;
    protected int indiceCaminoAStar;

    protected Vector3 posicionSonido;
    protected bool tienePosicionSonido = false;

    [SerializeField] protected float distanciaNodoAStar = 1f;
    [SerializeField] protected float tiempoRecalculoAStar = 0.3f;

    protected float temporizadorAStar;

    //===============================
    //LOS DIFERENTES ESTADOS
    //===============================
    public enum Estado{
        Patrullando,
        Buscando, //TODOS LOS SITIOS LOS QUE PONGA BUSCANDO HAY Q SUSTITUIRLOS POR PATRULLANDO (he usado el estado Buscando para hacer pruebas solo )
        SeguirSonido,
        Persiguiendo,
        Acorralando
    }

    //===============================
    //START
    //===============================  
    protected void Awake()
    {
        //Preparación genérica
        player = GameObject.FindWithTag("Player");
        playerScr = player.GetComponent<Player>();
        playerTrn = player.GetComponent<Transform>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        maquinaEstados = GetComponent<StateMachine>();

        //Preparación para el seek
        velocity = Vector3.zero; //se inicializan los vectores a 0, en la web lo que pone es this.velocity = createVector(0, 0);
        acceleration = Vector3.zero; //this.acceleration = createVector(0, 0);
        
        //Preparación para el pursue
        firstPursue = true;
        playerSpeed = playerScr.speed;
        playerRunSpeed = playerScr.runSpeed;

        //Preparación wander
        // Inicializamos una dirección aleatoria y la normalizamos
        moveDirection = Random.insideUnitCircle.normalized;
        // Punto inicial dentro del círculo de wander
        wanderTarget = Random.insideUnitCircle * wanderRadius;

        //Preparación para el obstacle avoidance
        avoidanceSide = 0;//Cosa de obstacle

        pathfinding = FindFirstObjectByType<AStarPathfinding>();
    }
    //===============================
    // MAQUINA DE ESTADOS   
    //===============================
    protected void InicializarMaquinaEstados(Estado estadoInicial) 
    {
        //Esta función solo sirve para inicializar la Maquina
            maquinaEstados.Inicializar(estadoInicial);
    }
    protected void CambiarEstado(Estado nuevoEstado) 
    {
            maquinaEstados.CambiarEstado(nuevoEstado);
    }

    //===============================
    //ESCUCHAR EL SONIDO
    //===============================
    
    public virtual void SetNoisePosition(Transform noisePosition)
    {
        posicionSonido = noisePosition.position;
        tienePosicionSonido = true;

        // Si ya persigue al jugador, no interrumpir la persecución.
        // Guardamos el ruido para investigarlo después.
        if (EstadoActual == Estado.Persiguiendo)
            return;

        ReiniciarCaminoAStar();
        CambiarEstado(Estado.SeguirSonido);
    }


    //===============================
    //APPLY FORCE
    //===============================
    protected void applyForce(Vector3 force)
    {
        acceleration += force; // this.acceleration.add(force);
    }

    //===============================
    //SEEK
    //===============================
    protected void Seek(Vector3 target)
    {
        //direccion desde el Goblin hacia el objetivo -> let desired = p5.Vector.sub(target, this.position);
        Vector3 desired = target - transform.position;

        //Debug.Log("Desired: " + desired);
        //Si el goblin todavia NO está sobre el jugador -> desired.setMag(this.maxspeed);
        if (desired.magnitude > 0.01f) {desired = desired.normalized * maxSpeed; } //lo que hace esq el vector desired tenga una longitud igual a maxSpeed

        //fuerza de dirección -> let steer = p5.Vector.sub(desired, this.velocity);
        Vector3 steer = desired - velocity;

        //Debug.Log("Steer: " + steer);
        //limitar la fuerza maxima -> steer.limit(this.maxforce);
        steer = Vector3.ClampMagnitude(steer, maxForce);
        
        //aplicar fuerza -> this.applyForce(steer);
        applyForce(steer);
    }

    //===============================
    //PURSUE
    //===============================
    //Hace los cálculos del pursue y con eso hace un Seek
    protected void Pursue(Vector3 target)
    {
        //Si es la primera vez que se hace pursue se mueve hacia la posición actual del jugador porque todavía no ha podido obtener una posición anterior
        if (firstPursue)
        {
            firstPursue = false;
            Seek(target);
        }
        else
        {
            //Calculamos la dirección hacia la que se mueve el jugador
            Vector3 newDir = target - positionAnteriorPlayer;

            //Calculamos la dirección del esqueleto al jugador y lo multiplicamos por la diferencia entre las velocidades del jugador y el esqueleto
            float speedRatio;
            if (playerScr.Running())
            {
                speedRatio = playerRunSpeed/maxSpeed;
            }
            else
            {
                speedRatio = playerSpeed/maxSpeed;
            }
            Vector3 distEnmyPly = (transform.position - player.transform.position) * speedRatio;

            //Alineamos distEnmyPly con el vector newDir y obtenemos el punto final, con el que hacemos seek
            Vector3 finalVector = newDir.normalized * distEnmyPly.magnitude;
            Vector3 positionPursue = finalVector + target;
            Seek(positionPursue);
        }

        positionAnteriorPlayer = target;
    }

    //===============================
    //WANDER
    //===============================
    protected void ApplyWanderDirect()
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
    //OBSTACLE AVOIDANCE
    //===============================
    //Para avoidear obstáculos
    //Esto crea tres vectores, uno central largo y otros dos pequeños a los lados
    //Esos vectores se raycastean para ver si colisionan con algo
    //Cuando un vector colisiona con algo, se crea un nuevo target perpendicular a la colisión y se hace seek al nuevo target
    protected void ObstacleAvoidance()
    {
        //Si está parado no hace falta buscar obstáculos
        if (velocity.magnitude < 0.01f){
            return;
        }

        // Tres rayos: centro, izquierda y derecha
        Vector2 direction = velocity.normalized;
        Vector2 leftDir = Quaternion.Euler(0, 0, 30) * direction;
        Vector2 rightDir = Quaternion.Euler(0, 0, -30) * direction;

        float sideDistance = obstacleDistance * 0.6f;

        // Dibujar rayos
        Debug.DrawRay(transform.position, direction * obstacleDistance, Color.red);
        Debug.DrawRay(transform.position, leftDir * sideDistance, Color.yellow);
        Debug.DrawRay(transform.position, rightDir * sideDistance, Color.yellow);

        RaycastHit2D hitCenter = Physics2D.Raycast( //Crea un rayo que va
            transform.position,                     //desde una posición
            direction,                              //con una dirección
            obstacleDistance,                       //una distáncia máxima
            obstacleLayer                           //y detecta esta layer
        ); 

        RaycastHit2D hitLeft = Physics2D.Raycast(
            transform.position,
            leftDir,
            sideDistance,
            obstacleLayer
        );

        RaycastHit2D hitRight = Physics2D.Raycast(
            transform.position,
            rightDir,
            sideDistance,
            obstacleLayer
        );

        Vector2 avoidance = Vector2.zero;

        // Obstáculo de frente
        if (hitCenter.collider != null)
        {
            //rozando la pared se sale de todo con 180
            if (hitCenter.distance < emergencyDistance)
            {
                Vector2 oppositeDirection = -direction;

                Vector3 desired = oppositeDirection * maxSpeed;
                Vector3 steer = desired - velocity;

                steer = Vector3.ClampMagnitude(
                    steer,
                    maxForce * 3f
                );

                applyForce(steer);

                return;
            }
            else  //temporal hata el Pathfollowing A*
            {
            // Si no hay obstáculo de frente, reseteamos el lado para evitar que se ralle
            avoidanceSide = 0; 
            } 

            //Se busca si los lados están libres para que la nueva posición no sea recto (porque se chocaría con pared)
            //para que no se siga llendo recto y pasen cosas malas
            float leftDistance = hitLeft.collider != null ? hitLeft.distance : Mathf.Infinity;
            float rightDistance = hitRight.collider != null ? hitRight.distance : Mathf.Infinity;
            
            // Solo elegimos el lado si todavía no tenemos uno elegido
            //Cosa para que no se quede stuck
            if (avoidanceSide == 0)
            {
                if (leftDistance > rightDistance)
                {
                    avoidanceSide = -1;
                }
                else
                {
                    avoidanceSide = 1;
                }
            }

            //nuevo target
            avoidance += hitCenter.normal;
            if (avoidanceSide == -1)
            {
                avoidance += leftDir;
            }
            else
            {
                avoidance += rightDir;
            }
        }

        // Obstáculo a la izquierda
        if (hitLeft.collider != null)
        {
            avoidance += rightDir;
        }

        // Obstáculo a la derecha
        if (hitRight.collider != null)
        {
            avoidance += leftDir;
        }

        //Esto genera la nueva dirección deseada y se aplica fuerza en consecuencia a lo Steer
        if (avoidance != Vector2.zero)
        {
            avoidance.Normalize();

            Vector3 desired = avoidance * maxSpeed;
            Vector3 steer = desired - velocity;

            steer = Vector3.ClampMagnitude(steer, maxForce * 2f);

            applyForce(steer);
        }
    }

    //===============================
    //Patrulla Waypoints
    //===============================

    protected void EncontrarPuntoMasCercano()   //Esto es temporal hasta el Pathfollowing A* para que no se ralle el bicho
    {
        if (puntosDePatrulla == null || puntosDePatrulla.Length == 0) return;
        
        float distanciaMinima = Mathf.Infinity;
        int indiceMasCercano = 0;
        
        for (int i = 0; i < puntosDePatrulla.Length; i++)
        {
            float distancia = Vector3.Distance(transform.position, puntosDePatrulla[i].position);
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                indiceMasCercano = i;
            }
        }
        
        // Ahora su nuevo objetivo sera el punto que tenga mas cerca
        indicePatrulla = indiceMasCercano;
    }


    protected Vector3 CalcularDireccionPatrulla()
    {
        if (puntosDePatrulla == null || puntosDePatrulla.Length == 0) 
            return Vector3.zero;

        Transform objetivo = puntosDePatrulla[indicePatrulla];
        float distancia = Vector3.Distance(transform.position, objetivo.position);

        if (distancia < distanciaCambioPunto)
        {
            indicePatrulla++;
            if (indicePatrulla >= puntosDePatrulla.Length)
            {
                indicePatrulla = 0;
            }
            objetivo = puntosDePatrulla[indicePatrulla];
        }

        // Dirección hacia el punto
        return (objetivo.position - transform.position).normalized * maxSpeed;
    }

    //===============================
    // PATHFINDING A*
    //===============================

    protected void CalcularCaminoAStar(Vector3 objetivo)
    {
        if (pathfinding == null)
        {
            Debug.LogWarning("No se ha encontrado AStarPathfinding.");
            return;
        }

        caminoAStar = pathfinding.BuscarCamino(
            transform.position,
            objetivo
        );

        indiceCaminoAStar = 0;
        temporizadorAStar = tiempoRecalculoAStar;
    }

    protected Vector3 ObtenerSiguienteNodoAStar(Vector3 objetivo)
    {
        if (caminoAStar == null || caminoAStar.Count == 0)
        {
            CalcularCaminoAStar(objetivo);
        }

        if (caminoAStar == null || caminoAStar.Count == 0)
            return transform.position;

        if (indiceCaminoAStar < caminoAStar.Count)
        {
            float distancia = Vector2.Distance(
                transform.position,
                caminoAStar[indiceCaminoAStar].Posicion
            );

            if (distancia <= distanciaNodoAStar)
            {
                indiceCaminoAStar++;
            }
        }

        if (indiceCaminoAStar >= caminoAStar.Count)
        {
            return caminoAStar[caminoAStar.Count - 1].Posicion;
        }

        Debug.Log(
            "Nodo actual: " + indiceCaminoAStar +
            " / " + caminoAStar.Count
        );

        return caminoAStar[indiceCaminoAStar].Posicion;
    }

    protected void ReiniciarCaminoAStar()
    {
        caminoAStar = null;
        indiceCaminoAStar = 0;
    }

}
