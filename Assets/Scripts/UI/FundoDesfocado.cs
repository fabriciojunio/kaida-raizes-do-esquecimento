using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mostra o cenário do menu desfocado atrás da interface.
///
/// A câmera da cena renderiza para uma textura em vez da tela; essa textura
/// é desenhada num RawImage com o shader de desfoque. Assim o fundo continua
/// vivo — a Kaida anda, as folhas se mexem — mas sem competir com o texto.
///
/// Se o shader não estiver disponível por qualquer motivo, cai para um véu
/// escuro simples: o menu continua legível.
/// </summary>
public class FundoDesfocado : MonoBehaviour
{
    [Tooltip("Câmera que enquadra o cenário do fundo.")]
    public Camera cameraDoCenario;

    [Range(0f, 12f)] public float raio = 5f;
    [Range(0f, 1f)] public float escurecer = 0.42f;

    RenderTexture textura;
    RawImage imagem;
    Material material;

    void Start()
    {
        if (cameraDoCenario == null) cameraDoCenario = Camera.main;
        if (cameraDoCenario == null) return;

        Montar();
    }

    void OnDestroy()
    {
        // a câmera precisa voltar a desenhar na tela para as outras cenas
        if (cameraDoCenario != null) cameraDoCenario.targetTexture = null;

        if (textura != null) { textura.Release(); Destroy(textura); }
        if (material != null) Destroy(material);
    }

    void Update()
    {
        // acompanha mudanças de resolução (alternar tela cheia, por exemplo)
        if (textura != null && (textura.width != Screen.width || textura.height != Screen.height))
            Montar();
    }

    void Montar()
    {
        if (textura != null) { cameraDoCenario.targetTexture = null; textura.Release(); Destroy(textura); }

        textura = new RenderTexture(Mathf.Max(320, Screen.width), Mathf.Max(180, Screen.height), 16);
        textura.filterMode = FilterMode.Bilinear;
        cameraDoCenario.targetTexture = textura;

        var canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = UIKit.ConfigurarCanvas(gameObject, 0, comCliques: false);
        canvas.sortingOrder = 0;   // atrás de todo o resto da interface

        if (imagem == null)
        {
            var go = new GameObject("Cenario", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            imagem = go.AddComponent<RawImage>();

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        imagem.texture = textura;

        var shader = Shader.Find("Kaida/DesfoqueDeFundo");
        if (shader != null)
        {
            if (material == null) material = new Material(shader);
            material.SetFloat("_Raio", raio);
            material.SetFloat("_Escurecer", escurecer);
            imagem.material = material;
            imagem.color = Color.white;
        }
        else
        {
            // sem o shader, escurece na unha para o texto continuar legível
            imagem.material = null;
            imagem.color = new Color(1f - escurecer, 1f - escurecer, 1f - escurecer, 1f);
            Debug.LogWarning("[Kaida] shader de desfoque não encontrado; usando véu escuro.");
        }
    }
}
