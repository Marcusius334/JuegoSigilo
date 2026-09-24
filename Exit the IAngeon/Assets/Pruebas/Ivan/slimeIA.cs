using UnityEngine;

public class slimeIA : MonoBehaviour
{
    public enum Estado
    {
        busqueda,
        persecucion
    }

    public SpriteRenderer sprite;

    [Header("Parámetros de Wander")]
    public float wanderRadius = 2.0f;
    public float wanderDistance = 4.0f;
    public float wanderJitter = 0.5f;

    [Header("Parámetros de Evasión / Rebote")]
    public float avoidDistance = 1.5f; // Distancia para detectar la pared antes del choque
    public LayerMask obstacleMask;

    [Header("Movimiento Constante")]
    public float maxSpeed = 5.0f;
    
    private Vector2 wanderTarget;
    private Vector2 moveDirection; // Dirección actual estandarizada

    private Estado estadoActual;

    void Start()
    {
        estadoActual = Estado.busqueda;

        // Inicializamos una dirección aleatoria y la normalizamos
        moveDirection = Random.insideUnitCircle.normalized;
        
        // Punto inicial dentro del círculo de wander
        wanderTarget = Random.insideUnitCircle * wanderRadius;
    }

    void Update()
    {
        if (estadoActual == Estado.busqueda)
        {
            // 1. Primero comprobamos si hay una pared al frente para rebotar
            CheckWallCollision();

            // 2. Si no rebotó, aplicamos la variación de dirección ligera del Wander
            ApplyWanderDirect();

            // 3. Movimiento a VELOCIDAD CONSTANTE
            transform.Translate(moveDirection * maxSpeed * Time.deltaTime, Space.World);
        }

        if (moveDirection.x < 0) sprite.flipX = true;
        else sprite.flipX = false;
    }

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
}