using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Skeleton : Enemy
{
    void Start()
    {
        estadoActual = Estado.Buscando;
    }

    //===============================
    //UPDATE
    //===============================
    void Update()
    {
        if (estadoActual == Estado.Persiguiendo) 
        {
            Pursue(playerTrn.position);
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

    //===============================
    //FOLLOW MODE
    //===============================
    public void FollowMode(Transform objetivo)
    {
        estadoActual = Estado.Persiguiendo;
        playerTrn = objetivo;

        Debug.Log("¡Jugador detectado!, entrando en modo persecucion");
        //lastPosition = pos;
    }

    //===============================
    //COSAS VISION HITBOX
    //===============================
    public bool EstaPersiguiendo()
    {
        return estadoActual == Estado.Persiguiendo;
    }
    public Transform PlayerTransform()//para que lo de la vision hitbox vaya bien
    {
        return playerTrn;
    }
}
