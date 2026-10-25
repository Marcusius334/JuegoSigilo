using UnityEngine;

public class DartsObject : MonoBehaviour
{
    public float velocidad = 10f;
    public Direccion direccion;
    public SceneLoader sceneLoader;

    public enum Direccion
    {
        Derecha,
        Abajo,
        Izquierda
    }

    private Vector2 movimiento;

    private void Awake()
    {
        if (sceneLoader == null)
        {
            sceneLoader = FindFirstObjectByType<SceneLoader>();
        }
        switch (direccion)
        {
            case Direccion.Derecha:
                movimiento = Vector2.right;
                break;

            case Direccion.Abajo:
                movimiento = Vector2.down;
                break;

            case Direccion.Izquierda:
                movimiento = Vector2.left;
                break;
        }
    }

    private void Update()
    {
        transform.Translate(
            movimiento * velocidad * Time.deltaTime,
            Space.World
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (sceneLoader != null)
                sceneLoader.LoadScene("Death");
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Muros"))
        {
            Destroy(gameObject);
        }
    }
}