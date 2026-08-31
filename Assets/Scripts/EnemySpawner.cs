using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Oleada
    {
        [Header("Identificación")]
        public string nombreOleada = "Oleada";

        [Header("Naves")]
        [Tooltip("Objeto padre que contiene las naves de esta oleada.")]
        public Transform contenedorNaves;

        [Header("Entrada")]
        public EnemigoGalaga.TipoEntrada tipoEntrada =
            EnemigoGalaga.TipoEntrada.Curva;

        public EnemigoGalaga.DireccionEntrada direccion =
            EnemigoGalaga.DireccionEntrada.Derecha;

        [Header("Tiempo")]
        public float delayEntreNaves = 0.3f;
        public float esperaAntes = 0f;
        public float esperaDespues = 1f;

        [Header("Puntaje")]
        [Tooltip("Puntos que se suman al destruir la última nave de esta oleada.")]
        public int puntajeOleada = 100;
    }

    [Header("Oleadas")]
    public List<Oleada> oleadas = new List<Oleada>();

    [Header("Configuración")]
    public bool iniciarAutomaticamente = true;

    [Header("Escena de Victoria")]
    public string nombreEscenaVictoria = "Victoria";

    private int totalEnemigosNivel = 0;
    private int enemigosDestruidos = 0;

    public class EstadoOleada
    {
        public int enemigosVivos;
        public int puntaje;
    }

    private bool oleadaEnCurso = false;
    private Dictionary<EnemigoGalaga, Vector3> posicionesFormacion = new Dictionary<EnemigoGalaga, Vector3>();

    void Awake()
    {
        foreach (Oleada oleada in oleadas)
        {
            if (oleada.contenedorNaves == null) continue;

            EnemigoGalaga[] naves = oleada.contenedorNaves.GetComponentsInChildren<EnemigoGalaga>(true);

            totalEnemigosNivel += naves.Length;

            foreach (EnemigoGalaga nave in naves)
            {
                if (nave == null) continue;

                posicionesFormacion[nave] = nave.transform.position;
                nave.gameObject.SetActive(false);
            }
        }
    }

    void Start()
    {
        if (iniciarAutomaticamente)
        {
            IniciarOleadas();
        }
    }

    public void IniciarOleadas()
    {
        if (!oleadaEnCurso)
        {
            StartCoroutine(EjecutarOleadas());
        }
    }

    private IEnumerator EjecutarOleadas()
    {
        oleadaEnCurso = true;

        foreach (Oleada oleada in oleadas)
        {
            if (GameManager.Instancia != null && !GameManager.Instancia.JuegoActivo)
            {
                yield break;
            }

            if (oleada.contenedorNaves == null) continue;

            if (oleada.esperaAntes > 0f)
            {
                yield return new WaitForSeconds(oleada.esperaAntes);
            }

            EnemigoGalaga[] naves = oleada.contenedorNaves.GetComponentsInChildren<EnemigoGalaga>(true);

            EstadoOleada estadoOleada = new EstadoOleada
            {
                enemigosVivos = naves.Length,
                puntaje = oleada.puntajeOleada
            };

            foreach (EnemigoGalaga nave in naves)
            {
                if (nave == null) continue;

                if (!posicionesFormacion.TryGetValue(nave, out Vector3 posicionFormacion)) continue;

                nave.ConfigurarOleada(
                    oleada.tipoEntrada,
                    oleada.direccion,
                    posicionFormacion,
                    this,
                    estadoOleada
                );

                nave.gameObject.SetActive(true);

                if (oleada.delayEntreNaves > 0f)
                {
                    yield return new WaitForSeconds(oleada.delayEntreNaves);
                }
            }

            if (oleada.esperaDespues > 0f)
            {
                yield return new WaitForSeconds(oleada.esperaDespues);
            }
        }

        oleadaEnCurso = false;
    }

    public void NotificarEnemigoDestruido(EstadoOleada estadoOleada)
    {
        estadoOleada.enemigosVivos--;
        enemigosDestruidos++;

        if (estadoOleada.enemigosVivos <= 0 && GameManager.Instancia != null)
        {
            GameManager.Instancia.SumarPuntaje(estadoOleada.puntaje);
        }

        Debug.Log("Enemigos destruidos: " + enemigosDestruidos + " de " + totalEnemigosNivel);

        if (enemigosDestruidos >= totalEnemigosNivel)
        {
            Debug.Log("¡Victoria! Todas las naves han sido destruidas.");

            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.GuardarPuntajeAntesDeSalir();
            }

            StartCoroutine(EsperarYCargarVictoria());
        }
    }

    private IEnumerator EsperarYCargarVictoria()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(nombreEscenaVictoria);
    }
}