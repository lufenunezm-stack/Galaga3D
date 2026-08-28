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

    [Header("Input System References")]
    public InputActionReference accionMover;
    public InputActionReference accionDisparar; // Nueva referencia para el botón de disparo

    private float direccionX = 0f;

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
        if (balaPrefab == null || puntoCreacionBala == null) return;

        // 1. Instancia la bala en la posición del punto de creación
        GameObject nuevaBala = Instantiate(balaPrefab, puntoCreacionBala.position, puntoCreacionBala.rotation);

        // 2. Le aplica la fuerza física hacia adelante (Eje Z positivo)
        Rigidbody rbBala = nuevaBala.GetComponent<Rigidbody>();
        if (rbBala != null)
        {
            rbBala.AddForce(puntoCreacionBala.forward * fuerzaDisparo, ForceMode.Impulse);
        }
    }

    private void Update()
    {
        if (direccionX != 0)
        {
            float nuevaX = this.transform.position.x + (direccionX * velocidad * Time.deltaTime);
            nuevaX = Mathf.Clamp(nuevaX, limiteIzquierdo, limiteDerecho);

            this.transform.position = new Vector3(nuevaX, this.transform.position.y, this.transform.position.z);
        }
    }
}