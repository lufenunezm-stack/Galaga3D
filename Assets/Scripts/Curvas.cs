using UnityEngine;

// Funciones de matemática pura para mover cosas en curva.
// No dependen de ningún script en particular: las puede usar
// cualquiera que necesite una curva Bézier o un ease-in-out.
public static class Curvas
{
    // Curva de Bézier cúbica entre 4 puntos.
    // p0 = inicio, p3 = destino, p1 y p2 son "imanes" que jalan
    // la curva sin que el objeto llegue a tocarlos.
    // t va de 0 (inicio) a 1 (destino).
    public static Vector3 Bezier3(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        return u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3;
    }

    // Suaviza el arranque y la llegada de un movimiento (acelera y después frena).
    public static float EaseInOut(float t)
    {
        return t * t * (3f - 2f * t);
    }
}
