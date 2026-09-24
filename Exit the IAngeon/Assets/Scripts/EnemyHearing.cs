using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    private IHearing hearingScript;

    void Awake()
    {
        hearingScript = GetComponent<IHearing>();
        Debug.Log("cosa de codigo");
    }

    public void HearNoise(Vector2 noisePosition)
    {
        Debug.Log("¡He escuchado un ruido!");

        // De momento, simplemente nos movemos hacia el ruido
        Debug.Log("Ruido en: " + noisePosition);

        hearingScript.SetNoisePosition(noisePosition);
    }
}
