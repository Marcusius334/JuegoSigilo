using UnityEngine;

public class NormalDoorInteraction : MonoBehaviour
{
    public NormalDoor door;
    
    // Contador para manejar múltiples enemigos cruzando a la vez (Flocking)
    private int enemigosEnZona = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            door.PlayerNear();
        }
        else if (other.CompareTag("Enemy"))
        {
            enemigosEnZona++;
            
            // Si entra el primer enemigo, abrimos la puerta
            if (enemigosEnZona > 0)
            {
                door.AbrirParaEnemigo();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            door.PlayerFar();
        }
        else if (other.CompareTag("Enemy"))
        {
            enemigosEnZona--;
            
            // Solo reactivamos la puerta si ya no queda ni un enemigo cruzando
            if (enemigosEnZona <= 0)
            {
                enemigosEnZona = 0; // Por seguridad
                door.CerrarParaEnemigo();
            }
        }
    }
}
