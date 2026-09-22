using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float runSpeed = 10f;

    public bool hasKey = false;

    private Vector2 movement;
    private SpriteRenderer spriteRenderer;

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
}
