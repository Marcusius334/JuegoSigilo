
using System;

public class PersiguiendoState : State
{
    public PersiguiendoState()
    {
        transiciones["player_lost"] = typeof(BuscandoState);
        transiciones["noise_heard"] = typeof(SeguirSonidoState);
        transiciones["start_surround"] = typeof(AcorralandoState);

        transiciones["player_lost_patrol"] = typeof(PatrullandoState);
        transiciones["player_lost_search"] = typeof(BuscandoState);

        transiciones["leader_lost_patrol"] = typeof(PatrullandoState);
        transiciones["leader_lost_search"] = typeof(BuscandoState);
    }

    public override void Enter(Enemy enemigo)
    {
        enemigo.PlayAlert();
        enemigo.tiempoSinVerJugador = 0f;
    }

    public override void Execute(Enemy enemigo)
    {
        enemigo.EjecutarPersecucion();
    }
}

