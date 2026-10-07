using NUnit.Framework;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;

public class Goblin : Enemy
{    
    [Header("Floking")]
    [Tooltip("Radio en el que busca a otros individuos para formar o unirse a una manada.")]
    public LayerMask GoblinLayer;
    private Vector3 fuerzaWander;
    public float perceptionRadius = 8.0f;
    public float separationRadius = 1.0f;

    [Header("Pesos de Comportamiento")]
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;
    public float wanderWeight = 0.5f;

    [Header("Acorralamiento")]
    private bool haVistoPersonalmenteAlJugador = false;// Indica si ESTE goblin ha visto personalmente al jugador.
    private Goblin liderGoblin; // Si está persiguiendo porque otro goblin le avisó, aquí guardamos quién fue ese goblin.
    private float ultimoMomentoVioJugador = -Mathf.Infinity;// Último momento en el que este goblin vio personalmente al jugador.
    private Vector3 objetivoAcorralamiento;
    private bool tieneObjetivoAcorralamiento;
    private bool acorralamientoIniciado = false;
    public float radioAcorralamiento = 15f;
    public float distanciaParaAcorralar = 20f;
    public float anguloAcorralamiento = 360f;
    public int maxGoblinsAcorralamiento = 6;

    void Start()
    {
       
        if (puntosDePatrulla != null && puntosDePatrulla.Length > 0)   //Empezara a patrullar dados los puntos en el inspector
        {
            InicializarMaquinaEstados(Estado.Patrullando);
        }
        else
        {
            InicializarMaquinaEstados(Estado.Buscando);
        }
        
        
        fuerzaWander = Vector3.zero;    //La fuerza a cero patatero
       
       
       /* estadoActual = Estado.Buscando;                Por si acaso lo comento xd!!!!
        
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
        fuerzaWander = Vector3.zero;*/
    }

    //===============================
    //UPDATE
    //===============================
    void Update()
    {
        if (EstadoActual == Estado.Buscando)
        {
            velocity = Vector3.zero;
            acceleration = Vector3.zero;
        }
        else if (EstadoActual == Estado.SeguirSonido) 
        {
            if (playerTrn == null) return;
            Seek(playerTrn.position);
            ObstacleAvoidance();

            velocity += acceleration;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            transform.position += velocity * Time.deltaTime;
            acceleration = Vector3.zero;
        }
        else if (EstadoActual == Estado.Persiguiendo)
        {
            //CALCULAR EL SEEK:

            Seek(playerTrn.position);
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

            if (haVistoPersonalmenteAlJugador && playerTrn != null)
            {
                float distanciaJugador = Vector3.Distance(
                    transform.position,
                    playerTrn.position
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
        else if (EstadoActual == Estado.Patrullando)
        {
            fuerzaWander = CalcularDireccionPatrulla(); // Calcula la ruta
            ObstacleAvoidance();                        // Esquiva las paredes

            
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
        else if (EstadoActual == Estado.Acorralando)
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

            //separamos de otros goblins que también estén acorralando
            Collider2D[] goblinsCercanos = Physics2D.OverlapCircleAll(transform.position, separationRadius * 3f, GoblinLayer);

            Vector3 separacionAcorralamiento = Vector3.zero;
            foreach (Collider2D collider in goblinsCercanos) 
            {
                Goblin goblin = collider.GetComponent<Goblin>();
                if (goblin == null || goblin == this) continue;
                if (goblin.EstadoActual != Estado.Acorralando) continue;
                Vector3 direccion = transform.position - goblin.transform.position;
                float distanciaGoblin = direccion.magnitude;

                if (distanciaGoblin > 0.01f) 
                {
                    separacionAcorralamiento += direccion.normalized / distanciaGoblin;
                }
            }
            acceleration += separacionAcorralamiento * 2f;

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

    //===============================
    //PERDER JUGADOR
    //===============================
    void PerderJugador()
    {
        velocity = Vector3.zero;
        acceleration = Vector3.zero;

        if (puntosDePatrulla != null && puntosDePatrulla.Length > 0)
        {

            EncontrarPuntoMasCercano(); //  Busca el punto mas cercano, tambien temporal hasta el Pathfollowing A*
            CambiarEstado(Estado.Patrullando);
        }
        else
            CambiarEstado(Estado.Buscando);


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

        if (puntosDePatrulla != null && puntosDePatrulla.Length > 0){
            EncontrarPuntoMasCercano(); //  Busca el punto mas cercano, tambien temporal hasta el Pathfollowing A*
            InicializarMaquinaEstados(Estado.Patrullando);
        }
        else
            InicializarMaquinaEstados(Estado.Buscando);

        liderGoblin = null;
        playerTrn = null;

        tieneObjetivoAcorralamiento = false;
        tiempoSinVerJugador = 0f;
    }


    //===============================
    //COMPORTAMIENTO INTELIGENTE (Acorralamiento)
    //===============================
    public Transform PlayerTransform() 
    {
        return playerTrn;
    }
    public void IniciarAcorralamiento() 
    {
        if (playerTrn == null) return;

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
            if(goblin == null) continue;
            if (goblin.EstadoActual == Estado.Acorralando) continue;

            goblinsDisponibles.Add(goblin);
        }
        // Si no hay otros Goblins, no se puede formar
        if(goblinsDisponibles.Count == 0) return;

        // No utilizar más Goblins de los permitidos 
        int cantidad = Mathf.Min(
            goblinsDisponibles.Count,
            maxGoblinsAcorralamiento
        );

        //-----CREAR LAS POSICIONES DEL CÍRCULO----

        float anguloInicial = Random.Range(0f, 360f);
        //Separacion entre las posiciones 
        float anguloEntreGoblins = 360f / cantidad;
        List<Vector3> posicionesDisponibles = new List<Vector3>();

        for (int i = 0; i < cantidad; i++) 
        {
            float angulo = anguloInicial + i * anguloEntreGoblins;
            float radianes = angulo * Mathf.Deg2Rad;

            Vector3 direccion = new Vector3(Mathf.Cos(radianes), Mathf.Sin(radianes), 0);

            //Posicion alrededor del jugador 
            Vector3 posicion = playerTrn.position + direccion * radioAcorralamiento;

            
            posicionesDisponibles.Add(posicion);
        }

        //-----ASIGNAR CADA GOBLIN A LA POSICIÓN MÁS CERCANA A ÉL----
        for (int i = 0; i < cantidad; i++) 
        {
            Goblin goblinMasCercano = null;
            Vector3 posicionElegida = Vector3.zero;

            float distanciaMinima = Mathf.Infinity;
            int indicePosicionElegida = -1;

            //Buscar qué combinacion goblin->posicion tiene sentido para el acorralamiento
            foreach (Goblin goblin in goblinsDisponibles) 
            {
                for (int j = 0; j < posicionesDisponibles.Count; j++) 
                {
                    float distancia = Vector3.Distance(goblin.transform.position, posicionesDisponibles[j]);
                    if (distancia < distanciaMinima) 
                    {
                        distanciaMinima = distancia;
                        goblinMasCercano = goblin;
                        posicionElegida = posicionesDisponibles [j];
                        indicePosicionElegida = j;
                    }
                }
            }

            //si encontramos una combinacion válida
            if (goblinMasCercano != null) 
            {
                goblinMasCercano.IrAcorralamiento(posicionElegida);
                //Esa posicion ya está ocupada
                posicionesDisponibles.RemoveAt(indicePosicionElegida);
                //Ese goblin ya tiene su posición
                goblinsDisponibles.Remove(goblinMasCercano);
            }
        }

    }
    public void IrAcorralamiento(Vector3 posicion) 
    {
        objetivoAcorralamiento = posicion;
        tieneObjetivoAcorralamiento = true;

        velocity = Vector3.zero;
        acceleration = Vector3.zero;

        CambiarEstado(Estado.Acorralando);
    }



    //===============================
    //DETECTAR PLAYER 
    //===============================
    public void FollowMode(Transform objetivo)
    {
        
     
        playerTrn = objetivo;

        //Los Goblins que entran en FOLLOWMODE son los que SI han visto al jugador (no los que escuchan)
        haVistoPersonalmenteAlJugador = true;
        liderGoblin = null; //No tiene lider, él es el líder 
        ultimoMomentoVioJugador = Time.time;  // Cada vez que realmente ve al jugador,actualizamos el momento de la última visión.
        tiempoSinVerJugador = 0f;

        CambiarEstado(Estado.Persiguiendo);
        Scream();

        AvisarPosicionJugador();
    }
    private void AvisarPosicionJugador()
    {
        if (playerTrn == null) return;

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

            goblin.FollowSound(playerTrn, this);
        }
    }

    //===============================
    //NOISE
    //===============================
    //Esta función pretende alertar a los goblins cercanos en un radio marcado en el inspector
    public void Scream()
    {
        audioSourceExtra.Play();
        NoiseManager.MakeNoise(transform, noiseRadius);
        Debug.Log("Sonido.MP5");
    }

    //Escuchar el sonido
    public override void SetNoisePosition(Transform noisePosition)
    {
        Goblin lider = noisePosition.GetComponent<Goblin>();
        if (lider != null)
            FollowSound(lider.PlayerTransform(), lider);
        else
            FollowSound(noisePosition, lider);
    }

     //Es lo mismo que FollowMode pero adaptado a escuchar el sonido
    //Se podría cambiar para usar A* tal vez
    public void FollowSound(Transform objetivo, Goblin lider)
    {
        if (haVistoPersonalmenteAlJugador)
            return;
        //Vector2 sonido = objetivo; //^^Same^^^^^^
        //Todo lo de abajo es simplemente para probar que funciona, cuando lo del sonido se cambia
        playerTrn = objetivo;

        haVistoPersonalmenteAlJugador = false;
        liderGoblin = lider; //Guardamos el Goblin que le ha avisado (lider)
        tiempoSinVerJugador = 0f;

        CambiarEstado(Estado.SeguirSonido);
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
