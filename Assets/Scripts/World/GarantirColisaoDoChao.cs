using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Reconstrói a geometria de colisão do chão ao abrir a cena.
///
/// A forma do CompositeCollider2D não é totalmente confiável quando vem
/// serializada: dependendo da ordem em que a cena foi montada, ela pode
/// chegar vazia. E chão sem colisão é o pior defeito possível aqui - o
/// cenário aparece perfeito e o jogador atravessa o mundo no primeiro
/// segundo, sem nenhuma pista do que houve.
///
/// Custa uma chamada por carregamento de cena. Vale o seguro.
/// </summary>
[RequireComponent(typeof(CompositeCollider2D))]
public class GarantirColisaoDoChao : MonoBehaviour
{
    void Awake()
    {
        var tilemap = GetComponent<Tilemap>();
        var colisorDoMapa = GetComponent<TilemapCollider2D>();
        var composto = GetComponent<CompositeCollider2D>();

        if (tilemap != null) tilemap.RefreshAllTiles();

        if (colisorDoMapa != null && !colisorDoMapa.usedByComposite)
            colisorDoMapa.usedByComposite = true;

        if (composto != null) composto.GenerateGeometry();
    }

    void Start()
    {
        var composto = GetComponent<CompositeCollider2D>();
        if (composto != null && composto.pathCount == 0)
            Debug.LogError($"[Kaida] O chão de '{gameObject.scene.name}' está sem colisão: " +
                           "o jogador vai atravessar o cenário.");
    }
}
