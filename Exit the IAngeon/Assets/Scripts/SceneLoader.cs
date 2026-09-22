using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    //He hecho una clase específica solo para cambiar de escena para poder tener el mismo cambio de escena en cualquier momento.
    //También puede sernos útil si queremos que pase algo al cambiar de escena.
    //También está para poder usar la función Click de los botones.
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    //Hay una opción de cargar una escena pesada de forma asíncrona para que no se quede parado mientras carga.
    //Lo dejo aquí debajo por si cargar el nivel fuera demasiado pesado.
    /*
    IEnumerator LoadLevel(string name)
    {
        AsyncOperation carga = SceneManager.LoadSceneAsync(name);

        while (!carga.isDone)
        {
            //Supongo que aquí es donde pondríamos la animación de carga.
            yield return null;
        }
    }
    */

}
