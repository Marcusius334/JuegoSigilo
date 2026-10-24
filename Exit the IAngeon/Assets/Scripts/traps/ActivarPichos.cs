using UnityEngine;

public class ActivarPichos : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Spikes trampa;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            trampa.ActivarPinchos();
        }
    }
}
