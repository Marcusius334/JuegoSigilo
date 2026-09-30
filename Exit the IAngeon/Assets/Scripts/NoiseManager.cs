using UnityEngine;

public static class NoiseManager
{
    // Para los sonidos que ya existían
    public static void MakeNoise(Transform position, float radius)
    {
        MakeNoise(position, radius, true);
    }

    // Para los sonidos del jugador
    public static void MakeNoise(Transform position, float radius, bool isRunning)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(position.position, radius);

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