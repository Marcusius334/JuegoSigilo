using UnityEngine;

public class AcorralandoState : State
{
    public AcorralandoState() 
    {
        transiciones["player_lost"] = typeof(PatrullandoState);
        transiciones["noise_heard"] = typeof(SeguirSonidoState);
    }
    public override void Execute(Enemy enemigo)
    {
        if (enemigo is Goblin goblin)
        {
            EjecutarAcorralamiento(goblin);
        }
    }
    public void EjecutarAcorralamiento(Goblin goblin)
    {
        if (!goblin.TieneObjetivoAcorralamiento())
        {
            /*velocity = Vector3.zero;
            acceleration = Vector3.zero;*/
            goblin.DetenerMovimiento();
            return;
        }

        float distancia = Vector3.Distance(
            goblin.transform.position,
            goblin.ObtenerObjetivoAcorralamiento()
        );

        // Ya ha llegado a la posici�n asignada.
        if (distancia < 0.2f)
        {
            /*velocity = Vector3.zero;
            acceleration = Vector3.zero;*/
            goblin.DetenerMovimiento();
            return;
        }
        goblin.BuscarObjetivoAcorralamiento();
        //Seek(objetivoAcorralamiento);

        // Separaci�n respecto a otros goblins que est�n acorralando.
        Collider2D[] goblinsCercanos = Physics2D.OverlapCircleAll(
            goblin.transform.position,
            goblin.separationRadius * 3f,
            goblin.GoblinLayer
        );

        Vector3 separacion = Vector3.zero;

        foreach (Collider2D collider in goblinsCercanos)
        {
            Goblin otroGoblin = collider.GetComponent<Goblin>();

            if (otroGoblin == null || otroGoblin == goblin)
                continue;

            if (!(otroGoblin.EstadoActual is AcorralandoState))
                continue;

            Vector3 direccion =
                goblin.transform.position - otroGoblin.transform.position;

            float distanciaGoblin = direccion.magnitude;

            if (distanciaGoblin > 0.01f)
            {
                separacion += direccion.normalized / distanciaGoblin;
            }
        }

        goblin.AplicarSeparacionAcorralamiento(separacion);
        goblin.EvitarObstaculos();
        goblin.ActualizarMovimiento();

        //Lo q est� comentado equivale a las 3 lineas de arriba, pero con funciones auxiliares para poder acceder a esas variables
        /*acceleration += separacionAcorralamiento * 2f;

        ObstacleAvoidance();

        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;

        acceleration = Vector3.zero;*/
    }
}