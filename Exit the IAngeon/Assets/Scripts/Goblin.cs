using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Goblin : MonoBehaviour
{
    public enum Estado{
        Patrullando,
        Buscando,
        SeguirSonido,
        Persiguiendo
    }

    private Estado estadoActual;
    private Vector3 lastPosition;

    private Vector3 velocity;
    private Vector3 acceleration;
    private Transform jugador;
    private AudioSource audioSource;

    private float tiempoSinVerJugador = 0f;
    
    [Header("Seek")]
    public float maxSpeed = 5f;
    public float maxForce = 0.2f;
    public float tiempoDePersecucion = 8f;
    public float noiseRadius = 5f;


    void Start()
    {
        estadoActual = Estado.Patrullando;
        velocity = Vector3.zero; //se inicializan los vectores a 0, en la web lo que pone es this.velocity = createVector(0, 0);
        acceleration = Vector3.zero; //this.acceleration = createVector(0, 0);
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (estadoActual == Estado.Persiguiendo) 
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
        Debug.Log("Velocity: " + velocity);

    }
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
    public void FollowSound(Vector2 objetivo)
    {
        estadoActual = Estado.Persiguiendo;//el estado debería ser seguir sonido, pero como no hay nada de eso, pongo perseguir para que pase algo
        //Vector2 sonido = objetivo; //^^Same^^^^^^
        //Todo lo de abajo es simplemente para probar que funciona, cuando lo del sonido se cambia
        GameObject player = new GameObject("PosicionRuido");
        player.transform.position = objetivo;
        jugador = player.transform;

        //Lo mismo que con lo de arriba de perseguir, cuando sepamos más sobre lo del sonido lo cambiamos
        tiempoSinVerJugador = 0;
        Debug.Log("¡Jugador detectado!, entrando en modo persecucion");
        //lastPosition = pos;
    }

    //Esta función pretende alertar a los goblins cercanos en un radio marcado en el inspector
    public void Scream()
    {
        audioSource.Play();
        NoiseManager.MakeNoise(transform.position, noiseRadius);
        Debug.Log("Sonido.MP5");
    }

    //Escuchar el sonido
    public void HearNoise(Vector2 noisePosition)
    {
        Debug.Log("¡He escuchado un ruido!");

        // De momento, simplemente nos movemos hacia el ruido
        Debug.Log("Ruido en: " + noisePosition);

        FollowSound(noisePosition);
    }

    //para dibujar el radio del grito en el editor
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, noiseRadius);
    }
}
