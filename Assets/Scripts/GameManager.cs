using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    [Header("Vidas")]
    public int vidasIniciales = 3;
    public GameObject[] iconosVidas;

    [Header("Puntaje")]
    public TextMeshProUGUI textoPuntaje;

    [Header("Explosión")]
    public GameObject explosionPrefab;
    public float duracionExplosion = 2f;

    public bool JuegoActivo { get; private set; } = true;

    private int vidasActuales;
    private int puntajeActual;

    void Awake()
    {
        Instancia = this;
        vidasActuales = vidasIniciales;
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
            // Al quedarse sin vidas, carga de inmediato la escena de Game Over
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
    }
}