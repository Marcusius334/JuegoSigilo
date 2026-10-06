using System.Collections.Generic;
using UnityEngine;

public class AStarTest : MonoBehaviour
{
    [SerializeField] private Transform inicio;
    [SerializeField] private Transform objetivo;

    [SerializeField] private AStarPathfinding pathfinding;

    private List<AStarNode> camino;

    private void Start()
    {
        camino = pathfinding.BuscarCamino(
            inicio.position,
            objetivo.position
        );
    }

    private void OnDrawGizmos()
    {
        if (camino == null || camino.Count < 2)
            return;

        Gizmos.color = Color.red;

        for (int i = 0; i < camino.Count - 1; i++)
        {
            Gizmos.DrawLine(
                camino[i].Posicion,
                camino[i + 1].Posicion
            );
        }
    }
}