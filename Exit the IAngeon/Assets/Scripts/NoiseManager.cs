using UnityEngine;

public static class NoiseManager
{
    public static void MakeNoise(Vector2 position, float radius)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(position, radius);

        foreach (Collider2D enemy in enemies)
        {
            //Para comprovar que solo se detectan las colisiones de los enemies
            //y no sus conos de visión o similares
            if (!enemy.CompareTag("Enemy"))
                continue;

            EnemyHearing hearing = enemy.GetComponentInParent<EnemyHearing>();

            if (hearing != null)
            {
                hearing.HearNoise(position);
            }
        }
    }
}
