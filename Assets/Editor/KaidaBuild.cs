using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Ponto de entrada para montar o jogo inteiro de uma vez, pelo menu ou pela
/// linha de comando:
///
///   Unity.exe -batchmode -quit -projectPath . -executeMethod KaidaBuild.MontarTudo
///
/// A ordem importa: sprites → animações → prefabs → tiles → cenas. Cada etapa
/// consome o que a anterior gerou.
/// </summary>
public static class KaidaBuild
{
    [MenuItem("Kaida/MONTAR TUDO", false, 0)]
    public static void MontarTudo()
    {
        try
        {
            Log("configurando projeto");
            ConfigurarProjeto();

            Log("1/5 fatiando sprites");
            SpriteSheetSetup.ConfigurarTudo();

            Log("2/5 gerando animações");
            AnimationBuilder.GerarTudo();

            Log("3/5 recortando itens e gerando prefabs");
            RecorteDeSprites.RecortarTudo();   // os prefabs dependem destes sprites
            PrefabBuilder.GerarTudo();

            Log("4/5 gerando tiles");
            TileSetup.GerarTiles();

            Log("5/5 montando cenas");
            SceneBuilder.GerarTudo();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Log("PRONTO - jogo montado.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Kaida] Falhou ao montar: {e}");
            if (Application.isBatchMode) EditorApplication.Exit(1);
            throw;
        }
    }

    /// <summary>
    /// Ajustes de projeto que as cenas assumem prontos: matriz de colisão e
    /// qualidade de render para pixel art.
    /// </summary>
    public static void ConfigurarProjeto()
    {
        // O jogador não deve ser empurrado pelo corpo dos inimigos - o dano de
        // contato é resolvido por overlap, não por colisão física.
        Physics2D.IgnoreLayerCollision(PrefabBuilder.LayerPlayer, PrefabBuilder.LayerEnemy, true);
        Physics2D.IgnoreLayerCollision(PrefabBuilder.LayerEnemy, PrefabBuilder.LayerEnemy, true);

        // O colisor do chefe é um gatilho - ele voa e não pode esbarrar no
        // cenário. Sem isto, o golpe da Kaida não o encontraria.
        Physics2D.queriesHitTriggers = true;

        // pixel art não pode ser filtrada nem anti-aliased
        QualitySettings.antiAliasing = 0;

        PlayerSettings.companyName = "Projeto de Faculdade";
        PlayerSettings.productName = "Kaida - Raizes do Esquecimento";
        PlayerSettings.runInBackground = true;

        // O jogo pode ser aberto em qualquer tela: monitor ultrawide, notebook
        // ou projetor de sala de aula. "Fullscreen Window" assume a resolução
        // nativa de quem estiver rodando, sem trocar o modo do monitor - é o
        // que não quebra em projetor. A janela também pode ser redimensionada.
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultIsNativeResolution = true;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.allowFullscreenSwitch = true;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        // Nenhuma proporção precisa ser liberada à mão: versões atuais da
        // Unity aceitam todas. Quem se adapta é a câmera (calcula a margem
        // pelo aspect real) e o CanvasScaler (casa pela altura).
    }

    /// <summary>Gera o executável Windows em Build/.</summary>
    [MenuItem("Kaida/Gerar executável", false, 20)]
    public static void GerarExecutavel()
    {
        var cenas = EditorBuildSettings.scenes;
        if (cenas.Length == 0)
        {
            Debug.LogError("[Kaida] Nenhuma cena em Build Settings - rode 'MONTAR TUDO' antes.");
            if (Application.isBatchMode) EditorApplication.Exit(1);
            return;
        }

        var caminhos = new string[cenas.Length];
        for (int i = 0; i < cenas.Length; i++) caminhos[i] = cenas[i].path;

        var opcoes = new BuildPlayerOptions
        {
            scenes = caminhos,
            locationPathName = "Build/Kaida.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var relatorio = BuildPipeline.BuildPlayer(opcoes);
        var resumo = relatorio.summary;

        if (resumo.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Log($"executável gerado: Build/Kaida.exe ({resumo.totalSize / (1024 * 1024)} MB)");
        }
        else
        {
            Debug.LogError($"[Kaida] Build falhou: {resumo.result}, {resumo.totalErrors} erros.");
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }

    /// <summary>Monta tudo e já gera o executável. Usado pela linha de comando.</summary>
    public static void MontarEBuildar()
    {
        MontarTudo();
        GerarExecutavel();
    }

    static void Log(string msg) => Debug.Log($"[Kaida] {msg}");
}
