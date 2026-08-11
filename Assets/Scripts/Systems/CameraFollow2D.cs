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
    public bool useBounds = false;
    public Vector2 minBounds, maxBounds;

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
            desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
        }
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
