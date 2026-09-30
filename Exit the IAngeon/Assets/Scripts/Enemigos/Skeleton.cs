using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Skeleton : MonoBehaviour
{
    public enum Estado{
        Buscando,
        Persiguiendo
    }

    private Vector3 velocity;
    private Vector3 acceleration;
    private Vector3 rotacionVision;
    private Vector3 positionAnteriorPlayer;
    private bool firstPursue;
    private GameObject player;
    private Player playerScr;
    private float playerSpeed;
    private float playerRunSpeed;
    private int avoidanceSide;

    public Estado estadoActual;
    public Transform jugador;
    public Transform conoVision;
    public float velocidadRotacion = 15f;
    public LayerMask obstacleLayer;
    public float obstacleDistance = 3f;
    public float emergencyDistance = 0.5f;
    
    [Header("Seek")]
    public float maxSpeed = 5f;
    public float maxForce = 0.2f;

    void Start()
    {
        estadoActual = Estado.Buscando;
        velocity = Vector3.zero; //se inicializan los vectores a 0, en la web lo que pone es this.velocity = createVector(0, 0);
        acceleration = Vector3.zero; //this.acceleration = createVector(0, 0);
        //Preparación para el pursue
        firstPursue = true;
        player = GameObject.FindWithTag("Player");
        playerScr = player.GetComponent<Player>();
        playerSpeed = playerScr.speed;
        playerRunSpeed = playerScr.runSpeed;
        //Fin de preparación del pursue
        avoidanceSide = 0;//Cosa de obstacle
    }

    void Update()
    {
        if (estadoActual == Estado.Persiguiendo) 
        {
            Pursue(jugador.position);
            ObstacleAvoidance();

            //aplicar aceleración a la velocidad -> this.velocity.add(this.acceleration);
            velocity += acceleration; 
            //limita la velocidad actual a la velocidad máxima -> this.velocity.limit(this.maxspeed);
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            //mover el goblin -> this.position.add(this.velocity);
            transform.position += velocity * Time.deltaTime;
            //rotar cono de vision según el movimiento:
            rotacionVision = velocity.normalized;
            if (rotacionVision != Vector3.zero)
            {
                //Se calcula el angulo:
                float anguloRadianes = Mathf.Atan2(rotacionVision.y, rotacionVision.x);
                float anguloGrados = anguloRadianes * Mathf.Rad2Deg;

                //Crea la rotación en Z:
                Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, anguloGrados);

                //Aplica la rotación:
                conoVision.rotation = Quaternion.Slerp(
                    conoVision.rotation, 
                    rotacionObjetivo, 
                    velocidadRotacion * Time.deltaTime
                );
            }
            //resetear la aceleracion -> this.acceleration.mult(0);
            acceleration = Vector3.zero;
        }
        else
        {
            //Al tener otro estado, prepara el pursue para que el if funcione
            firstPursue = true;
        }
        //Debug.Log("Velocity: " + velocity);

    }

    void applyForce(Vector3 force)
    {
        acceleration += force; // this.acceleration.add(force);
    }

    //Para avoidear obstáculos
    //Esto crea tres vectores, uno central largo y otros dos pequeños a los lados
    //Esos vectores se raycastean para ver si colisionan con algo
    //Cuando un vector colisiona con algo, se crea un nuevo target perpendicular a la colisión y se hace seek al nuevo target
    void ObstacleAvoidance()
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

    //Hace los cálculos del pursue y con eso hace un Seek
    void Pursue(Vector3 target)
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
            if (playerScr.IsRunning)
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

    void Seek(Vector3 target)
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
 

    public void FollowMode(Transform objetivo)
    {
        estadoActual = Estado.Persiguiendo;
        jugador = objetivo;

        Debug.Log("¡Jugador detectado!, entrando en modo persecucion");
        //lastPosition = pos;
    }

    public bool EstaPersiguiendo()
    {
        return estadoActual == Estado.Persiguiendo;
    }
}
