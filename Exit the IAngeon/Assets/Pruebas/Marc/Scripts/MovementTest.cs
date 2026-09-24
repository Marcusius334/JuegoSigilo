using System;
using UnityEngine;

public class MovementTest : MonoBehaviour
{
    //Esta variable habrá que asignarla en la interfaz.
    public SceneLoader sceneLoader;

    //Este código es símplemente para testear como si fuera el personaje.
    //Lo úni relevante son las colisiones al pasar de escena.
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        Vector3 movimiento = Vector3.zero;

        if (Input.GetKey(KeyCode.A))
        {
            movimiento.x -= 10;
        }

        if (Input.GetKey(KeyCode.D))
        {
            movimiento.x += 10;
        }

        rb.linearVelocity = new Vector2(
            movimiento.x,
            rb.linearVelocity.y
        );
    }

    //VVV Esto es lo importante VVV
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            sceneLoader.LoadScene("Death");
        }else if (collision.gameObject.CompareTag("Exit"))
        {
            sceneLoader.LoadScene("Victory");
        }
    }
}
