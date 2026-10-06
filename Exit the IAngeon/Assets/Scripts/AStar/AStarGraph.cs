using System.Collections.Generic;
using UnityEngine;

public class AStarGraph : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float distanciaMaxima = 0.1f;
    [SerializeField] private LayerMask capaMuros;

    [Header("Visualización")]
    [SerializeField] private bool mostrarConexiones = true;

    private List<AStarNode> nodos = new List<AStarNode>();

    private void Awake()
    {
        ObtenerNodos();
        GenerarConexiones();
    }

    private void ObtenerNodos()
    {
        nodos.Clear();

        AStarNode[] nodosEncontrados = GetComponentsInChildren<AStarNode>();

        foreach (AStarNode nodo in nodosEncontrados)
        {
            nodos.Add(nodo);
        }
    }

    public void GenerarConexiones()
    {
        foreach (AStarNode nodo in nodos)
        {
            nodo.vecinos.Clear();
        }

        for (int i = 0; i < nodos.Count; i++)
        {
            for (int j = i + 1; j < nodos.Count; j++)
            {
                AStarNode nodoA = nodos[i];
                AStarNode nodoB = nodos[j];

                float distancia = Vector2.Distance(
                    nodoA.Posicion,
                    nodoB.Posicion
                );

                if (distancia > distanciaMaxima)
                    continue;

                if (!HayMuroEntre(nodoA.Posicion, nodoB.Posicion))
                {
                    nodoA.vecinos.Add(nodoB);
                    nodoB.vecinos.Add(nodoA);
                }
            }
        }
    }

    private bool HayMuroEntre(Vector3 posicionA, Vector3 posicionB)
    {
        Vector2 direccion = posicionB - posicionA;
        float distancia = direccion.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(
            posicionA,
            direccion.normalized,
            distancia,
            capaMuros
        );

        return hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        if (!mostrarConexiones)
            return;

        AStarNode[] nodosActuales = GetComponentsInChildren<AStarNode>();

        Gizmos.color = Color.yellow;

        foreach (AStarNode nodo in nodosActuales)
        {
            foreach (AStarNode vecino in nodo.vecinos)
            {
                if (vecino != null)
                {
                    Gizmos.DrawLine(
                        nodo.Posicion,
                        vecino.Posicion
                    );
                }
            }
        }
    }
}