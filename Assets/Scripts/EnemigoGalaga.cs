using UnityEngine;

public class EnemigoGalaga : MonoBehaviour
{
    // Enums

    public enum EstadoEnemigo { Entrando, EnFormacion, Atacando }
    public enum TipoEntrada { Curva, Circular }
    public enum DireccionEntrada { Izquierda, Derecha }

    private enum FaseCircular { LlegandoAlCirculo, DandoVuelta, Acomodando }

    // Estado

    [Header("Estado")]
    public EstadoEnemigo estadoActual = EstadoEnemigo.Entrando;

    private TipoEntrada tipoEntrada;
    private DireccionEntrada direccionEntrada;
    private EnemySpawner spawnerDueño;
    private EnemySpawner.EstadoOleada estadoOleada;

    // Formación

    [Header("Formación")]
    public Vector3 posicionFormacion;
    public float movimientoFormacion = 0.8f;
    public float amplitudFormacion = 0.8f;

    private float tiempoFormacion;
    private float faseFormacion;

    // Entrada - configuración general

    [Header("Entrada")]
    public float distanciaAparicion = 18f;

    [Tooltip("Qué tan arriba aparece la nave respecto a su formación.")]
    public float alturaAparicion = 12f;

    // Entrada - curva

    [Header("Curva")]
    public float duracionEntrada = 2.5f;

    [Tooltip("Distancia horizontal del primer punto de control.")]
    public float curvaControl1X = 10f;

    [Tooltip("Altura del primer punto de control.")]
    public float curvaControl1Z = 8f;

    [Tooltip("Distancia horizontal del segundo punto de control.")]
    public float curvaControl2X = 8f;

    [Tooltip("Altura del segundo punto de control.")]
    public float curvaControl2Z = 6f;

    private Vector3 puntoInicio;
    private Vector3 puntoControl1;
    private Vector3 puntoControl2;
    private float tiempoEntrada;

    // Entrada - circular

    [Header("Circular")]
    [Tooltip("Distancia del centro del círculo respecto a la formación.")]
    public float distanciaCentroCirculo = 7f;

    public float alturaCentroCirculo = 5f;
    public float radioCirculo = 4f;

    [Tooltip("Tiempo que tarda en llegar al círculo.")]
    public float duracionLlegadaCirculo = 0.8f;

    [Tooltip("Tiempo que tarda en dar las vueltas.")]
    public float duracionCirculo = 1.5f;

    [Tooltip("Cantidad de vueltas.")]
    public float vueltasCirculo = 1f;

    [Tooltip("Tiempo para abandonar el círculo y entrar en formación.")]
    public float duracionAcomodo = 1f;

    private FaseCircular faseCircular;
    private Vector3 centroCirculo;
    private Vector3 puntoEntradaCirculo;
    private Vector3 puntoSalidaCirculo;
    private float tiempoFaseCircular;
    private float anguloInicialCirculo;

    // Ataque

    [Header("Ataque")]
    public float duracionAtaque = 2.5f;
    public Vector3 ataqueControl1 = new Vector3(6f, 0f, -4f);
    public Vector3 ataqueControl2 = new Vector3(-5f, 0f, -12f);

    [Tooltip("Cada cuánto, en promedio, la nave abandona la formación para atacar en picada.")]
    public float cadenciaAtaqueMinima = 3f;
    public float cadenciaAtaqueMaxima = 7f;

    private Vector3 ataqueInicio;
    private Vector3 ataqueControl1Real;
    private Vector3 ataqueControl2Real;
    private Vector3 ataqueDestino;
    private float tiempoAtaque;
    private float proximoAtaque;

    // Disparo

    [Header("Disparo")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;
    public float fuerzaDisparo = 15f;
    public float cadenciaMinima = 2f;
    public float cadenciaMaxima = 6f;

    private float proximoDisparo;

    // Puntaje

    [Header("Puntaje")]
    [Tooltip("Puntos que suma al jugador cuando esta nave es destruida.")]
    public int puntaje = 100;

    private bool destruido = false;

    // Start / Update

    void Start()
    {
        // La formación ya fue configurada por el Spawner.
        ProgramarSiguienteDisparo();
    }

    void Update()
    {
        if (GameManager.Instancia != null && !GameManager.Instancia.JuegoActivo) return;

        switch (estadoActual)
        {
            case EstadoEnemigo.Entrando:
                if (tipoEntrada == TipoEntrada.Curva) MovimientoEntradaCurva();
                else MovimientoEntradaCircular();
                break;

            case EstadoEnemigo.EnFormacion:
                MovimientoFormacion();
                break;

            case EstadoEnemigo.Atacando:
                MovimientoAtaque();
                break;
        }

        ManejarDisparo();
    }

    // Configurar desde el Spawner

    public void ConfigurarOleada(TipoEntrada nuevoTipo, DireccionEntrada nuevaDireccion, Vector3 nuevaPosicionFormacion, EnemySpawner spawner, EnemySpawner.EstadoOleada estado)
    {
        tipoEntrada = nuevoTipo;
        direccionEntrada = nuevaDireccion;
        spawnerDueño = spawner;
        estadoOleada = estado;

        // ESTA es la posición que tenía la nave originalmente en el editor.
        posicionFormacion = nuevaPosicionFormacion;

        faseFormacion = Random.Range(-0.5f, 0.5f);

        PrepararEntrada();
    }

    // Preparar entrada

    private void PrepararEntrada()
    {
        estadoActual = EstadoEnemigo.Entrando;
        tiempoEntrada = 0f;
        tiempoFaseCircular = 0f;

        float lado = direccionEntrada == DireccionEntrada.Derecha ? 1f : -1f;

        if (tipoEntrada == TipoEntrada.Curva)
        {
            puntoInicio = posicionFormacion + new Vector3(lado * distanciaAparicion, 0f, alturaAparicion);
            puntoControl1 = posicionFormacion + new Vector3(lado * curvaControl1X, 0f, curvaControl1Z);

            // OJO: cambiamos de lado en el segundo control para producir
            // una curva más pronunciada, en vez de una simple diagonal.
            puntoControl2 = posicionFormacion + new Vector3(-lado * curvaControl2X, 0f, curvaControl2Z);

            transform.position = puntoInicio;
        }
        else
        {
            centroCirculo = posicionFormacion + new Vector3(lado * distanciaCentroCirculo, 0f, alturaCentroCirculo);

            // La nave aparece lejos del círculo.
            puntoInicio = posicionFormacion + new Vector3(lado * distanciaAparicion, 0f, alturaAparicion);

            // Punto donde comienza a girar.
            puntoEntradaCirculo = centroCirculo + new Vector3(lado * radioCirculo, 0f, 0f);

            transform.position = puntoInicio;
            faseCircular = FaseCircular.LlegandoAlCirculo;
            anguloInicialCirculo = lado > 0f ? 0f : 180f;
        }
    }

    // Movimiento curva

    private void MovimientoEntradaCurva()
    {
        tiempoEntrada += Time.deltaTime;

        float t = Mathf.Clamp01(tiempoEntrada / duracionEntrada);
        t = Curvas.EaseInOut(t);

        transform.position = Curvas.Bezier3(puntoInicio, puntoControl1, puntoControl2, posicionFormacion, t);

        if (t >= 1f)
        {
            transform.position = posicionFormacion;
            estadoActual = EstadoEnemigo.EnFormacion;
            tiempoFormacion = 0f;
            ProgramarSiguienteAtaque();
        }
    }

    // Movimiento circular

    private void MovimientoEntradaCircular()
    {
        tiempoFaseCircular += Time.deltaTime;

        switch (faseCircular)
        {
            // Llegar al círculo
            case FaseCircular.LlegandoAlCirculo:
            {
                float t = Mathf.Clamp01(tiempoFaseCircular / duracionLlegadaCirculo);
                t = Curvas.EaseInOut(t);

                transform.position = Vector3.Lerp(puntoInicio, puntoEntradaCirculo, t);

                if (t >= 1f)
                {
                    tiempoFaseCircular = 0f;
                    faseCircular = FaseCircular.DandoVuelta;
                }
                break;
            }

            // Dar la vuelta
            case FaseCircular.DandoVuelta:
            {
                float t = Mathf.Clamp01(tiempoFaseCircular / duracionCirculo);
                float angulo = anguloInicialCirculo + (360f * vueltasCirculo * t);
                float rad = angulo * Mathf.Deg2Rad;

                Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radioCirculo;
                transform.position = centroCirculo + offset;

                if (t >= 1f)
                {
                    puntoSalidaCirculo = transform.position;
                    tiempoFaseCircular = 0f;
                    faseCircular = FaseCircular.Acomodando;
                }
                break;
            }

            // Salir del círculo
            case FaseCircular.Acomodando:
            {
                float t = Mathf.Clamp01(tiempoFaseCircular / duracionAcomodo);
                t = Curvas.EaseInOut(t);

                transform.position = Vector3.Lerp(puntoSalidaCirculo, posicionFormacion, t);

                if (t >= 1f)
                {
                    transform.position = posicionFormacion;
                    estadoActual = EstadoEnemigo.EnFormacion;
                    tiempoFormacion = 0f;
                    ProgramarSiguienteAtaque();
                }
                break;
            }
        }
    }

    // Formación

    private void MovimientoFormacion()
    {
        if (Time.time >= proximoAtaque)
        {
            ComenzarAtaque();
            return;
        }

        tiempoFormacion += Time.deltaTime;

        float desplazamiento = Mathf.Sin(tiempoFormacion * movimientoFormacion + faseFormacion) * amplitudFormacion;

        Vector3 destino = posicionFormacion;
        destino.x += desplazamiento;

        transform.position = Vector3.Lerp(transform.position, destino, Time.deltaTime * 5f);
    }

    // Ataque

    public void ComenzarAtaque()
    {
        estadoActual = EstadoEnemigo.Atacando;
        tiempoAtaque = 0f;

        ataqueInicio = transform.position;
        ataqueControl1Real = ataqueInicio + ataqueControl1;
        ataqueControl2Real = ataqueInicio + ataqueControl2;
        ataqueDestino = ataqueInicio + new Vector3(0f, 0f, -30f);
    }

    private void MovimientoAtaque()
    {
        tiempoAtaque += Time.deltaTime;

        float t = Mathf.Clamp01(tiempoAtaque / duracionAtaque);
        t = Curvas.EaseInOut(t);

        transform.position = Curvas.Bezier3(ataqueInicio, ataqueControl1Real, ataqueControl2Real, ataqueDestino, t);

        if (t >= 1f)
        {
            PrepararEntrada();
        }
    }

    // Disparo

    private void ManejarDisparo()
    {
        if (Time.time >= proximoDisparo)
        {
            Disparar();
            ProgramarSiguienteDisparo();
        }
    }

    private void Disparar()
    {
        if (balaPrefab == null || puntoDisparo == null) return;

        GameObject nuevaBala = Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);

        Rigidbody rbBala = nuevaBala.GetComponent<Rigidbody>();
        if (rbBala != null)
        {
            rbBala.AddForce(puntoDisparo.forward * fuerzaDisparo, ForceMode.Impulse);
        }
    }

    private void ProgramarSiguienteDisparo()
    {
        proximoDisparo = Time.time + Random.Range(cadenciaMinima, cadenciaMaxima);
    }

    private void ProgramarSiguienteAtaque()
    {
        proximoAtaque = Time.time + Random.Range(cadenciaAtaqueMinima, cadenciaAtaqueMaxima);
    }

    // Colisión

    private void OnTriggerEnter(Collider other)
    {
        if (destruido) return;

        if (other.CompareTag("BalaJugador"))
        {
            destruido = true;

            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.InstanciarExplosion(transform.position);
                GameManager.Instancia.SumarPuntaje(puntaje);
            }

            if (spawnerDueño != null)
            {
                spawnerDueño.NotificarEnemigoDestruido(estadoOleada);
            }

            Destroy(gameObject);
        }
    }
}
