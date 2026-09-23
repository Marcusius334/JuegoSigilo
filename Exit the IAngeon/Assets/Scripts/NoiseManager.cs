using UnityEngine;

public static class NoiseManager
{
    public static void MakeNoise(Vector2 position, float radius)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(position, radius);

        foreach (Collider2D enemy in enemies)
        {
            object hearing;

            if(enemy.name == "Goblin_1" || enemy.name == "Goblin_2" || enemy.name == "Goblin_3")
            {
                hearing = enemy.GetComponent<Goblin>();
            }
            else
            {
                hearing = enemy.GetComponent<EnemyHearing>();
            }

            if (hearing != null)
            {
                //hearing.HearNoise(position);
            }
        }
    }
}
