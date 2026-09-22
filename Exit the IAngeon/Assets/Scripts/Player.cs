using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float runSpeed = 10f;

    public bool hasKey = false;

    private Vector2 movement;
    private SpriteRenderer spriteRenderer;
    public SceneLoader sceneLoader;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();

        if (movement.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (movement.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    void Update()
    {
        bool isRunning = Keyboard.current.leftShiftKey.isPressed;

        float currentSpeed = isRunning ? runSpeed : speed;

        transform.Translate(movement * currentSpeed * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Detectar las colisiones con los enemigos y cosas
        if (collision.gameObject.CompareTag("Enemy"))
        {
            sceneLoader.LoadScene("Death");
        }else if (collision.gameObject.CompareTag("Exit"))
        {
            sceneLoader.LoadScene("Victory");
        }
    }
}
