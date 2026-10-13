using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    [SerializeField] private bool hearsWalking = false;

    private IHearing hearingScript;
    private slimeIA slimeia;

    void Awake()
    {
        Debug.Log("EnemyHearing iniciado en: " + gameObject.name);
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
        Debug.Log("EnemyHearing.HearNoise ejecutado en: " + gameObject.name);
        if (!isRunning && !hearsWalking)
            return;

        Debug.Log("¡He escuchado un ruido!");
        Debug.Log("Ruido en: " + noisePosition);

        if (slimeia != null)
        {
            slimeia.HearNoise(noisePosition);
        }
        else if (hearingScript != null)
        {
            hearingScript.SetNoisePosition(noisePosition);
            Debug.Log("EnemyHearing: enviando ruido a SetNoisePosition()");
        }
        Debug.Log("EnemyHearing: " + gameObject.name +
          " ha recibido el ruido. Corriendo: " + isRunning);

        Debug.Log("EnemyHearing: ¿Es slime? " + (slimeia != null));
    }
}