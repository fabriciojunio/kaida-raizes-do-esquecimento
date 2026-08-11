using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Renderiza cada cena para um PNG, sem precisar abrir o jogo.
/// Serve para conferir enquadramento, escala e cor sem depender de alguém
/// jogar e descrever o que viu.
/// </summary>
public static class CapturaDeTela
{
    const string Destino = "Capturas";

    [MenuItem("Kaida/Capturar telas", false, 40)]
    public static void CapturarTudo()
    {
        Directory.CreateDirectory(Destino);

        foreach (var cena in EditorBuildSettings.scenes)
        {
            if (!cena.enabled) continue;
            Capturar(cena.path);
        }

        Debug.Log($"[Kaida] Capturas salvas em {Destino}/");
    }

    static void Capturar(string caminhoCena)
    {
        var cena = EditorSceneManager.OpenScene(caminhoCena, OpenSceneMode.Single);
        string nome = Path.GetFileNameWithoutExtension(caminhoCena);

        var cam = Object.FindObjectOfType<Camera>();
        if (cam == null)
        {
            Debug.LogWarning($"[Kaida] {nome} não tem câmera.");
            return;
        }

        // enquadra o jogador, como ficaria no primeiro frame de jogo
        var jogador = Object.FindObjectOfType<PlayerController>();
        if (jogador != null)
        {
            var seguidor = cam.GetComponent<CameraFollow2D>();
            var offset = seguidor != null ? seguidor.offset : new Vector3(0f, 1.5f, -10f);
            var alvo = jogador.transform.position + offset;

            // respeita os limites da sala, senão a captura mostra o vazio
            // fora do mapa — que o jogador nunca chega a ver
            if (seguidor != null && seguidor.useBounds)
            {
                float meiaAltura = cam.orthographicSize;
                float meiaLargura = meiaAltura * (16f / 9f);
                alvo.x = Mathf.Clamp(alvo.x,
                    seguidor.limiteMundoMin.x + meiaLargura,
                    seguidor.limiteMundoMax.x - meiaLargura);
                alvo.y = Mathf.Clamp(alvo.y,
                    seguidor.limiteMundoMin.y + meiaAltura,
                    seguidor.limiteMundoMax.y - meiaAltura);
            }
            cam.transform.position = alvo;
        }

        // proporção ultrawide de propósito: é a tela mais exigente, onde
        // sobra mais mapa visível e os buracos de cenário aparecem primeiro
        const int largura = 1280, altura = 540;
        var rt = new RenderTexture(largura, altura, 24);
        cam.targetTexture = rt;

        var foto = new Texture2D(largura, altura, TextureFormat.RGB24, false);
        cam.Render();

        RenderTexture.active = rt;
        foto.ReadPixels(new Rect(0, 0, largura, altura), 0, 0);
        foto.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;

        File.WriteAllBytes($"{Destino}/{nome}.png", foto.EncodeToPNG());
        Object.DestroyImmediate(foto);
        rt.Release();

        Debug.Log($"[Kaida] capturei {nome}");
    }

    /// <summary>Diagnóstico em texto: escala, enquadramento e contagem de peças.</summary>
    [MenuItem("Kaida/Diagnóstico das cenas", false, 41)]
    public static void Diagnosticar()
    {
        foreach (var cenaBuild in EditorBuildSettings.scenes)
        {
            if (!cenaBuild.enabled) continue;
            var cena = EditorSceneManager.OpenScene(cenaBuild.path, OpenSceneMode.Single);
            string nome = Path.GetFileNameWithoutExtension(cenaBuild.path);

            var cam = Object.FindObjectOfType<Camera>();
            var jogador = Object.FindObjectOfType<PlayerController>();
            var tilemap = Object.FindObjectOfType<UnityEngine.Tilemaps.Tilemap>();

            Debug.Log($"--- {nome}");
            if (cam != null)
                Debug.Log($"    câmera: ortho={cam.orthographic} size={cam.orthographicSize} " +
                          $"mostra {cam.orthographicSize * 2f * 16f / 9f:F1} x {cam.orthographicSize * 2f:F1} unidades");

            if (jogador != null)
            {
                var sr = jogador.GetComponent<SpriteRenderer>();
                var col = jogador.GetComponent<Collider2D>();
                Debug.Log($"    Kaida em {jogador.transform.position}, " +
                          $"sprite {(sr != null && sr.sprite != null ? sr.sprite.bounds.size.ToString() : "SEM SPRITE")}, " +
                          $"collider {(col != null ? col.bounds.size.ToString() : "SEM COLLIDER")}");
            }

            if (tilemap != null)
            {
                Debug.Log($"    tilemap: {tilemap.GetUsedTilesCount()} tiles, " +
                          $"limites {tilemap.cellBounds}, cor {tilemap.color}");
                var tile = tilemap.GetTile<UnityEngine.Tilemaps.Tile>(
                    new Vector3Int(tilemap.cellBounds.xMin, tilemap.cellBounds.yMin, 0));
            }

            int inimigos = Object.FindObjectsOfType<EnemyController>().Length;
            Debug.Log($"    inimigos: {inimigos}");
        }
    }
}
