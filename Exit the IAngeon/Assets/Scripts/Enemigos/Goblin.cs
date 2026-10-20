using UnityEngine;
using System.Collections.Generic;

public class Goblin : Enemy
{    
    [Header("Floking")]
    [Tooltip("Radio en el que busca a otros individuos para formar o unirse a una manada.")]
    public LayerMask GoblinLayer;
    private Vector3 fuerzaWander;
    public float perceptionRadius = 8.0f;
    public float separationRadius = 3.0f;

    [Header("Pesos de Comportamiento")]
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;
    public float wanderWeight = 0.5f;

    [Header("Acorralamiento")]
    public float radioAcorralamiento = 15f;
    public float distanciaParaAcorralar = 20f;
    public float anguloAcorralamiento = 360f;
    public int maxGoblinsAcorralamiento = 6;
    private bool haVistoPersonalmenteAlJugador = false;// Indica si ESTE goblin ha visto personalmente al jugador.
    private Goblin liderGoblin; // Si está persiguiendo porque otro goblin le avisó, aquí guardamos quién fue ese goblin.
    private float ultimoMomentoVioJugador = -Mathf.Infinity;// Último momento en el que este goblin vio personalmente al jugador.
    private Vector3 objetivoAcorralamiento;
    private bool tieneObjetivoAcorralamiento;
    private bool acorralamientoIniciado = false;
    

    void Start()
    {
        

        if (puntosDePatrulla != null && puntosDePatrulla.Length > 0)   //Empezara a patrullar dados los puntos en el inspector
        {
            InicializarMaquinaEstados<PatrullandoState>(typeof(PatrullandoState),typeof(BuscandoState),typeof(PersiguiendoState),typeof(SeguirSonidoState),typeof(AcorralandoState));
        }
        else
        {
            InicializarMaquinaEstados<BuscandoState>(typeof(PatrullandoState),typeof(BuscandoState),typeof(PersiguiendoState),typeof(SeguirSonidoState),typeof(AcorralandoState));
        }
        fuerzaWander = Vector3.zero;    //La fuerza a cero patatero

    }

    //===============================
    //UPDATE
    //===============================
    void Update()
    {
        if (maquinaEstados == null)
            return;

        maquinaEstados.ActualizarEstado();

        // Rotación del cono de visión según el movimiento.
        rotacionVision = velocity.normalized;

        if (rotacionVision != Vector3.zero && conoVision != null)
        {
            float angulo = Mathf.Atan2(
                rotacionVision.y,
                rotacionVision.x
            ) * Mathf.Rad2Deg;

            Quaternion rotacionObjetivo =
                Quaternion.Euler(0f, 0f, angulo);

            conoVision.rotation = Quaternion.Slerp(
                conoVision.rotation,
                rotacionObjetivo,
                velocidadRotacion * Time.deltaTime
            );
        }
    }


    //===============================
    //ESTADO BUSCANDO
    //===============================
    public override void EjecutarBuscando()
    {
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
    }

    //===============================
    //ESTADO SEGUIR SONIDO
    //===============================
    public override void EjecutarSeguirSonido()
    {
        Debug.Log("ENTRA1");
        if (tienePosicionSonido)
        {
            Debug.Log("ENTRA2");
            Vector3 siguienteNodo = ObtenerSiguienteNodoAStar(posicionSonido);
            Seek(siguienteNodo);
            ObstacleAvoidance();

            velocity += acceleration;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            transform.position += velocity * Time.deltaTime;
            acceleration = Vector3.zero;

            // Si ha llegado a la posición del sonido, vuelve a buscar.
            if (Vector3.Distance(transform.position, posicionSonido) < distanciaNodoAStar)
            {
                tienePosicionSonido = false;
                playerTrn = null;

                ReiniciarCaminoAStar();
                velocity = Vector3.zero;
                acceleration = Vector3.zero;

                if (puntosDePatrulla != null && puntosDePatrulla.Length > 0)
                {
                    EncontrarPuntoMasCercano();
                    LanzarEvento("sound_reached_patrol");
                }
                else
                {
                    LanzarEvento("sound_reached");
                }
            }

        }
        else
        {
            ReiniciarCaminoAStar();
            velocity = Vector3.zero;
            LanzarEvento("sound_reached");
        }
        return;
    }

    //===============================
    //ESTADO PERSECUCIÓN
    //===============================
    public override void EjecutarPersecucion()
    {
        // Si no hay jugador objetivo, no podemos perseguirlo.
        if (playerTrn == null)
        {
            velocity = Vector3.zero;
            acceleration = Vector3.zero;
            LanzarEvento("player_lost");
            return;
        }

        // Buscar y desplazarse hacia el jugador.
        Seek(playerTrn.position);
        ObstacleAvoidance();

        //aplicar aceleración a la velocidad -> this.velocity.add(this.acceleration);
        velocity += acceleration;
        //limita la velocidad actual a la velocidad máxima -> this.velocity.limit(this.maxspeed);
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        //APLICAR EL MOVIMIENTO:
        transform.position += velocity * Time.deltaTime;
        //resetear la aceleracion -> this.acceleration.mult(0);
        acceleration = Vector3.zero;

        // Iniciar el acorralamiento si este goblin vio al jugador.
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

            // CONTROL DEL TIEMPO SIN VER AL JUGADOR
            tiempoSinVerJugador = Time.time - ultimoMomentoVioJugador;

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
    }


    //===============================
    //ESTADO PATRULLA
    //===============================
    public override void EjecutarPatrulla()
    {
        fuerzaWander = CalcularDireccionPatrulla();// Calcula la ruta
        

        //buscar goblins cercanos:
        Collider2D[] nearbyGoblins = Physics2D.OverlapCircleAll(
            transform.position,
            perceptionRadius,
            GoblinLayer
        );

        int vecinos = 0;

        Vector3 cohesion = Vector3.zero;
        Vector3 alineacion = Vector3.zero;
        Vector3 separacion = Vector3.zero;

        foreach (Collider2D goblin in nearbyGoblins)
        {
            //ignora a si mismo
            if (goblin.gameObject == gameObject) continue;

            Goblin vecino = goblin.GetComponent<Goblin>();
            if (vecino == null) continue;

            float distancia = Vector3.Distance(transform.position,vecino.transform.position);

            if (distancia > 0.0001f)
            {
                // Cohesión: Acumular posiciones relativas o absolutas
                cohesion += vecino.transform.position;
                
                // Alineación: Acumular velocidades
                alineacion += vecino.velocity;

                // Separación: Inversamente proporcional al CUADRADO de la distancia
                if (distancia < separationRadius)
                {
                    Vector3 direccionAlejamiento = (transform.position - vecino.transform.position).normalized;
                    // Cuanto más cerca estén, exponencialmente mayor será la fuerza de repulsión
                    separacion += direccionAlejamiento / (distancia * distancia);
                }

                vecinos++;
            }
        }

        Vector3 steering = Vector3.zero;

        //si tiene vecinos, se cambia su comprotamiento:
        if (vecinos > 0)
        {
            // --- EN MANADA ---
            // Cohesión: Obtener el centro de masa y calcular dirección hacia él
            Vector3 centroDeMasa = cohesion / vecinos;
            cohesion = (centroDeMasa - transform.position).normalized;

            // Alineación: Obtener velocidad media
            alineacion = (alineacion / vecinos).normalized;

            // Separación: Normalizar el vector resultante de repulsión
            if (separacion != Vector3.zero)
            {
                separacion = separacion.normalized;
            }

            //combinar las 3 fuerzas:
            steering =
                cohesion * cohesionWeight +
                alineacion * alignmentWeight +
                separacion * separationWeight;

            // 3. Combinar Flocking con el objetivo principal (WayPoints/Patrulla)
            // fuerzaWander representa la dirección al Waypoint
            Vector3 direccionFinal = (fuerzaWander.normalized * wanderWeight) + steering;

            if (direccionFinal != Vector3.zero)
            {
                // Aplicar fuerza de movimiento progresiva
                Vector3 velocidadDeseada = direccionFinal.normalized * maxSpeed;
                velocity = Vector3.Lerp(velocity, velocidadDeseada, Time.deltaTime * 5f);
            }
        }
        else
        {
            // --- EN SOLITARIO --
            velocity = Vector3.Lerp(velocity,fuerzaWander.normalized * maxSpeed,Time.deltaTime * 2f);
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        }
        ObstacleAvoidance();// Esquiva las paredes
        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;
    }

    //===============================
    //ESTADO ACORRALAMIENTO: el acorralamiento, al ser una función única de los Goblins, está programada en el script AcorralandoState.cs
    //===============================



//--------------------------------------------------------------------------------------------------------------------------------
    //===============================
    //MÉTODOS AUXILIARES (se usan en el AcorralandoState.cs para poder acceder a las variables de goblin y funciones heredadas de Enemy)
    //===============================

    public bool TieneObjetivoAcorralamiento(){return tieneObjetivoAcorralamiento;}

    public Vector3 ObtenerObjetivoAcorralamiento(){return objetivoAcorralamiento;}

    public void BuscarObjetivoAcorralamiento(){Seek(objetivoAcorralamiento);}

    public void AplicarSeparacionAcorralamiento(Vector3 separacion){acceleration += separacion * 2f;}

    public void EvitarObstaculos(){ObstacleAvoidance();}

    public void ActualizarMovimiento()
    {
        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;

        acceleration = Vector3.zero;
    }

    public void DetenerMovimiento()
    {
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
    }







    //-----------------------------------------------------------------------------------------------------------------------//





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
            LanzarEvento("player_lost_patrol");
        }
        else 
        {
            LanzarEvento("player_lost_search");

        }
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

        if (puntosDePatrulla != null && puntosDePatrulla.Length > 0)
        {
            EncontrarPuntoMasCercano(); //  Busca el punto mas cercano, tambien temporal hasta el Pathfollowing A*
            //CambiarEstado<PatrullandoState>();
            LanzarEvento("leader_lost_patrol");
        }
        else 
        {
            //CambiarEstado<BuscandoState>();
            LanzarEvento("leader_lost_search");
        }
            

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
            if (goblin.EstadoActual is AcorralandoState) continue;

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

        LanzarEvento("start_surround");
    }



    //===============================
    //DETECTAR PLAYER 
    //===============================
    public void FollowMode(Transform objetivo)
    {
        if (objetivo == null) return;

        playerTrn = objetivo;

        //Los Goblins que entran en FOLLOWMODE son los que SI han visto al jugador (no los que escuchan)
        haVistoPersonalmenteAlJugador = true;
        liderGoblin = null; //No tiene lider, él es el líder 
        ultimoMomentoVioJugador = Time.time;  // Cada vez que realmente ve al jugador,actualizamos el momento de la última visión.
        tiempoSinVerJugador = 0f;

        LanzarEvento("player_detected");
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
        Debug.Log("Scream");
    }

    //Escuchar el sonido
    public override void SetNoisePosition(Transform noisePosition)
    {
        if (noisePosition == null)
            return;

        Goblin lider = noisePosition.GetComponent<Goblin>();

        if (lider != null)
            FollowSound(lider.PlayerTransform(), lider);
        else
            FollowSound(noisePosition, null);
    }

     //Es lo mismo que FollowMode pero adaptado a escuchar el sonido
    //Se podría cambiar para usar A* tal vez
    public void FollowSound(Transform objetivo, Goblin lider)
    {
        if (haVistoPersonalmenteAlJugador || objetivo== null)
            return;
        //Vector2 sonido = objetivo; //^^Same^^^^^^
        //Todo lo de abajo es simplemente para probar que funciona, cuando lo del sonido se cambia
        playerTrn = objetivo;
        posicionSonido = objetivo.position;
        tienePosicionSonido = true;

        haVistoPersonalmenteAlJugador = false;
        liderGoblin = lider; //Guardamos el Goblin que le ha avisado (lider)
        tiempoSinVerJugador = 0f;

        ReiniciarCaminoAStar();
        LanzarEvento("noise_heard");
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
