using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class VictoriaManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textoPuntajeFinal;
    public TMP_InputField inputNombreJugador; // Opcional para guardar el nombre

    void Start()
    {
        // 1. Recuperamos el puntaje guardado o pasados por PlayerPrefs
        int puntajeFinal = PlayerPrefs.GetInt("PuntajeFinal", 0);

        if (textoPuntajeFinal != null)
        {
            textoPuntajeFinal.text = "Puntaje : " + puntajeFinal.ToString("0000");
        }
    }

    // Botón para volver al menú principal
    public void VolverAlMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }
}
