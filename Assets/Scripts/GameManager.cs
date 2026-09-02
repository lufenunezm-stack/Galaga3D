using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    [Header("Vidas")]
    public int vidasIniciales = 3;
    public GameObject[] iconosVidas;

    [Header("Puntaje y Récord")]
    public TextMeshProUGUI textoPuntaje;
    public TextMeshProUGUI textoHighScore;

    [Header("Explosión")]
    public GameObject explosionPrefab;
    public float duracionExplosion = 2f;

    public bool JuegoActivo { get; private set; } = true;
    public int VidasActuales => vidasActuales;

    private int vidasActuales;
    private int puntajeActual;
    private int highScoreActual;

    void Awake()
    {
        Instancia = this;
        vidasActuales = vidasIniciales;

        highScoreActual = PlayerPrefs.GetInt("HighScore", 0);
        ActualizarTextoHighScore();
    }

    public void InstanciarExplosion(Vector3 posicion)
    {
        if (explosionPrefab == null) return;
        GameObject fx = Instantiate(explosionPrefab, posicion, Quaternion.identity);
        Destroy(fx, duracionExplosion);
    }

    public bool PerderVida()
    {
        vidasActuales--;

        int indiceIcono = (vidasIniciales - 1) - vidasActuales;
        if (iconosVidas != null && indiceIcono >= 0 && indiceIcono < iconosVidas.Length && iconosVidas[indiceIcono] != null)
        {
            iconosVidas[indiceIcono].SetActive(false);
        }

        if (vidasActuales <= 0)
        {
            JuegoActivo = false;
            SceneManager.LoadScene("GameOver");
            return false;
        }

        return true;
    }

    public void SumarPuntaje(int cantidad)
    {
        puntajeActual += cantidad;

        if (textoPuntaje != null)
        {
            textoPuntaje.text = puntajeActual.ToString("0000");
        }

        if (puntajeActual > highScoreActual)
        {
            highScoreActual = puntajeActual;

            PlayerPrefs.SetInt("HighScore", highScoreActual);
            PlayerPrefs.Save();

            ActualizarTextoHighScore();
        }
    }

    public void GuardarPuntajeAntesDeSalir()
    {
        PlayerPrefs.SetInt("PuntajeFinal", puntajeActual);
        PlayerPrefs.Save();
    }

    void ActualizarTextoHighScore()
    {
        if (textoHighScore != null)
        {
            textoHighScore.text = highScoreActual.ToString("0000");
        }
    }
}