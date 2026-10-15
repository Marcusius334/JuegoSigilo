using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [SerializeField] private Enemy owner;

    private State estadoActual;
    private State estadoAnterior;

    private readonly Dictionary<Type, State> instanciasEstados =
        new Dictionary<Type, State>();

    public State EstadoActual => estadoActual;
    public State EstadoAnterior => estadoAnterior;

    private void Awake()
    {
        if (owner == null)
            owner = GetComponent<Enemy>();
    }

    public void Inicializar(Type tipoEstado)
    {
        CambiarEstado(tipoEstado);
    }
    public void Inicializar<T>() where T : State, new()
    {
        Inicializar(typeof(T));
    }

    public void CambiarEstado(Type tipoEstado)
    {
        if (tipoEstado == null || owner == null)
            return;

        if (!typeof(State).IsAssignableFrom(tipoEstado) ||
            tipoEstado.IsAbstract)
        {
            Debug.LogError($"Tipo de estado inválido: {tipoEstado}");
            return;
        }

        if (estadoActual != null &&
            estadoActual.GetType() == tipoEstado)
            return;


        if (!instanciasEstados.TryGetValue(tipoEstado, out State nuevoEstado))
        {
            nuevoEstado = (State)Activator.CreateInstance(tipoEstado);
            instanciasEstados.Add(tipoEstado, nuevoEstado);
        }

        estadoActual?.Exit(owner);

        estadoAnterior = estadoActual;
        estadoActual = nuevoEstado;

        estadoActual.Enter(owner);
    }

    public void ActualizarEstado()
    {
        estadoActual?.Execute(owner);
    }

    public void LanzarEvento(string evento)
    {
        if (estadoActual == null)
            return;

        Type siguienteEstado =
            estadoActual.GetNextStateClass(evento);

        if (siguienteEstado != null)
            CambiarEstado(siguienteEstado);
    }
}
