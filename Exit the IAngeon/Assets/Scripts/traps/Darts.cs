using UnityEngine;
using System.Collections;

public class Darts : MonoBehaviour
{
    public float tiempoEspera = 3f;
    public float tiempoEspera2 = 3f;

    [Header("Dardo")]
    public GameObject prefabDardo;
    public DartsObject.Direccion direccionDardo;
    public Transform puntoSalida;

    private Animator animator;
    private bool puedeDisparar = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && puedeDisparar)
        {
            puedeDisparar = false;
            animator.SetBool("Shooting", true);

            StartCoroutine(EsperarParaDisparar());
        }
    }

    private IEnumerator EsperarParaDisparar()
    {
        yield return new WaitForSeconds(tiempoEspera);

        shoot();

        animator.SetBool("Shooting", false);

        yield return new WaitForSeconds(tiempoEspera2);

        puedeDisparar = true;
    }

    private void shoot()
    {
        Vector3 posicion = puntoSalida != null
            ? puntoSalida.position
            : transform.position;

        GameObject dardo = Instantiate(
            prefabDardo,
            posicion,
            Quaternion.identity
        );

        DartsObject scriptDardo = dardo.GetComponent<DartsObject>();

        if (scriptDardo != null)
        {
            scriptDardo.direccion = direccionDardo;
        }
    }
}