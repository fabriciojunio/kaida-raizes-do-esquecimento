using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Verifica que dá para chegar em tudo.
///
/// Percorre as superfícies de cada região a partir do ponto de partida,
/// respeitando o quanto a Kaida sobe e cruza de fato, e cobra que nenhuma
/// plataforma, item ou passagem fique ilhado.
///
/// Este teste existe porque as primeiras versões dos mapas tinham degraus de
/// 4 e 5 unidades, acima do que o pulo alcança: metade do cenário era
/// decoração inalcançável e a progressão simplesmente travava.
/// </summary>
public class AlcanceDoMapaTests
{
    static readonly string[] Cenas =
    {
        "Assets/Scenes/01_OrlaDaVila.unity",
        "Assets/Scenes/02_FlorestaSilente.unity",
        "Assets/Scenes/03_LagoSilente.unity",
        "Assets/Scenes/04_CavernaMusgosa.unity",
        "Assets/Scenes/05_SantuarioEsquecido.unity",
    };

    // Limites conservadores em relação aos stats reais: o pulo sobe 4 e cruza
    // cerca de 5. Exigir folga evita mapas que só funcionam no pixel exato.
    const int SubidaMaxima = 3;
    const int VaoMaximo = 5;
    const int QuedaMaxima = 14;

    [TestCase("Assets/Scenes/01_OrlaDaVila.unity")]
    [TestCase("Assets/Scenes/02_FlorestaSilente.unity")]
    [TestCase("Assets/Scenes/03_LagoSilente.unity")]
    [TestCase("Assets/Scenes/04_CavernaMusgosa.unity")]
    [TestCase("Assets/Scenes/05_SantuarioEsquecido.unity")]
    public void TudoQueImporta_EstaAoAlcanceDoJogador(string caminho)
    {
        var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
        var raizes = cena.GetRootGameObjects();

        var superficies = LevantarSuperficies(raizes);
        Assert.Greater(superficies.Count, 0, "a região não tem onde pisar");

        var jogador = raizes.Select(r => r.GetComponentInChildren<PlayerController>())
                            .FirstOrDefault(p => p != null);
        Assert.IsNotNull(jogador, "sem jogador na cena");

        var alcancaveis = Espalhar(superficies, jogador.transform.position, Pocos(raizes));

        // tudo com que o jogador precisa interagir
        var alvos = new List<(string, Vector3)>();
        foreach (var r in raizes)
        {
            foreach (var c in r.GetComponentsInChildren<Checkpoint>())
                alvos.Add(("marco de descanso", c.transform.position));
            foreach (var p in r.GetComponentsInChildren<PickupAbility>())
                alvos.Add(($"habilidade {p.abilityId}", p.transform.position));
            foreach (var n in r.GetComponentsInChildren<HealthNode>())
                alvos.Add(("nódulo de vida", n.transform.position));
            foreach (var f in r.GetComponentsInChildren<LoreFragment>())
                alvos.Add(("fragmento", f.transform.position));
            foreach (var t in r.GetComponentsInChildren<RoomTransition>())
                alvos.Add(($"passagem para {t.targetScene}", t.transform.position));
        }

        var inalcancaveis = new List<string>();
        foreach (var (nome, pos) in alvos)
        {
            bool perto = alcancaveis.Any(s =>
                Mathf.Abs(s.x - pos.x) <= 3f && Mathf.Abs(s.y - pos.y) <= 5f);
            if (!perto) inalcancaveis.Add($"{nome} em ({pos.x:F0}, {pos.y:F0})");
        }

        Assert.IsEmpty(inalcancaveis,
            $"em {System.IO.Path.GetFileName(caminho)} o jogador não chega em: " +
            string.Join("; ", inalcancaveis));
    }

    [TestCase("Assets/Scenes/01_OrlaDaVila.unity")]
    [TestCase("Assets/Scenes/02_FlorestaSilente.unity")]
    [TestCase("Assets/Scenes/03_LagoSilente.unity")]
    [TestCase("Assets/Scenes/04_CavernaMusgosa.unity")]
    [TestCase("Assets/Scenes/05_SantuarioEsquecido.unity")]
    public void NenhumaPlataforma_FicaIlhada(string caminho)
    {
        var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
        var raizes = cena.GetRootGameObjects();

        var superficies = LevantarSuperficies(raizes);
        var jogador = raizes.Select(r => r.GetComponentInChildren<PlayerController>())
                            .FirstOrDefault(p => p != null);
        var alcancaveis = Espalhar(superficies, jogador.transform.position, Pocos(raizes));

        var ilhadas = superficies.Except(alcancaveis).ToList();
        var colunas = ilhadas.Select(p => Mathf.RoundToInt(p.x)).Distinct().OrderBy(x => x).Take(10);

        Assert.IsEmpty(ilhadas,
            $"{ilhadas.Count} pontos de apoio fora de alcance em " +
            $"{System.IO.Path.GetFileName(caminho)} (colunas {string.Join(", ", colunas)}). " +
            "Plataforma que não dá para pisar é armadilha visual: o jogador " +
            "tenta chegar e não consegue.");
    }

    [TestCase("Assets/Scenes/01_OrlaDaVila.unity")]
    [TestCase("Assets/Scenes/02_FlorestaSilente.unity")]
    [TestCase("Assets/Scenes/04_CavernaMusgosa.unity")]
    [TestCase("Assets/Scenes/05_SantuarioEsquecido.unity")]
    public void RegioesSemAgua_NaoTemBuracoMortalNoChao(string caminho)
    {
        // Cair num vão e morrer sem nada visível ali é o tipo de morte que o
        // jogador não entende. Onde não há lago, o piso é contínuo.
        var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
        var tilemap = cena.GetRootGameObjects()
                          .Select(r => r.GetComponentInChildren<Tilemap>())
                          .FirstOrDefault(t => t != null);
        Assert.IsNotNull(tilemap);

        var limites = tilemap.cellBounds;
        var buracos = new List<int>();

        for (int x = limites.xMin; x < limites.xMax; x++)
        {
            bool temChao = false;
            for (int y = limites.yMin; y <= limites.yMin + 3; y++)
                if (tilemap.GetTile(new Vector3Int(x, y, 0)) != null) { temChao = true; break; }
            if (!temChao) buracos.Add(x);
        }

        Assert.IsEmpty(buracos,
            $"{System.IO.Path.GetFileName(caminho)} tem piso faltando nas colunas " +
            string.Join(", ", buracos.Take(12)));
    }

    [TestCase("Assets/Scenes/01_OrlaDaVila.unity")]
    [TestCase("Assets/Scenes/02_FlorestaSilente.unity")]
    [TestCase("Assets/Scenes/03_LagoSilente.unity")]
    [TestCase("Assets/Scenes/04_CavernaMusgosa.unity")]
    [TestCase("Assets/Scenes/05_SantuarioEsquecido.unity")]
    public void HabilidadeDoPoco_EstaAoAlcanceAntesDoPoco(string caminho)
    {
        // Se o poço fosse alcançável antes da habilidade que ele cobra, o
        // jogador ficaria preso no pé dele sem entender por quê. A varredura
        // aqui roda SEM os poços: o que abre o poço tem que ser pegável assim.
        var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
        var raizes = cena.GetRootGameObjects();

        var pocos = Pocos(raizes);
        if (pocos.Count == 0) Assert.Pass("região sem poço de escalada");

        var superficies = LevantarSuperficies(raizes);
        var jogador = raizes.Select(r => r.GetComponentInChildren<PlayerController>())
                            .FirstOrDefault(p => p != null);
        var semPoco = Espalhar(superficies, jogador.transform.position, new List<Rect>());

        var habilidades = raizes.SelectMany(r => r.GetComponentsInChildren<PickupAbility>())
                                .Where(p => p.abilityId == "wall_climb")
                                .ToList();

        Assert.IsNotEmpty(habilidades,
            "a região tem poço de escalada mas não tem onde pegar a habilidade");

        foreach (var h in habilidades)
        {
            var pos = h.transform.position;
            bool perto = semPoco.Any(s =>
                Mathf.Abs(s.x - pos.x) <= 3f && Mathf.Abs(s.y - pos.y) <= 5f);
            Assert.IsTrue(perto,
                $"a escalada de parede em ({pos.x:F0}, {pos.y:F0}) só é alcançável " +
                "com a própria escalada de parede");
        }
    }

    [TestCase("Assets/Scenes/04_CavernaMusgosa.unity")]
    public void PocoDeEscalada_TemParedesDeFrenteUmaParaAOutra(string caminho)
    {
        // Poço sem parede dos dois lados não dá salto de parede: dá queda.
        var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
        var raizes = cena.GetRootGameObjects();

        var tilemap = raizes.SelectMany(r => r.GetComponentsInChildren<Tilemap>())
                            .FirstOrDefault(t => t.name == "Ground");
        Assert.IsNotNull(tilemap, "sem tilemap de chão");

        foreach (var poco in raizes.SelectMany(r => r.GetComponentsInChildren<PocoDeEscalada>()))
        {
            var a = poco.area;
            var semParede = new List<int>();

            // metade de baixo do poço: é onde a subida começa e onde faltar
            // parede trava o jogador
            for (int y = Mathf.CeilToInt(a.yMin) + 2; y < Mathf.FloorToInt(a.yMax) - 1; y++)
            {
                bool esquerda = false, direita = false;
                for (int x = Mathf.CeilToInt(a.xMin); x <= Mathf.FloorToInt(a.xMax); x++)
                {
                    if (tilemap.GetTile(new Vector3Int(x, y, 0)) == null) continue;
                    if (x < a.center.x) esquerda = true; else direita = true;
                }
                if (!esquerda || !direita) semParede.Add(y);
            }

            // as duas fileiras do topo podem ser abertas: é a saída do poço
            Assert.LessOrEqual(semParede.Count, 2,
                $"o poço em {a} não tem parede dos dois lados nas alturas " +
                string.Join(", ", semParede));

            float largura = a.width;
            Assert.LessOrEqual(largura, 10f,
                $"o poço tem {largura:F0} de vão: largo demais para o salto de parede");
        }
    }

    // ------------------------------------------------------------ utilidades
    static List<Rect> Pocos(GameObject[] raizes) =>
        raizes.SelectMany(r => r.GetComponentsInChildren<PocoDeEscalada>())
              .Select(p => p.area)
              .ToList();

    /// <summary>Todo topo de tile com espaço livre acima para o corpo caber.</summary>
    static List<Vector2> LevantarSuperficies(GameObject[] raizes)
    {
        var achadas = new List<Vector2>();
        var mapas = raizes.SelectMany(r => r.GetComponentsInChildren<Tilemap>())
                          .Where(t => t.gameObject.layer == PrefabBuilder.LayerGround)
                          .ToList();

        foreach (var tilemap in mapas)
        {
            var limites = tilemap.cellBounds;
            for (int x = limites.xMin; x < limites.xMax; x++)
            {
                for (int y = limites.yMin; y < limites.yMax; y++)
                {
                    if (tilemap.GetTile(new Vector3Int(x, y, 0)) == null) continue;

                    // precisa de três células livres acima: é a altura da Kaida
                    bool livre = true;
                    for (int k = 1; k <= 3 && livre; k++)
                        foreach (var m in mapas)
                            if (m.GetTile(new Vector3Int(x, y + k, 0)) != null) { livre = false; break; }

                    if (livre) achadas.Add(new Vector2(x, y + 1));
                }
            }
        }
        return achadas;
    }

    /// <summary>
    /// Espalha a partir do ponto de partida, respeitando o alcance. Dentro de
    /// um poço de escalada o limite de subida não vale: ali o jogador sobe de
    /// parede em parede, e não de salto.
    /// </summary>
    static HashSet<Vector2> Espalhar(List<Vector2> superficies, Vector3 origem, List<Rect> pocos)
    {
        if (superficies.Count == 0) return new HashSet<Vector2>();

        var inicio = superficies.OrderBy(s =>
            Mathf.Abs(s.x - origem.x) * 2f + Mathf.Abs(s.y - origem.y)).First();

        var vistos = new HashSet<Vector2> { inicio };
        var fila = new Stack<Vector2>();
        fila.Push(inicio);

        while (fila.Count > 0)
        {
            var atual = fila.Pop();
            foreach (var s in superficies)
            {
                if (vistos.Contains(s)) continue;

                float dx = Mathf.Abs(s.x - atual.x);
                float dy = s.y - atual.y;

                bool noMesmoPoco = pocos.Any(p => p.Contains(atual) && p.Contains(s));

                if (dx > VaoMaximo && !noMesmoPoco) continue;
                if (dy > SubidaMaxima && !noMesmoPoco) continue;   // alto demais para subir
                if (-dy > QuedaMaxima) continue;                   // queda longa demais

                vistos.Add(s);
                fila.Push(s);
            }
        }
        return vistos;
    }
}
