using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void Reintentar()
    {
        SceneManager.LoadScene("Nivel_1"); // Vuelve a arrancar el juego
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene("MenuPrincipal"); // Vuelve al menú inicial
    }
}