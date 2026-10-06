using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    private AStarGraph graph;

    private void Awake()
    {
        graph = FindFirstObjectByType<AStarGraph>();
    }

    public List<AStarNode> BuscarCamino(Vector3 inicio, Vector3 objetivo)
    {
        if (graph == null)
        {
            Debug.LogError("No se ha encontrado ningún AStarGraph en la escena.");
            return null;
        }

        AStarNode nodoInicio = ObtenerNodoMasCercano(inicio);
        AStarNode nodoObjetivo = ObtenerNodoMasCercano(objetivo);

        Debug.Log(
    "Nodo inicio: " + nodoInicio.name +
    " | Nodo objetivo: " + nodoObjetivo.name
);

        if (nodoInicio == null || nodoObjetivo == null)
        {
            Debug.LogWarning("No se ha podido encontrar un nodo de inicio u objetivo.");
            return null;
        }

        List<AStarNode> abiertos = new List<AStarNode>();
        HashSet<AStarNode> cerrados = new HashSet<AStarNode>();

        Dictionary<AStarNode, float> costeG = new Dictionary<AStarNode, float>();
        Dictionary<AStarNode, float> costeH = new Dictionary<AStarNode, float>();
        Dictionary<AStarNode, AStarNode> padres = new Dictionary<AStarNode, AStarNode>();

        abiertos.Add(nodoInicio);
        costeG[nodoInicio] = 0f;
        costeH[nodoInicio] = Distancia(nodoInicio, nodoObjetivo);

        while (abiertos.Count > 0)
        {
            AStarNode actual = ObtenerMejorNodo(abiertos, costeG, costeH);

            if (actual == nodoObjetivo)
            {
                return ReconstruirCamino(padres, actual);
            }

            abiertos.Remove(actual);
            cerrados.Add(actual);

            foreach (AStarNode vecino in actual.vecinos)
            {
                if (vecino == null || cerrados.Contains(vecino))
                    continue;

                float nuevoCosteG =
                    costeG[actual] + Distancia(actual, vecino);

                if (!costeG.ContainsKey(vecino))
                {
                    costeG[vecino] = Mathf.Infinity;
                }

                if (!abiertos.Contains(vecino))
                {
                    abiertos.Add(vecino);
                }

                if (nuevoCosteG < costeG[vecino])
                {
                    costeG[vecino] = nuevoCosteG;
                    costeH[vecino] = Distancia(vecino, nodoObjetivo);
                    padres[vecino] = actual;
                }
            }
        }
        

        Debug.LogWarning("No se ha encontrado ningún camino.");
        return null;
    }

    private AStarNode ObtenerNodoMasCercano(Vector3 posicion)
    {
        AStarNode[] nodos = graph.GetComponentsInChildren<AStarNode>();

        AStarNode nodoMasCercano = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (AStarNode nodo in nodos)
        {
            float distancia = Vector3.Distance(
                posicion,
                nodo.Posicion
            );

            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                nodoMasCercano = nodo;
            }
        }

        return nodoMasCercano;
    }

    private AStarNode ObtenerMejorNodo(
        List<AStarNode> abiertos,
        Dictionary<AStarNode, float> costeG,
        Dictionary<AStarNode, float> costeH)
    {
        AStarNode mejorNodo = abiertos[0];

        float mejorCosteF = ObtenerCosteF(
            mejorNodo,
            costeG,
            costeH
        );

        for (int i = 1; i < abiertos.Count; i++)
        {
            AStarNode nodo = abiertos[i];

            float costeF = ObtenerCosteF(
                nodo,
                costeG,
                costeH
            );

            if (costeF < mejorCosteF)
            {
                mejorNodo = nodo;
                mejorCosteF = costeF;
            }
        }

        return mejorNodo;
    }

    private float ObtenerCosteF(
        AStarNode nodo,
        Dictionary<AStarNode, float> costeG,
        Dictionary<AStarNode, float> costeH)
    {
        float g = costeG.ContainsKey(nodo)
            ? costeG[nodo]
            : Mathf.Infinity;

        float h = costeH.ContainsKey(nodo)
            ? costeH[nodo]
            : Distancia(nodo, nodo);

        return g + h;
    }

    private float Distancia(AStarNode nodoA, AStarNode nodoB)
    {
        return Vector3.Distance(
            nodoA.Posicion,
            nodoB.Posicion
        );
    }

    private List<AStarNode> ReconstruirCamino(
        Dictionary<AStarNode, AStarNode> padres,
        AStarNode nodoActual)
    {
        List<AStarNode> camino = new List<AStarNode>();

        camino.Add(nodoActual);

        while (padres.ContainsKey(nodoActual))
        {
            nodoActual = padres[nodoActual];
            camino.Add(nodoActual);
        }

        camino.Reverse();

        return camino;
    }
}