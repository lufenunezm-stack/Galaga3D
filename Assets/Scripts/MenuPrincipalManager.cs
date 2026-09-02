using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalManager : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene("Nivel_1"); // Carga tu escena de juego
    }

}