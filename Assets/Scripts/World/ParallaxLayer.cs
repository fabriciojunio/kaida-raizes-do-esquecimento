using UnityEngine;

/// <summary>
/// Camada de fundo que acompanha a câmera mais devagar, dando profundidade.
/// Fator 0 = colado na câmera (céu), 1 = anda junto com o mundo.
/// </summary>
[ExecuteAlways]
public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1f)] public float fator = 0.5f;
    public bool travarVertical = false;

    Transform cam;
    Vector3 posInicial;
    Vector3 camInicial;

    void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : null;
        posInicial = transform.position;
        if (cam != null) camInicial = cam.position;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main != null ? Camera.main.transform : null;
            if (cam == null) return;
            camInicial = cam.position;
        }

        Vector3 delta = cam.position - camInicial;
        float y = travarVertical ? 0f : delta.y * fator;
        transform.position = new Vector3(posInicial.x + delta.x * fator, posInicial.y + y, posInicial.z);
    }
}
