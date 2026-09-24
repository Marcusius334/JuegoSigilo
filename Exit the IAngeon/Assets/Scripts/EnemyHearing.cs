using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    public void HearNoise(Vector2 noisePosition)
    {
        Debug.Log("¡He escuchado un ruido!");

        // De momento, simplemente nos movemos hacia el ruido
        Debug.Log("Ruido en: " + noisePosition);
    }
}
