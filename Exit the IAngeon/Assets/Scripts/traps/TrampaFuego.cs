using UnityEngine;
using System.Collections;

public class TrampaFuego : MonoBehaviour
{
    public float tiempoEntreAtaques = 3f;
    public Collider2D zonaDeFuego;
    public SceneLoader sceneLoader;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (zonaDeFuego != null)
        {
            zonaDeFuego.enabled = false;
        }

        if (sceneLoader == null)
        {
            sceneLoader = FindFirstObjectByType<SceneLoader>();
        }
    }

    private void Start()
    {
        StartCoroutine(RutinaTrampa());
    }

    private IEnumerator RutinaTrampa()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoEntreAtaques);
            animator.SetTrigger("Atacar");
        }
    }

    public void ActivarFuego()
    {
        if (zonaDeFuego != null) zonaDeFuego.enabled = true;
    }

    public void DesactivarFuego()
    {
        if (zonaDeFuego != null) zonaDeFuego.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (sceneLoader != null)
                sceneLoader.LoadScene("Death");
        }
    }
}