using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float runSpeed = 10f;

    public bool hasKey = false;

    public AudioSource audioSourceMovement;
    public AudioSource audioSourceDamage;
    public AudioSource audioSourceKey;
    public AudioSource audioSourceDoor;

    private Vector2 movement;
    private SpriteRenderer spriteRenderer;
    public SceneLoader sceneLoader;

    public bool IsMoving => movement.magnitude > 0.1f;
    public bool IsRunning => Keyboard.current.leftShiftKey.isPressed && IsMoving;

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
        if (IsRunning && !audioSourceMovement.isPlaying)
        {
            audioSourceMovement.pitch = 2f;
            audioSourceMovement.Play();
        }else if(IsMoving && !audioSourceMovement.isPlaying)
        {
            audioSourceMovement.pitch = 1f;
            audioSourceMovement.Play();
        }else if(!IsMoving && audioSourceMovement.isPlaying)
        {
            audioSourceMovement.Stop();
        }

        float currentSpeed = IsRunning ? runSpeed : speed;

        transform.Translate(movement * currentSpeed * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Detectar las colisiones con los enemigos y cosas
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //audioSourceDamage.Play(); // <-- En caso de meter vidas
            sceneLoader.LoadScene("Death");
        }
        else if (collision.gameObject.CompareTag("Exit"))
        {
            sceneLoader.LoadScene("Victory");
        }
    }

    public bool Running()//Para que se pueda saber si el jugador corre fuera de player
    {
        return IsRunning;
    }

    //Las siguientes dos funciones son para el sonido de llaves y puertas para simplificarlo, se llaman desde llaves y puertas
    public void GetKey()
    {
        audioSourceKey.Play();
    }

    public void OpenDoor()
    {
        audioSourceDoor.Play();
    }
}