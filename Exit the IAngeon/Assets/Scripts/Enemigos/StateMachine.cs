using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public Enemy owner;
    public Enemy.Estado EstadoActual { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void Awake()
    {
        if (owner == null)
        {
            owner = GetComponent<Enemy>();
        }
    }
    public void Inicializar(Enemy.Estado estadoInicial)
    {
        EstadoActual = estadoInicial;
        EstadoEntrante(EstadoActual);
    }
    public void CambiarEstado(Enemy.Estado nuevoEstado)
    {
        if (EstadoActual == nuevoEstado)
            return;
        EstadoSaliente(EstadoActual);
        EstadoActual = nuevoEstado;
        EstadoEntrante(EstadoActual);
    }
    private void EstadoEntrante(Enemy.Estado nuevoEstado)
    {
        switch (nuevoEstado)
        {
            case Enemy.Estado.Patrullando:
                Debug.Log(owner.name + "entra en PATRULLANDO");
                break;
            case Enemy.Estado.Buscando:
                Debug.Log(owner.name + " entra en BUSCANDO");
                break;

            case Enemy.Estado.SeguirSonido:
                Debug.Log(owner.name + " entra en SEGUIR SONIDO");
                break;
            case Enemy.Estado.Persiguiendo:
                Debug.Log(owner.name + " entra en PERSIGUIENDO");
                break;

            case Enemy.Estado.Acorralando:
                Debug.Log(owner.name + " entra en ACORRALANDO");
                break;
        }
    }
    private void EstadoSaliente(Enemy.Estado estadoAnterior)
    {
        switch (estadoAnterior)
        {
            case Enemy.Estado.Patrullando:
                break;

            case Enemy.Estado.Buscando:
                break;

            case Enemy.Estado.SeguirSonido:
                break;

            case Enemy.Estado.Persiguiendo:
                break;

            case Enemy.Estado.Acorralando:
                break;
        }
    }
}
