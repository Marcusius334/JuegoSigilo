using UnityEngine;

public static class NoiseManager
{
    // Para que los enemigos que no escuchan al andar funcionen bn (para que no haya conflictos en otros enemigos mientras se trabajaba)
    public static void MakeNoise(Vector2 position, float radius)
    {
        MakeNoise(position, radius, true);
    }

    // Para los sonidos del jugador
    public static void MakeNoise(Vector2 position, float radius, bool isRunning)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(position, radius);

        foreach (Collider2D enemy in enemies)
        {
            EnemyHearing hearing = enemy.GetComponentInParent<EnemyHearing>();

            if (hearing != null)
            {
                hearing.HearNoise(position, isRunning);
            }
        }
    }
}