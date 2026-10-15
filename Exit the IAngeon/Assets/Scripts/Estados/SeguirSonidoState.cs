
using System;

public class SeguirSonidoState : State
{
    public SeguirSonidoState()
    {
        transiciones["player_detected"] = typeof(PersiguiendoState);
        transiciones["sound_reached"] = typeof(BuscandoState);
        transiciones["noise_heard"] = typeof(SeguirSonidoState);

        transiciones["sound_reached_patrol"] = typeof(PatrullandoState);
    }

    public override void Enter(Enemy enemigo)
    {
        
    }

    public override void Execute(Enemy enemigo)
    {
        enemigo.EjecutarSeguirSonido();
    }
}

