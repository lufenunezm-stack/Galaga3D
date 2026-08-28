using UnityEngine;
using UnityEngine.InputSystem; // Necesario para InputActionReference

public class AccionNave : MonoBehaviour
{
    public float velocidad = 10f;
    public float limiteIzquierdo = -8.5f;
    public float limiteDerecho = 8.5f;

    [Header("Input System")]
    public InputActionReference accionMover;

    private float direccionX = 0f;

    void Start()
    {
        // Inicialización si es necesario
    }
    private void OnEnable()
    {
        if (accionMover != null)
        {
            accionMover.action.Enable();
            // Suscripción a los eventos
            accionMover.action.performed += OnMovimientoPerformed;
            accionMover.action.canceled += OnMovimientoCanceled;
        }
    }

    private void OnDisable()
    {
        if (accionMover != null)
        {
            // Desuscripción de los eventos
            accionMover.action.performed -= OnMovimientoPerformed;
            accionMover.action.canceled -= OnMovimientoCanceled;
            accionMover.action.Disable();
        }
    }

    // Método cuando MANTIENES presionado A/D o Flechas (Soluciona error: OnMovimientoPerformed)
    private void OnMovimientoPerformed(InputAction.CallbackContext context)
    {
        direccionX = context.ReadValue<float>();
    }

    // Método cuando SUELTAS la tecla (Soluciona error: OnMovimientoCanceled)
    private void OnMovimientoCanceled(InputAction.CallbackContext context)
    {
        direccionX = 0f; // Se detiene al soltar
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