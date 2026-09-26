using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    private IHearing hearingScript;
    private slimeIA slimeia;

    void Awake()
    {
        hearingScript = GetComponent<IHearing>();

        Debug.Log("ENEMY HEARING: AWAKE");

        slimeia = GetComponent<slimeIA>();

        if (slimeia == null)
            Debug.LogError("NO SE ENCUENTRA SLIMEIA");
        else
            Debug.Log("SLIMEIA ENCONTRADO");
    }

    public void HearNoise(Vector2 noisePosition)
    {
        Debug.Log("¡He escuchado un ruido!");
        Debug.Log("Ruido en: " + noisePosition);

        if (slimeia != null)
        {
            slimeia.estadoActual = slimeIA.Estado.Persiguiendo;
            slimeia.tiempoSinVerJugador = 0f;

            Debug.Log("Slime → PERSIGUIENDO");
        }

        hearingScript.SetNoisePosition(noisePosition);
    }
}