using UnityEngine;

public static class NoiseManager
{
    public static void MakeNoise(Vector2 position, float radius)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(position, radius);

        foreach (Collider2D enemy in enemies)
        {
            EnemyHearing hearing = enemy.GetComponentInParent<EnemyHearing>();

            if (hearing != null)
            {
                Debug.Log("He escuchado");
                hearing.HearNoise(position);
            }
        }
    }
}