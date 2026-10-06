using System.Collections.Generic;
using UnityEngine;

public class AStarNode : MonoBehaviour
{
    [HideInInspector]
    public List<AStarNode> vecinos = new List<AStarNode>();

    public Vector3 Posicion => transform.position;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}
