using UnityEngine;

public class ControladorBala : MonoBehaviour
{
    public float tiempoVida = 3f;

    void Start()
    {
        // Respaldo por si la cámara no detecta la salida
        Destroy(this.gameObject, tiempoVida);
    }

    void OnBecameInvisible()
    {
        Destroy(this.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        Destroy(this.gameObject);
    }
}