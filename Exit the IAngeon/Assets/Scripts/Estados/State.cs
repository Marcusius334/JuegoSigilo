using System;
using System.Collections.Generic;

public abstract class State
{
    protected readonly Dictionary<string, Type> transiciones =
        new Dictionary<string, Type>();

    public virtual void Enter(Enemy enemigo) { }

    public abstract void Execute(Enemy enemigo);

    public virtual void Exit(Enemy enemigo) { }

    public virtual Type GetNextStateClass(string evento)
    {
        if (transiciones.TryGetValue(evento, out Type siguiente))
            return siguiente;

        return null;
    }
}

