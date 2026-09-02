using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // Necesario para InputActionReference

public class AccionNave : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 10f;
    public float limiteIzquierdo = -8.5f;
    public float limiteDerecho = 8.5f;

    [Header("Disparo")]
    public GameObject balaPrefab;
    public Transform puntoCreacionBala; // Objeto vacío en la punta de la nave
    public float fuerzaDisparo = 20f;

    [Header("Audio")]
    public AudioSource audioDisparo; // El componente AudioSource de la nave
    public AudioSource audioMuerte; // El componente AudioSource de la nave

    [Header("Input System References")]
    public InputActionReference accionMover;
    public InputActionReference accionDisparar; // Nueva referencia para el botón de disparo

    [Header("Vidas")]
    public float retrasoRespawn = 2f;
    public float duracionInvulnerabilidad = 2f;
    public float intervaloParpadeo = 0.15f;

    private float direccionX = 0f;
    private bool estaViva = true;
    private bool esInvulnerable = false;
    private Vector3 posicionInicial;
    private Collider[] colliders;
    private MeshRenderer meshRenderer;

    private void OnEnable()
    {
        // Suscripción de Movimiento
        if (accionMover != null)
        {
            accionMover.action.Enable();
            accionMover.action.performed += OnMovimientoPerformed;
            accionMover.action.canceled += OnMovimientoCanceled;
        }

        // Suscripción de Disparo (estilo de tu profe con +=)
        if (accionDisparar != null)
        {
            accionDisparar.action.Enable();
            accionDisparar.action.started += OnDispararStarted;
        }
    }

    private void OnDisable()
    {
        // Desuscripción de Movimiento
        if (accionMover != null)
        {
            accionMover.action.performed -= OnMovimientoPerformed;
            accionMover.action.canceled -= OnMovimientoCanceled;
            accionMover.action.Disable();
        }

        // Desuscripción de Disparo
        if (accionDisparar != null)
        {
            accionDisparar.action.started -= OnDispararStarted;
            accionDisparar.action.Disable();
        }
    }

    private void OnMovimientoPerformed(InputAction.CallbackContext context)
    {
        direccionX = context.ReadValue<float>();
    }

    private void OnMovimientoCanceled(InputAction.CallbackContext context)
    {
        direccionX = 0f;
    }

    // Método que se ejecuta exacto en el frame que presionas la tecla de disparo
    private void OnDispararStarted(InputAction.CallbackContext context)
    {
        DispararBala();
    }

    private void DispararBala()
    {
        if (!estaViva) return;
        if (balaPrefab == null || puntoCreacionBala == null) return;

        // 1. Instancia la bala en la posición del punto de creación
        GameObject nuevaBala = Instantiate(balaPrefab, puntoCreacionBala.position, puntoCreacionBala.rotation);

        // 2. Le aplica la fuerza física hacia adelante (Eje Z positivo)
        Rigidbody rbBala = nuevaBala.GetComponent<Rigidbody>();
        if (rbBala != null)
        {
            rbBala.AddForce(puntoCreacionBala.forward * fuerzaDisparo, ForceMode.Impulse);
        }
        audioDisparo.pitch = Random.Range(0.65f, 2f);
        audioDisparo.Play();
    }

    private void Start()
    {
        posicionInicial = transform.position;
        colliders = GetComponents<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (!estaViva) return;

        if (direccionX != 0)
        {
            float nuevaX = this.transform.position.x + (direccionX * velocidad * Time.deltaTime);
            nuevaX = Mathf.Clamp(nuevaX, limiteIzquierdo, limiteDerecho);

            this.transform.position = new Vector3(nuevaX, this.transform.position.y, this.transform.position.z);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!estaViva || esInvulnerable) return;

        // Mueres si te da una bala enemiga o si un enemigo en picada te choca
        if (other.gameObject.CompareTag("BalaEnemiga") || other.gameObject.CompareTag("Enemigo"))
        {
            Morir();
        }
    }

    private void Morir()
    {
        estaViva = false;

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.InstanciarExplosion(transform.position);
        }

        SetVisible(false);

        bool sigueVivo = GameManager.Instancia == null || GameManager.Instancia.PerderVida();

        if (sigueVivo)
        {
            Invoke(nameof(Reaparecer), retrasoRespawn);
        }
        if (audioMuerte != null && audioMuerte.clip != null)
        {
            AudioSource.PlayClipAtPoint(audioMuerte.clip, transform.position, 10.0f);
        }
    }

    private void Reaparecer()
    {
        transform.position = posicionInicial;
        SetVisible(true);
        estaViva = true;

        StartCoroutine(Invulnerabilidad());
    }

    private IEnumerator Invulnerabilidad()
    {
        esInvulnerable = true;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracionInvulnerabilidad)
        {
            if (meshRenderer != null) meshRenderer.enabled = !meshRenderer.enabled;
            yield return new WaitForSeconds(intervaloParpadeo);
            tiempoTranscurrido += intervaloParpadeo;
        }

        if (meshRenderer != null) meshRenderer.enabled = true;
        esInvulnerable = false;
    }

    private void SetVisible(bool visible)
    {
        if (meshRenderer != null) meshRenderer.enabled = visible;

        foreach (Collider col in colliders)
        {
            col.enabled = visible;
        }
    }
}