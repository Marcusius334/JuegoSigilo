using System;

public class BuscandoState : State
{
    public BuscandoState()
    {
        transiciones["player_detected"] = typeof(PersiguiendoState);
        transiciones["noise_heard"] = typeof(SeguirSonidoState);
    }

    public override void Enter(Enemy enemigo)
    {
        enemigo.tiempoSinVerJugador = 0f;
    }

    public override void Execute(Enemy enemigo)
    {
        enemigo.EjecutarBuscando();
    }
}
