using UnityEngine;

public class BalaEnemiga : MonoBehaviour
{
    public float tiempoVida = 3f;

    void Start()
    {
        Destroy(this.gameObject, tiempoVida);
    }

    void OnBecameInvisible()
    {
        Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignoramos si atraviesa a otro enemigo o a otras balas enemigas (esto evita el fuego amigo de la fila de atrás)
        if (other.gameObject.CompareTag("Enemigo") || other.gameObject.CompareTag("BalaEnemiga")) return;

        // Se destruye al tocar al jugador
        Destroy(this.gameObject);
    }
}