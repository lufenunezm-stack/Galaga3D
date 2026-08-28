using UnityEngine;

public class ScrollFondo : MonoBehaviour
{
    public float velocidadScroll = 0.2f;
    private Renderer renderizador;

    void Start()
    {
        renderizador = GetComponent<Renderer>();
    }

    void Update()
    {
        // Calcula el desplazamiento continuo en el eje Y de la textura
        float offset = Time.time * velocidadScroll;
        renderizador.material.mainTextureOffset = new Vector2(0, offset);
    }
}