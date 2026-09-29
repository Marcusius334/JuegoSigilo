using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Goblin : MonoBehaviour, IHearing
{
    public enum Estado{
        Patrullando,
        Buscando,
        SeguirSonido,
        Persiguiendo
    }

    private Estado estadoActual;
    private Vector3 lastPosition;
    private SpriteRenderer sprite;
    private Vector3 rotacionVision;
    public Vector3 velocity;
    private Vector3 acceleration;
    private Transform jugador;
    private AudioSource audioSource;
    private float tiempoSinVerJugador = 0f;

    public Transform conoVision;
    public float velocidadRotacion = 15f;
    
    [Header("Seek")]
    public float maxSpeed = 5f;
    public float maxForce = 0.2f;
    public float tiempoDePersecucion = 8f;
    public float noiseRadius = 5f;

    [Header("Floking")]
    [Tooltip("Radio en el que busca a otros individuos para formar o unirse a una manada.")]
    public float perceptionRadius = 8.0f;
    public float separationRadius = 1.0f;
    public LayerMask GoblinLayer;
    private Vector3 wanderDirection;
    private Vector3 fuerzaWander;

    [Header("Pesos de Comportamiento")]
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;
    public float wanderWeight = 0.5f;
    


    void Start()
    {
        estadoActual = Estado.Patrullando;
        //velocity = Vector3.zero; //se inicializan los vectores a 0, en la web lo que pone es this.velocity = createVector(0, 0);
        acceleration = Vector3.zero; //this.acceleration = createVector(0, 0);
        audioSource = GetComponent<AudioSource>();

        sprite = GetComponentInChildren<SpriteRenderer>();

        //se inica con un movimiento aleatorio (por ahora):
        float min = -10f;
        float max = 10f;
        Vector3 vectorAleatorio = new Vector3(
            Random.Range(min, max),
            Random.Range(min, max),
            0
        );
        velocity = vectorAleatorio.normalized * maxSpeed;
        fuerzaWander = velocity;
    }

    // Update is called once per frame
    void Update()
    {
        if (estadoActual == Estado.Persiguiendo) 
        {
            //CALCULAR EL SEEK:
            
            Seek(jugador.position);

            //aplicar aceleración a la velocidad -> this.velocity.add(this.acceleration);
            velocity += acceleration; 
            //limita la velocidad actual a la velocidad máxima -> this.velocity.limit(this.maxspeed);
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            //=========================================================================================
            //APLICAR EL MOVIMIENTO:

            transform.position += velocity * maxSpeed * Time.deltaTime;
            //transform.position += direccion * Time.deltaTime;
            
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
        else if (estadoActual == Estado.Patrullando)
        {
            //CALCULAR EL FLOKING:

            //buscar goblins cercanos:
            Collider2D[] nearbyGoblins = Physics2D.OverlapCircleAll(transform.position, perceptionRadius, GoblinLayer);

            int vecinos = 0;

            Vector3 cohesion = Vector3.zero;
            Vector3 alineacion = Vector3.zero;
            Vector3 separacion = Vector3.zero;

            //procesar vecinos:
            foreach (Collider2D goblin in nearbyGoblins)
            {
                //ignora a si mismo:
                if (goblin.gameObject == this.gameObject) continue;

                Goblin vecino = goblin.GetComponent<Goblin>();

                if (vecino == null) continue;

                float distancia = Vector3.Distance(transform.position, goblin.transform.position);
                //Vector3 direccion = Vector3.zero;

                //cohesion (sumar posiciones):
                cohesion += goblin.transform.position;
                //alineacion (sumar orientaciones):
                alineacion += vecino.velocity;
                //separacion (en caso de estar muy cerca el uno del otro):
                if (distancia < separationRadius && distancia > 0.001f)
                {
                    separacion += (transform.position - goblin.transform.position) / distancia;
                }

                vecinos++;
            }

            //Debug.Log("Vecinos: " + vecinos);

            //si tiene vecinos, se cambia su comprotamiento:
            if (vecinos > 0)
            {
                // --- EN MANADA ---

                //cohesion:
                cohesion = ((cohesion/vecinos) - transform.position).normalized;
                //alineacion:
                alineacion = (alineacion/vecinos).normalized;
                //separacion:
                separacion = separacion.normalized;

                //combinar las 3 fuerzas:
                Vector3 steering = (cohesion * cohesionWeight) + 
                           (alineacion * alignmentWeight) + 
                           (separacion * separationWeight) + 
                           fuerzaWander;

                velocity = Vector3.Lerp(velocity, steering.normalized * maxSpeed, Time.deltaTime * 3f);
                velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            }
            
            else
            {
                // --- EN SOLITARIO ---

                velocity = Vector3.Lerp(velocity, fuerzaWander.normalized * maxSpeed, Time.deltaTime * 2f);
                velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            }
            

            transform.position += velocity * Time.deltaTime;
        }
        
        //Debug.Log("Velocity: " + velocity);
        /* if (jugador != null && sprite != null)
        {
            if (jugador.position.x < transform.position.x)
            {
                sprite.flipX = true;
                conoVision.localScale = new Vector3(-1, conoVision.localScale.y, 1);
            }
            else
            {
                sprite.flipX = false;
                conoVision.localScale = new Vector3(1, conoVision.localScale.y, 1);
            }
        } */

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

        Debug.Log("Desired: " + desired);
        //Si el goblin todavia NO está sobre el jugador -> desired.setMag(this.maxspeed);
        if (desired.magnitude > 0.01f) {desired = desired.normalized * maxSpeed; } //lo que hace esq el vector desired tenga una longitud igual a maxSpeed

        //fuerza de dirección -> let steer = p5.Vector.sub(desired, this.velocity);
        Vector3 steer = desired - velocity;

        Debug.Log("Steer: " + steer);
        //limitar la fuerza maxima -> steer.limit(this.maxforce);
        steer = Vector3.ClampMagnitude(steer, maxForce);
        
        //aplicar fuerza -> this.applyForce(steer);
        applyForce(steer);
    }



    //===============================
    //DETECTAR PLAYER 
    //===============================
    public void FollowMode(Transform objetivo)
    {
        Scream();
        estadoActual = Estado.Persiguiendo;
        jugador = objetivo;

        tiempoSinVerJugador = 0;
        Debug.Log("¡Jugador detectado!, entrando en modo persecucion");
        //lastPosition = pos;
    }

    //Es lo mismo que FollowMode pero adaptado a escuchar el sonido
    //Se podría cambiar para usar A* tal vez
    public void FollowSound(Transform objetivo)
    {
        estadoActual = Estado.Persiguiendo;//el estado debería ser seguir sonido, pero como no hay nada de eso, pongo perseguir para que pase algo
        //Vector2 sonido = objetivo; //^^Same^^^^^^
        //Todo lo de abajo es simplemente para probar que funciona, cuando lo del sonido se cambia
        GameObject player = new GameObject("PosicionRuido");
        player.transform.position = objetivo.position;
        jugador = objetivo;

        //Lo mismo que con lo de arriba de perseguir, cuando sepamos más sobre lo del sonido lo cambiamos
        tiempoSinVerJugador = 0;
        Debug.Log("¡Jugador detectado!, entrando en modo persecucion");
        //lastPosition = pos;
    }

    //Esta función pretende alertar a los goblins cercanos en un radio marcado en el inspector
    public void Scream()
    {
        audioSource.Play();
        NoiseManager.MakeNoise(transform, noiseRadius);
        Debug.Log("Sonido.MP5");
    }

    //Escuchar el sonido
    public void SetNoisePosition(Transform noisePosition)
    {
        FollowSound(noisePosition);
    }


    //===============================
    //DIBUJAR EL AREA DE DETECCION DEL GOBLIN
    //===============================
    //para dibujar el radio del grito en el editorx
    //y lsa areas del flocking
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, noiseRadius);

        //area efectiva de flocking (en rojo):
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, perceptionRadius);

        //area de espacio perosnal del flocking (en verde):
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
