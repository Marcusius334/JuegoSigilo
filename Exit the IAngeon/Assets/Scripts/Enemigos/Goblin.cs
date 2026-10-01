using NUnit.Framework;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;

public class Goblin : MonoBehaviour, IHearing
{
    public enum Estado{
        Patrullando,
        Buscando, //TODOS LOS SITIOS LOS QUE PONGA BUSCANDO HAY Q SUSTITUIRLOS POR PATRULLANDO (he usado el estado Buscando para hacer pruebas solo )
        SeguirSonido,
        Persiguiendo,
        Acorralando
    }

    private Estado estadoActual;
    private Vector3 lastPosition;
    private SpriteRenderer sprite;
    private Vector3 rotacionVision;

    //SEEK
    public Vector3 velocity;
    private Vector3 acceleration;
    private Transform jugador;
    private AudioSource audioSource;
    public float tiempoSinVerJugador = 0f;
    private int avoidanceSide;
    private bool haVistoPersonalmenteAlJugador = false;// Indica si ESTE goblin ha visto personalmente al jugador.
    private Goblin liderGoblin; // Si está persiguiendo porque otro goblin le avisó, aquí guardamos quién fue ese goblin.
    private float ultimoMomentoVioJugador = -Mathf.Infinity;// Último momento en el que este goblin vio personalmente al jugador.

    //ACORRALAMIENTO 
    private Vector3 objetivoAcorralamiento;
    private bool tieneObjetivoAcorralamiento;
    private bool acorralamientoIniciado = false;

    [Header("Acorralamiento")]
    public float radioAcorralamiento = 3f;
    public float distanciaParaAcorralar = 20f;
    public float anguloAcorralamiento = 360f;
    public int maxGoblinsAcorralamiento = 6;

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

    public LayerMask obstacleLayer;
    public float obstacleDistance = 3f;
    public float emergencyDistance = 0.5f;
    


    void Start()
    {
        estadoActual = Estado.Buscando;
        velocity = Vector3.zero; //se inicializan los vectores a 0, en la web lo que pone es this.velocity = createVector(0, 0);
        acceleration = Vector3.zero; //this.acceleration = createVector(0, 0);
        audioSource = GetComponent<AudioSource>();
        avoidanceSide = 0;//Cosa de obstacle

        sprite = GetComponentInChildren<SpriteRenderer>();

        //se inica con un movimiento aleatorio (por ahora):
        float min = -10f;
        float max = 10f;
        Vector3 vectorAleatorio = new Vector3(
            Random.Range(min, max),
            Random.Range(min, max),
            0
        );
        //velocity = vectorAleatorio.normalized * maxSpeed; para que inicien con una posicion aleatoria 
        //fuerzaWander = velocity;
        fuerzaWander = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (estadoActual == Estado.Buscando)
        {
            velocity = Vector3.zero;
            acceleration = Vector3.zero;
        }
        else if (estadoActual == Estado.SeguirSonido) 
        {
            if (jugador == null) return;
            Seek(jugador.position);
            ObstacleAvoidance();

            velocity += acceleration;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            transform.position += velocity * Time.deltaTime;
            acceleration = Vector3.zero;
        }
        else if (estadoActual == Estado.Persiguiendo)
        {
            //CALCULAR EL SEEK:

            Seek(jugador.position);
            ObstacleAvoidance();

            //aplicar aceleración a la velocidad -> this.velocity.add(this.acceleration);
            velocity += acceleration;
            //limita la velocidad actual a la velocidad máxima -> this.velocity.limit(this.maxspeed);
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            //=========================================================================================
            //APLICAR EL MOVIMIENTO:

            transform.position += velocity  * Time.deltaTime;
            //transform.position += direccion * Time.deltaTime;

            //resetear la aceleracion -> this.acceleration.mult(0);
            acceleration = Vector3.zero;

            if (haVistoPersonalmenteAlJugador && jugador != null)
            {
                float distanciaJugador = Vector3.Distance(
                    transform.position,
                    jugador.position
                );

                if (distanciaJugador <= distanciaParaAcorralar && !acorralamientoIniciado)
                {
                    acorralamientoIniciado = true;
                    IniciarAcorralamiento();
                }
            }

            // =====================================================
            // CONTROL DEL TIEMPO SIN VER AL JUGADOR
            // =====================================================

            if (haVistoPersonalmenteAlJugador)
            {
                tiempoSinVerJugador = Time.time - ultimoMomentoVioJugador;

                if (tiempoSinVerJugador >= tiempoDePersecucion)
                {
                    // Este goblin ha perdido al jugador.
                    PerderJugador();
                }
            }
        }
        else if (estadoActual == Estado.Patrullando)
        {
            //CALCULAR EL FLOKING: //Solo se hace flockin g cuando patrullan juntos 

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

            //si tiene vecinos, se cambia su comprotamiento:
            if (vecinos > 0)
            {
                // --- EN MANADA ---

                //cohesion:
                cohesion = ((cohesion / vecinos) - transform.position).normalized;
                //alineacion:
                alineacion = (alineacion / vecinos).normalized;
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
        else if (estadoActual == Estado.Acorralando)
        {
            if (!tieneObjetivoAcorralamiento) return;

            float distancia = Vector3.Distance(transform.position, objetivoAcorralamiento);

            if (distancia < 0.2f)// Si ya ha llegado a su posición
            {
                velocity = Vector3.zero;
                acceleration = Vector3.zero;
                return;
            }

            Seek(objetivoAcorralamiento);
            ObstacleAvoidance();

            velocity += acceleration;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            transform.position += velocity * Time.deltaTime;

            acceleration = Vector3.zero;
        }



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

    //===============================
    //SEEK
    //===============================

    void PerderJugador()
    {
        velocity = Vector3.zero;
        acceleration = Vector3.zero;

        estadoActual = Estado.Buscando;

        haVistoPersonalmenteAlJugador = false;
        liderGoblin = null;

        tieneObjetivoAcorralamiento = false;
        objetivoAcorralamiento = Vector3.zero;

        acorralamientoIniciado = false;

        tiempoSinVerJugador = 0f;

        // Avisamos a los goblins que estaban persiguiendo
        // porque este goblin les había avisado.
        AvisarPerdidaDelJugador();
    }
    void AvisarPerdidaDelJugador()
    {
        Collider2D[] goblinsCercanos = Physics2D.OverlapCircleAll(
            transform.position,
            noiseRadius,
            GoblinLayer
        );

        foreach (Collider2D collider in goblinsCercanos)
        {
            Goblin goblin = collider.GetComponent<Goblin>();

            if (goblin == null || goblin == this)
                continue;

            // Solo cancelamos a los goblins que estaban
            // siguiendo a ESTE líder.
            if (goblin.liderGoblin == this)
            {
                goblin.PerderPersecucionPorLider();
            }
        }
    }
    void PerderPersecucionPorLider()
    {
        // Si este goblin ya había visto personalmente
        // al jugador, NO debe detenerse.
        if (haVistoPersonalmenteAlJugador)
        {
            return;
        }

        velocity = Vector3.zero;
        acceleration = Vector3.zero;

        estadoActual = Estado.Buscando;

        liderGoblin = null;
        jugador = null;

        tieneObjetivoAcorralamiento = false;
        tiempoSinVerJugador = 0f;
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

    //===============================
    //COMPORTAMIENTO INTELIGENTE (Acorralamiento)
    //===============================
    public void IniciarAcorralamiento() 
    {
        if (jugador == null) return;

        Collider2D[] goblinsCercanos = Physics2D.OverlapCircleAll(
        transform.position,
        noiseRadius,
        GoblinLayer
        );
        List<Goblin> goblinsDisponibles = new List<Goblin>();

        // Convertir los Collider2D encontrados en Goblins
        foreach (Collider2D collider in goblinsCercanos) 
        {
            Goblin goblin = collider.GetComponent<Goblin>();
            if (goblin == null || goblin == this) continue;

            goblinsDisponibles.Add(goblin);
        }
        // Si no hay otros Goblins, no se puede formar
        if(goblinsDisponibles.Count == 0) return;

        // No utilizar más Goblins de los permitidos 
        int cantidad = Mathf.Min(
            goblinsDisponibles.Count,
            maxGoblinsAcorralamiento
        );

        //Separacion entre las posiciones 
        float anguloEntreGoblins = 360f / cantidad;

        for (int i = 0; i < cantidad; i++) 
        {
            float angulo = i * anguloEntreGoblins;
            float radianes = angulo * Mathf.Deg2Rad;

            //Posicion alrededor del jugador 
            Vector3 posicion = jugador.position + new Vector3(Mathf.Cos(radianes), Mathf.Sin(radianes), 0) * radioAcorralamiento;

            //Mandar al Goblin a esa posicion 
            goblinsDisponibles[i].IrAcorralamiento(posicion);
        }

    }
    public void IrAcorralamiento(Vector3 posicion) 
    {
        objetivoAcorralamiento = posicion;
        tieneObjetivoAcorralamiento = true;

        velocity = Vector3.zero;
        acceleration = Vector3.zero;

        estadoActual = Estado.Acorralando;
    }



    //===============================
    //DETECTAR PLAYER 
    //===============================
    public void FollowMode(Transform objetivo)
    {
        Scream();
        estadoActual = Estado.Persiguiendo;
        jugador = objetivo;

        //Los Goblins que entran en FOLLOWMODE son los que SI han visto al jugador (no los que escuchan)
        haVistoPersonalmenteAlJugador = true;
        liderGoblin = null; //No tiene lider, él es el líder 
        ultimoMomentoVioJugador = Time.time;  // Cada vez que realmente ve al jugador,actualizamos el momento de la última visión.
        tiempoSinVerJugador = 0f;

        AvisarPosicionJugador();
    }
    private void AvisarPosicionJugador()
    {
        if (jugador == null) return;

        Collider2D[] goblinsCercanos = Physics2D.OverlapCircleAll(
            transform.position,
            noiseRadius,
            GoblinLayer
        );

        foreach (Collider2D collider in goblinsCercanos)
        {
            Goblin goblin = collider.GetComponent<Goblin>();

            if (goblin == null || goblin == this)
                continue;

            goblin.FollowSound(jugador, this);
        }
    }
    //Es lo mismo que FollowMode pero adaptado a escuchar el sonido
    //Se podría cambiar para usar A* tal vez
    public void FollowSound(Transform objetivo, Goblin lider)
    {
        estadoActual = Estado.SeguirSonido;
        //Vector2 sonido = objetivo; //^^Same^^^^^^
        //Todo lo de abajo es simplemente para probar que funciona, cuando lo del sonido se cambia
        jugador = objetivo;

        haVistoPersonalmenteAlJugador = false;
        liderGoblin = lider; //Guardamos el Goblin que le ha avisado (lider)
        tiempoSinVerJugador = 0f;
        
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
        Goblin lider = noisePosition.GetComponent<Goblin>();

        FollowSound(noisePosition, lider);
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
