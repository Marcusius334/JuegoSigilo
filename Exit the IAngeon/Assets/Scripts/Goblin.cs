using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Goblin : MonoBehaviour
{
    public enum Estado{
        Buscando,
        Persiguiendo
    }

    private Estado estadoActual;
    private Vector3 lastPosition;

    private Vector3 velocity;
    private Vector3 acceleration;
    private Transform jugador;

    private float tiempoSinVerJugador = 0f;
    
    [Header("Seek")]
    public float maxSpeed = 5f;
    public float maxForce = 0.2f;
    public float tiempoDePersecucion = 8f;


    void Start()
    {
        estadoActual = Estado.Buscando;
        velocity = Vector3.zero; //se inicializan los vectores a 0, en la web lo que pone es this.velocity = createVector(0, 0);
        acceleration = Vector3.zero; //this.acceleration = createVector(0, 0);

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
                velocity = Vector3.zero;
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
        estadoActual = Estado.Persiguiendo;
        jugador = objetivo;

        tiempoSinVerJugador = 0;
        Debug.Log("¡Jugador detectado!, entrando en modo persecucion");
        //lastPosition = pos;
    }
}
