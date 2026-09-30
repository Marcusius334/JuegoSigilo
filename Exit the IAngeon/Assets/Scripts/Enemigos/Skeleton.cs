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

    public Estado estadoActual;
    public Transform jugador;
    public Transform conoVision;
    public float velocidadRotacion = 15f;
    
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
    }

    void Update()
    {
        if (estadoActual == Estado.Persiguiendo) 
        {
            Pursue(jugador.position);

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
