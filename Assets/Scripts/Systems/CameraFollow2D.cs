using UnityEngine;

/// <summary>
/// Câmera 2D que segue o jogador suavemente, com limites opcionais de sala
/// (útil antes de trocar para Cinemachine, se o pacote for adicionado depois).
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0.5f, -10f);
    public float smoothTime = 0.15f;

    [Header("Limites da sala")]
    public bool useBounds = false;

    [Tooltip("Cantos do mapa em unidades de mundo, não da câmera.")]
    public Vector2 limiteMundoMin;
    public Vector2 limiteMundoMax;

    Vector3 velocity;
    Camera cam;

    void Awake() => cam = GetComponent<Camera>();

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        if (useBounds) desired = Travar(desired);

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    /// <summary>
    /// Impede a câmera de mostrar fora do mapa.
    ///
    /// A margem é calculada a partir do que a câmera realmente enxerga, que
    /// depende da proporção da tela: num monitor 21:9 a largura visível é bem
    /// maior que num 16:9. Com margem fixa, o ultrawide mostraria o vazio
    /// além da borda do cenário.
    /// </summary>
    Vector3 Travar(Vector3 desejada)
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return desejada;

        float meiaAltura = cam.orthographicSize;
        float meiaLargura = meiaAltura * cam.aspect;

        float minX = limiteMundoMin.x + meiaLargura;
        float maxX = limiteMundoMax.x - meiaLargura;
        float minY = limiteMundoMin.y + meiaAltura;
        float maxY = limiteMundoMax.y - meiaAltura;

        // mapa mais estreito que a tela: centraliza em vez de travar torto
        desejada.x = (minX > maxX) ? (limiteMundoMin.x + limiteMundoMax.x) * 0.5f
                                   : Mathf.Clamp(desejada.x, minX, maxX);
        desejada.y = (minY > maxY) ? (limiteMundoMin.y + limiteMundoMax.y) * 0.5f
                                   : Mathf.Clamp(desejada.y, minY, maxY);
        return desejada;
    }
}
