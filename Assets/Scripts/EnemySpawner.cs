using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("puntajeOleada")]
        [Tooltip("Puntos extra que se suman si esta oleada se destruye por completo sin perder vidas.")]
        public int bonoOleadaPerfecta = 100;
    }

    [Header("Oleadas")]
    public List<Oleada> oleadas = new List<Oleada>();

    [Header("Configuración")]
    public bool iniciarAutomaticamente = true;

    [Header("Escena de Victoria")]
    public string nombreEscenaVictoria = "Victoria";

    private int totalEnemigosRealNivel = 0;
    private int enemigosDestruidos = 0;
    private bool oleadaEnCurso = false;
    private bool todasLasOleadasSpawneadas = false;
    private bool victoriaActivada = false;

    public class EstadoOleada
    {
        public int enemigosVivos;
        public int bonoOleadaPerfecta;
        public int vidasAlIniciar;
    }

    private Dictionary<EnemigoGalaga, Vector3> posicionesFormacion = new Dictionary<EnemigoGalaga, Vector3>();

    void Awake()
    {
        posicionesFormacion.Clear();
        totalEnemigosRealNivel = 0;

        foreach (Oleada oleada in oleadas)
        {
            if (oleada.contenedorNaves == null) continue;

            // Buscamos componentes solo de nivel directo para evitar duplicados en cascada
            foreach (Transform hijo in oleada.contenedorNaves)
            {
                if (hijo == null) continue;

                EnemigoGalaga nave = hijo.GetComponent<EnemigoGalaga>();
                if (nave != null && !posicionesFormacion.ContainsKey(nave))
                {
                    posicionesFormacion[nave] = nave.transform.position;
                    totalEnemigosRealNivel++;
                    nave.gameObject.SetActive(false);
                }
            }
        }

        Debug.Log("Total de enemigos reales detectados en el nivel: " + totalEnemigosRealNivel);
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
        todasLasOleadasSpawneadas = false;

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

            List<EnemigoGalaga> navesValidasOleada = new List<EnemigoGalaga>();
            foreach (Transform hijo in oleada.contenedorNaves)
            {
                if (hijo == null) continue;
                EnemigoGalaga nave = hijo.GetComponent<EnemigoGalaga>();
                if (nave != null && posicionesFormacion.ContainsKey(nave))
                {
                    navesValidasOleada.Add(nave);
                }
            }

            EstadoOleada estadoOleada = new EstadoOleada
            {
                enemigosVivos = navesValidasOleada.Count,
                puntaje = oleada.puntajeOleada,
                enemigosVivos = naves.Length,
                bonoOleadaPerfecta = oleada.bonoOleadaPerfecta,
                vidasAlIniciar = GameManager.Instancia != null ? GameManager.Instancia.VidasActuales : 0
            };

            foreach (EnemigoGalaga nave in navesValidasOleada)
            {
                Vector3 posicionFormacion = posicionesFormacion[nave];

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
        todasLasOleadasSpawneadas = true;
    }

    public void NotificarEnemigoDestruido(EstadoOleada estadoOleada)
    {
        if (victoriaActivada) return;

        estadoOleada.enemigosVivos--;
        enemigosDestruidos++;

        if (estadoOleada.enemigosVivos <= 0 && GameManager.Instancia != null)
        {
            bool oleadaPerfecta = GameManager.Instancia.VidasActuales == estadoOleada.vidasAlIniciar;

            if (oleadaPerfecta && estadoOleada.bonoOleadaPerfecta > 0)
            {
                GameManager.Instancia.SumarPuntaje(estadoOleada.bonoOleadaPerfecta);
            }
        }

        Debug.Log("Enemigos destruidos: " + enemigosDestruidos + " de " + totalEnemigosRealNivel);

        if (todasLasOleadasSpawneadas && enemigosDestruidos >= totalEnemigosRealNivel && !victoriaActivada)
        {
            victoriaActivada = true;
            Debug.Log("¡Victoria! Todas las oleadas completadas y enemigos destruidos.");

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