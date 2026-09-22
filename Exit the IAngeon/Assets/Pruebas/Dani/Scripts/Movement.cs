using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Movement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 5f;
    public Rigidbody2D rb;
    Vector3 move;
    

    

    void Update()
    {
        move = new Vector3(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"),0);
    }

    void FixedUpdate()
    {
        rb.MovePosition(transform.position+(speed*move*Time.fixedDeltaTime));
    }
}
