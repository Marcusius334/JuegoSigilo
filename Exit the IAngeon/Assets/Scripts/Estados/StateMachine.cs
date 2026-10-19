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

    public void Inicializar(Type estadoInicial, params Type[] estadosPosibles)
    {
        if (owner == null) return; //La stateMachine no tiene enemigo asignado 
        foreach (Type tipoEstado in estadosPosibles) 
        {
            RegistrarEstado(tipoEstado);
        }
        // También registra el estado inicial si no estaba en la lista.
        RegistrarEstado(estadoInicial);

        CambiarEstado(estadoInicial);
    }
    private void RegistrarEstado(Type tipoEstado) 
    {
        if (tipoEstado == null || !typeof(State).IsAssignableFrom(tipoEstado) || tipoEstado.IsAbstract) //El tipo estado es invalido
        {
            return;
        }
        if (!instanciasEstados.ContainsKey(tipoEstado)) 
        {
            State instancia = (State)Activator.CreateInstance(tipoEstado);
            instanciasEstados.Add(tipoEstado, instancia);
        }
    }
    /*public void Inicializar<T>() where T : State, new()
    {
        Inicializar(typeof(T));
    }*/

    public void CambiarEstado(Type tipoEstado)
    {
        if (tipoEstado == null || owner == null)
            return;

        if (!instanciasEstados.TryGetValue(tipoEstado, out State nuevoEstado)) //Estado no registrado
        {
            return;
        }

        if (estadoActual != null && estadoActual.GetType() == tipoEstado)
            return;


        

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
