using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    // Cada oleada tiene su propio estado (vivos + puntaje) para que una nave
    // que muere tarde (de una oleada anterior que quedó viva) siga reportando
    // a la oleada a la que pertenece, y no a la que esté activa en ese momento.
    public class EstadoOleada
    {
        public int enemigosVivos;
        public int puntaje;
    }

    private bool oleadaEnCurso = false;


    // Guarda la posición FINAL de cada nave.
    private Dictionary<EnemigoGalaga, Vector3> posicionesFormacion =
        new Dictionary<EnemigoGalaga, Vector3>();


    // =========================================================
    // AWAKE
    // =========================================================

    void Awake()
    {
        // Primero guardamos las posiciones EXACTAS
        // que tienen las naves en la escena.
        foreach (Oleada oleada in oleadas)
        {
            if (oleada.contenedorNaves == null)
                continue;


            EnemigoGalaga[] naves =
                oleada.contenedorNaves
                .GetComponentsInChildren<EnemigoGalaga>(true);


            foreach (EnemigoGalaga nave in naves)
            {
                if (nave == null)
                    continue;


                // IMPORTANTE:
                // transform.position = posición mundial.
                // No usamos localPosition.
                posicionesFormacion[nave] =
                    nave.transform.position;


                // Las apagamos hasta que corresponda su entrada.
                nave.gameObject.SetActive(false);
            }
        }
    }


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        if (iniciarAutomaticamente)
        {
            IniciarOleadas();
        }
    }


    // =========================================================
    // INICIAR
    // =========================================================

    public void IniciarOleadas()
    {
        if (!oleadaEnCurso)
        {
            StartCoroutine(EjecutarOleadas());
        }
    }


    // =========================================================
    // EJECUTAR OLEADAS
    // =========================================================

    private IEnumerator EjecutarOleadas()
    {
        oleadaEnCurso = true;


        foreach (Oleada oleada in oleadas)
        {
            if (GameManager.Instancia != null && !GameManager.Instancia.JuegoActivo)
            {
                yield break;
            }


            if (oleada.contenedorNaves == null)
            {
                Debug.LogWarning(
                    "EnemySpawner: Una oleada no tiene contenedor."
                );

                continue;
            }


            // -----------------------------------------------
            // ESPERA ANTES
            // -----------------------------------------------

            if (oleada.esperaAntes > 0f)
            {
                yield return new WaitForSeconds(
                    oleada.esperaAntes
                );
            }


            // -----------------------------------------------
            // OBTENER NAVES
            // -----------------------------------------------

            EnemigoGalaga[] naves =
                oleada.contenedorNaves
                .GetComponentsInChildren<EnemigoGalaga>(true);


            EstadoOleada estadoOleada = new EstadoOleada
            {
                enemigosVivos = naves.Length,
                puntaje = oleada.puntajeOleada
            };


            // -----------------------------------------------
            // ACTIVAR UNA POR UNA
            // -----------------------------------------------

            foreach (EnemigoGalaga nave in naves)
            {
                if (nave == null)
                    continue;


                // Buscar la posición que tenía en el editor.
                if (!posicionesFormacion.TryGetValue(
                    nave,
                    out Vector3 posicionFormacion))
                {
                    Debug.LogWarning(
                        "No se encontró la posición de formación de " +
                        nave.name
                    );

                    continue;
                }


                // Configurar la nave ANTES de activarla.
                nave.ConfigurarOleada(
                    oleada.tipoEntrada,
                    oleada.direccion,
                    posicionFormacion,
                    this,
                    estadoOleada
                );


                // Ahora sí aparece.
                nave.gameObject.SetActive(true);


                // Esperar antes de la siguiente.
                if (oleada.delayEntreNaves > 0f)
                {
                    yield return new WaitForSeconds(
                        oleada.delayEntreNaves
                    );
                }
            }


            // -----------------------------------------------
            // ESPERA DESPUÉS
            // -----------------------------------------------

            if (oleada.esperaDespues > 0f)
            {
                yield return new WaitForSeconds(
                    oleada.esperaDespues
                );
            }
        }


        oleadaEnCurso = false;
    }


    // =========================================================
    // NOTIFICACIÓN DE MUERTE
    // =========================================================

    public void NotificarEnemigoDestruido(EstadoOleada estadoOleada)
    {
        estadoOleada.enemigosVivos--;

        if (estadoOleada.enemigosVivos <= 0 && GameManager.Instancia != null)
        {
            GameManager.Instancia.SumarPuntaje(estadoOleada.puntaje);
        }
    }
}