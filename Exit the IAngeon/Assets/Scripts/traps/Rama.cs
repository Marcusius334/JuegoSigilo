using UnityEngine;

public class Rama : MonoBehaviour
{
    private Animator animator;

    private void Awake(){
        animator = GetComponent<Animator>();
    }


    private void OnTriggerEnter2D(Collider2D other){

        if (other.CompareTag("Player")){
           animator.SetBool("Pisar", true);
        }
    }
}
