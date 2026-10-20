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

    //CAMBIOS PARA SLIME:
    private AStarNode proxNodo;
    private AStarNode ultNodo;
    [Header("Para el Wander")]
    public float margenLlegada = 0.5f;

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

    //CAMBIOS PARA EL SLIME:
    public void ElegirPrimerNodo(Vector3 pos)
    {
        AStarNode nodoMasCercano = null;
        float menorDistancia = Mathf.Infinity;

        //Debug.Log("Cantidad de nodos: " + nodos.Count);
        foreach (AStarNode nodo in nodos)
        {
            // Diferencia entre la posición del nodo y el objetivo
            Vector3 diferencia = nodo.transform.position - pos;
            
            // Usamos sqrMagnitude para evitar el costo de la raíz cuadrada
            float distanciaCuadrada = diferencia.sqrMagnitude;

            if (distanciaCuadrada < menorDistancia)
            {
                menorDistancia = distanciaCuadrada;
                nodoMasCercano = nodo;
            }
        }

        proxNodo = nodoMasCercano;
        ultNodo = proxNodo;
        Debug.Log("Primer Nodo al que ir: " + proxNodo.transform.position);
    }

    private void ElegirProximoNodo()
    {
        int numPosiblesNodos = 0;
        List<AStarNode> posiblesNodos = new List<AStarNode>();

        foreach(AStarNode nodo in proxNodo.vecinos)
        {
            if (nodo != ultNodo) {
                numPosiblesNodos++;
                posiblesNodos.Add(nodo);
            }
        }

        if (numPosiblesNodos == 0)
        {
            proxNodo = ultNodo;
            return;
        }

        int siguientoNodo = Random.Range(0, numPosiblesNodos - 1);
        
        ultNodo = proxNodo;
        proxNodo = posiblesNodos[siguientoNodo];

        Debug.Log("Siguiente Nodo al que ir: " + proxNodo.transform.position);
    }

    public Vector2 DireccionDeMovimiento(Vector3 posOrigen)
    {
        Vector3 dir3 = proxNodo.transform.position - posOrigen;
        Vector2 dir2 = new Vector2(dir3.x, dir3.y).normalized;
        return dir2;
    }

    public bool HaLlegadoAlNodo(Vector3 pos)
    {
        Vector3 diferencia = proxNodo.transform.position - pos;

        if (diferencia.sqrMagnitude <= margenLlegada*margenLlegada)
        {
            ElegirProximoNodo();
            return true;
        }
        return false;
    }

    /* public Vector3 PosicionProximoNodo()
    {
        return proxNodo.transform.position;
    } */
}