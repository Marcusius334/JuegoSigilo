using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform jugador;
    
    void Start()
    {
        
    }
    void LateUpdate()
    {
        transform.position = new Vector3(jugador.position.x, jugador.position.y, -10f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
