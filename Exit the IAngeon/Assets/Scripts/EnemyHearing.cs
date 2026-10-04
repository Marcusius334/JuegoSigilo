using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    [SerializeField] private bool hearsWalking = false;

    private IHearing hearingScript;
    private slimeIA slimeia;

    void Awake()
    {
        hearingScript = GetComponent<IHearing>();

        Debug.Log("ENEMY HEARING: AWAKE");

        slimeia = GetComponent<slimeIA>();

        /*if (slimeia == null)
            Debug.LogError("NO SE ENCUENTRA SLIMEIA");
        else
            Debug.Log("SLIMEIA ENCONTRADO");*/
    }

    public void HearNoise(Transform noisePosition, bool isRunning)
    {
        // Si está andando y este enemigo no puede escuchar pasos normales, ignoramos el ruido
        if (!isRunning && !hearsWalking)
            return;

        Debug.Log("¡He escuchado un ruido!");
        Debug.Log("Ruido en: " + noisePosition);

        if (slimeia != null)
        {
            slimeia.estadoActual = slimeIA.Estado.Persiguiendo;
            slimeia.tiempoSinVerJugador = 0f;

            Debug.Log("Slime → PERSIGUIENDO");
        }

        if (hearingScript != null)
        {
            hearingScript.SetNoisePosition(noisePosition);
        }
    }
}