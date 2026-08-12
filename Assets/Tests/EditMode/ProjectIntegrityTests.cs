using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Verifica que o que o builder gerou casa com o que o código espera.
///
/// O caso que mais dói na prática: PlayAnim("dash") num Animator sem o estado
/// "dash". A Unity não reclama, a animação só não toca. Aqui isso vira erro
/// de teste.
/// </summary>
public class ProjectIntegrityTests
{
    const string CenaMenu = "Assets/Scenes/00_MenuPrincipal.unity";

    static readonly string[] Cenas =
    {
        "Assets/Scenes/01_OrlaDaVila.unity",
        "Assets/Scenes/02_FlorestaSilente.unity",
        "Assets/Scenes/03_LagoSilente.unity",
        "Assets/Scenes/04_CavernaMusgosa.unity",
        "Assets/Scenes/05_SantuarioEsquecido.unity",
    };

    /// <summary>Última região da progressão: onde o Guardião espera.</summary>
    static string CenaFinal => Cenas[Cenas.Length - 1];

    static string[] TodasAsCenas()
    {
        var lista = new List<string> { CenaMenu };
        lista.AddRange(Cenas);
        return lista.ToArray();
    }

    // exatamente os nomes que PlayerController.PlayAnim recebe pelos estados
    static readonly string[] EstadosDaKaida =
    { "idle", "run", "jump", "fall", "dash", "attack", "hurt", "death", "wallcling" };

    // EnemyController toca estes
    static readonly string[] EstadosDeInimigo = { "walk", "hurt", "death" };

    // ------------------------------------------------------------ animators
    [Test]
    public void AnimatorDaKaida_TemTodosOsEstadosQueOCodigoChama()
    {
        var estados = EstadosDoController("Assets/Animations/Kaida/Kaida.controller");
        foreach (var esperado in EstadosDaKaida)
        {
            Assert.Contains(esperado, estados,
                $"PlayAnim(\"{esperado}\") não encontraria estado no Animator da Kaida");
        }
    }

    [TestCase("Boar")]
    [TestCase("Bee")]
    [TestCase("Snail")]
    public void AnimatorDeInimigo_TemOsEstadosBasicos(string personagem)
    {
        var estados = EstadosDoController($"Assets/Animations/{personagem}/{personagem}.controller");
        foreach (var esperado in EstadosDeInimigo)
            Assert.Contains(esperado, estados, $"{personagem} não tem o estado '{esperado}'");
    }

    [Test]
    public void AnimatorDoChefe_TemOsEstadosQueOsEstadosDoBossChamam()
    {
        var estados = EstadosDoController("Assets/Animations/Guardian/Guardian.controller");
        foreach (var esperado in new[] { "idle", "attack", "hurt", "death" })
            Assert.Contains(esperado, estados, $"Guardião não tem o estado '{esperado}'");
    }

    [Test]
    public void TodosOsClipes_TemFramesDeVerdade()
    {
        var guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets/Animations" });
        Assert.Greater(guids.Length, 0, "nenhum clipe de animação foi gerado");

        foreach (var guid in guids)
        {
            var caminho = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(caminho);
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            Assert.Greater(bindings.Length, 0, $"{clip.name} não tem curva de sprite");

            var keys = AnimationUtility.GetObjectReferenceCurve(clip, bindings[0]);
            Assert.Greater(keys.Length, 0, $"{clip.name} está sem frames");
            foreach (var k in keys)
                Assert.IsNotNull(k.value, $"{clip.name} tem um frame vazio");
        }
    }

    // -------------------------------------------------------------- sprites
    [Test]
    public void FolhasDaKaida_ForamFatiadasEmVariosFrames()
    {
        string[] folhas =
        {
            "Assets/Art/Player/Kaida/Kaida-Idle.png",
            "Assets/Art/Player/Kaida/Kaida-Run.png",
            "Assets/Art/Player/Kaida/Kaida-Attack.png",
            "Assets/Art/Player/Kaida/Kaida-JumpAir.png",
        };
        foreach (var f in folhas)
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(f).OfType<Sprite>().ToArray();
            Assert.Greater(sprites.Length, 1, $"{f} deveria estar fatiada em vários frames");
        }
    }

    [TestCase("Assets/Art/Enemies/Boar", 48)]
    [TestCase("Assets/Art/Enemies/Snail", 48)]
    [TestCase("Assets/Art/Enemies/Bee", 64)]
    public void FramesDeUmMesmoPersonagem_TemSempreALarguraCerta(string pasta, int larguraEsperada)
    {
        // O javali tem o rabo separado do corpo, o que confunde a contagem de
        // frames. Se o detector escolher a largura errada, dois bichos acabam
        // dentro do mesmo frame e a animação fica com o dobro do tamanho.
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { pasta });
        Assert.Greater(guids.Length, 0, $"nenhuma folha em {pasta}");

        foreach (var guid in guids)
        {
            var caminho = AssetDatabase.GUIDToAssetPath(guid);
            var sprites = AssetDatabase.LoadAllAssetsAtPath(caminho).OfType<Sprite>().ToArray();
            Assert.Greater(sprites.Length, 1, $"{caminho} não foi fatiada");

            foreach (var s in sprites)
            {
                Assert.AreEqual(larguraEsperada, (int)s.rect.width,
                    $"{caminho} / {s.name}: frame de {s.rect.width}px, esperado {larguraEsperada}px");
            }
        }
    }

    [Test]
    public void PivosDaKaida_AlinhamOsPes_EntreAnimacoes()
    {
        // O idle tem 16px de folga embaixo e o pulo tem 0. Se os pivôs não
        // compensarem isso, a personagem afunda ao trocar de animação.
        float pesIdle = AlturaDoPivoEmPixels("Assets/Art/Player/Kaida/Kaida-Idle.png");
        float pesPulo = AlturaDoPivoEmPixels("Assets/Art/Player/Kaida/Kaida-JumpAir.png");
        float pesCorrida = AlturaDoPivoEmPixels("Assets/Art/Player/Kaida/Kaida-Run.png");

        Assert.AreEqual(0f, pesPulo, 1.5f, "no pulo os pés estão na base da folha");
        Assert.Greater(pesIdle, 8f, "o idle tem folga embaixo e o pivô precisa subir junto");
        Assert.Greater(pesCorrida, 6f, "a corrida também tem folga embaixo");
    }

    /// <summary>Distância entre a base da folha e o pivô, em pixels.</summary>
    static float AlturaDoPivoEmPixels(string caminho)
    {
        var sprite = AssetDatabase.LoadAllAssetsAtPath(caminho).OfType<Sprite>().FirstOrDefault();
        Assert.IsNotNull(sprite, $"{caminho} não tem sprites");
        return sprite.pivot.y;
    }

    // -------------------------------------------------------------- prefabs
    [Test]
    public void PrefabDaKaida_EstaCompleto()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Kaida.prefab");
        Assert.IsNotNull(prefab, "prefab da Kaida não foi gerado");

        var pc = prefab.GetComponent<PlayerController>();
        Assert.IsNotNull(pc, "falta o PlayerController");
        Assert.IsNotNull(pc.stats, "PlayerStats não foi ligado");
        Assert.IsNotNull(pc.groundCheck, "falta o GroundCheck");
        Assert.IsNotNull(pc.attackPoint, "falta o AttackPoint");
        Assert.IsNotNull(pc.animator, "falta o Animator");
        Assert.IsNotNull(pc.spriteRenderer, "falta o SpriteRenderer");
        Assert.IsNotNull(pc.animator.runtimeAnimatorController, "o Animator está sem controller");

        Assert.AreNotEqual(0, pc.groundLayer.value, "groundLayer não pode ficar vazia");
        Assert.AreNotEqual(0, pc.enemyLayer.value, "enemyLayer não pode ficar vazia");

        var rb = prefab.GetComponent<Rigidbody2D>();
        Assert.IsNotNull(rb);
        Assert.AreEqual(0f, rb.gravityScale, "a gravidade é aplicada por código, não pelo Rigidbody");
        Assert.IsTrue(rb.freezeRotation, "o corpo não pode girar");
        Assert.IsNotNull(prefab.GetComponent<Collider2D>(), "falta collider");
    }

    [TestCase("Assets/Prefabs/Inimigo_JavaliCasca.prefab")]
    [TestCase("Assets/Prefabs/Inimigo_AbelhaEco.prefab")]
    [TestCase("Assets/Prefabs/Inimigo_CaracolRastejante.prefab")]
    public void PrefabsDeInimigo_EstaoCompletos(string caminho)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
        Assert.IsNotNull(prefab, $"{caminho} não foi gerado");

        var ec = prefab.GetComponent<EnemyController>();
        Assert.IsNotNull(ec, "falta o EnemyController (ou subclasse)");
        Assert.IsNotNull(ec.animator, "falta o Animator");
        Assert.IsNotNull(ec.animator.runtimeAnimatorController, "Animator sem controller");
        Assert.IsNotNull(ec.spriteRenderer, "falta o SpriteRenderer");
        Assert.IsNotNull(ec.groundCheck, "sem groundCheck o inimigo cai da plataforma");
        Assert.IsNotNull(ec.wallCheck, "sem wallCheck o inimigo empurra parede");
        Assert.AreNotEqual(0, ec.groundLayer.value);
        Assert.AreNotEqual(0, ec.playerLayer.value);
        Assert.Greater(ec.maxHealth, 0);
    }

    [Test]
    public void PrefabDoChefe_TemFeixeEEcosLigados()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GuardiaoDoLumen.prefab");
        Assert.IsNotNull(prefab, "prefab do Guardião não foi gerado");

        var boss = prefab.GetComponent<GuardianBoss>();
        Assert.IsNotNull(boss);
        Assert.IsNotNull(boss.beamPrefab, "sem beamPrefab ele não ataca à distância");
        Assert.IsNotNull(boss.beamOrigin, "falta o ponto de origem do feixe");
        Assert.Greater(boss.maxHealth, 0, "o chefe precisa ter vida para poder morrer");
        Assert.AreNotEqual(0, boss.playerLayer.value);
    }

    // ---------------------------------------------------------------- cenas
    [Test]
    public void AsQuatroCenas_Existem()
    {
        foreach (var c in Cenas)
            Assert.IsTrue(System.IO.File.Exists(c), $"cena faltando: {c}");
    }

    [Test]
    public void BuildSettings_TemTodasAsCenasNaOrdem()
    {
        var esperadas = TodasAsCenas();
        var registradas = EditorBuildSettings.scenes.Select(s => s.path).ToArray();

        Assert.AreEqual(esperadas.Length, registradas.Length, "quantidade de cenas em Build Settings");
        for (int i = 0; i < esperadas.Length; i++)
            Assert.AreEqual(esperadas[i], registradas[i], $"ordem errada na posição {i}");

        Assert.AreEqual(CenaMenu, registradas[0],
            "o executável abre a primeira cena da lista: tem que ser o menu");
    }

    [Test]
    public void TelaInicial_ExisteETemOMenu()
    {
        Assert.IsTrue(System.IO.File.Exists(CenaMenu), "a cena do menu não foi gerada");

        var cena = EditorSceneManager.OpenScene(CenaMenu, OpenSceneMode.Single);
        var raizes = cena.GetRootGameObjects();

        Assert.IsNotNull(raizes.Select(r => r.GetComponentInChildren<MainMenu>()).FirstOrDefault(m => m != null),
            "a tela inicial não tem o MainMenu");
        Assert.IsNotNull(raizes.Select(r => r.GetComponentInChildren<Camera>()).FirstOrDefault(c => c != null),
            "sem câmera a tela inicial fica preta");
        Assert.IsNotNull(raizes.Select(r => r.GetComponentInChildren<SaveSystem>()).FirstOrDefault(s => s != null),
            "o menu precisa do SaveSystem para saber se o botão Continuar vale");
    }

    [TestCase("Assets/Scenes/01_OrlaDaVila.unity")]
    [TestCase("Assets/Scenes/02_FlorestaSilente.unity")]
    [TestCase("Assets/Scenes/03_LagoSilente.unity")]
    [TestCase("Assets/Scenes/04_CavernaMusgosa.unity")]
    [TestCase("Assets/Scenes/05_SantuarioEsquecido.unity")]
    public void CadaRegiao_TemPausaETelaDeMorte(string caminho)
    {
        var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
        var raizes = cena.GetRootGameObjects();

        Assert.IsNotNull(raizes.Select(r => r.GetComponentInChildren<PauseMenu>()).FirstOrDefault(p => p != null),
            "sem menu de pausa não há como sair do jogo");
        Assert.IsNotNull(raizes.Select(r => r.GetComponentInChildren<GameOverUI>()).FirstOrDefault(g => g != null),
            "sem tela de morte o jogador não sabe o que aconteceu");
    }

    [Test]
    public void CenaFinal_TemTelaDeVitoria()
    {
        var cena = EditorSceneManager.OpenScene(CenaFinal, OpenSceneMode.Single);
        var vitoria = cena.GetRootGameObjects()
                          .Select(r => r.GetComponentInChildren<VictoryUI>())
                          .FirstOrDefault(v => v != null);

        Assert.IsNotNull(vitoria, "vencer o chefe precisa levar a algum lugar");
    }

    [Test]
    public void ArenaDoChefe_ColocaOGuardiaoAoAlcanceDoJogador()
    {
        // Ele flutua: se ficar alto demais, as fases 1 e 2 viram impasse,
        // porque o ataque da Kaida é corpo a corpo.
        var cena = EditorSceneManager.OpenScene(CenaFinal, OpenSceneMode.Single);
        var raizes = cena.GetRootGameObjects();

        var boss = raizes.Select(r => r.GetComponentInChildren<GuardianBoss>()).FirstOrDefault(b => b != null);
        Assert.IsNotNull(boss);

        // Todos os tilemaps de chão, não só o primeiro: as plataformas ficam
        // numa camada separada da do piso, por serem atravessáveis por baixo.
        var mapas = raizes.SelectMany(r => r.GetComponentsInChildren<Tilemap>())
                          .Where(t => t.gameObject.layer == PrefabBuilder.LayerGround)
                          .ToList();
        Assert.Greater(mapas.Count, 0, "nenhum tilemap de chão na arena");

        float alturaDoChefe = boss.transform.position.y;

        // procura a plataforma mais alta abaixo do chefe, num raio horizontal razoável
        float melhorPlataforma = float.MinValue;
        foreach (var tilemap in mapas)
        {
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.GetTile(pos) == null) continue;
                var mundo = tilemap.GetCellCenterWorld(pos);
                if (Mathf.Abs(mundo.x - boss.transform.position.x) > 22f) continue;
                if (mundo.y >= alturaDoChefe) continue;
                melhorPlataforma = Mathf.Max(melhorPlataforma, mundo.y);
            }
        }

        Assert.Greater(melhorPlataforma, float.MinValue, "não há plataforma nenhuma sob o chefe");

        float subida = alturaDoChefe - melhorPlataforma;
        var stats = AssetDatabase.LoadAssetAtPath<PlayerStats>("Assets/Settings/PlayerStats.asset");
        Assert.IsNotNull(stats);

        // pulo simples mais a altura da própria Kaida e o alcance do ataque
        float alcanceVertical = stats.jumpHeight + 2.5f;
        Assert.LessOrEqual(subida, alcanceVertical,
            $"o Guardião está {subida:F1} unidades acima da plataforma mais próxima, " +
            $"e a Kaida só alcança {alcanceVertical:F1}");
    }

    [TestCase("Assets/Scenes/01_OrlaDaVila.unity")]
    [TestCase("Assets/Scenes/02_FlorestaSilente.unity")]
    [TestCase("Assets/Scenes/03_LagoSilente.unity")]
    [TestCase("Assets/Scenes/04_CavernaMusgosa.unity")]
    [TestCase("Assets/Scenes/05_SantuarioEsquecido.unity")]
    public void CadaCena_TemOEssencialParaJogar(string caminho)
    {
        var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
        var raizes = cena.GetRootGameObjects();

        var jogador = raizes.Select(r => r.GetComponentInChildren<PlayerController>())
                            .FirstOrDefault(p => p != null);
        Assert.IsNotNull(jogador, "a cena não tem a Kaida");

        var camera = raizes.Select(r => r.GetComponentInChildren<Camera>())
                           .FirstOrDefault(c => c != null);
        Assert.IsNotNull(camera, "a cena não tem câmera");
        Assert.IsTrue(camera.orthographic, "a câmera de um jogo 2D precisa ser ortográfica");

        var seguidor = camera.GetComponent<CameraFollow2D>();
        Assert.IsNotNull(seguidor, "a câmera não segue ninguém");
        Assert.IsNotNull(seguidor.target, "o alvo da câmera está vazio");

        var tilemap = raizes.Select(r => r.GetComponentInChildren<Tilemap>())
                            .FirstOrDefault(t => t != null);
        Assert.IsNotNull(tilemap, "a cena não tem tilemap");
        Assert.Greater(tilemap.GetUsedTilesCount(), 0, "o tilemap está vazio - sem chão não dá para jogar");

        var colisor = tilemap.GetComponent<TilemapCollider2D>();
        Assert.IsNotNull(colisor, "o chão não tem collider: a Kaida cairia para sempre");

        Assert.IsNotNull(raizes.Select(r => r.GetComponentInChildren<GameManager>()).FirstOrDefault(g => g != null),
            "falta o GameManager (checkpoint e respawn)");
        Assert.IsNotNull(raizes.Select(r => r.GetComponentInChildren<SaveSystem>()).FirstOrDefault(s => s != null),
            "falta o SaveSystem (habilidades e coletáveis)");
    }

    [Test]
    public void OJogador_ComecaAcimaDoChao_EmTodasAsCenas()
    {
        foreach (var caminho in Cenas)
        {
            var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
            var jogador = cena.GetRootGameObjects()
                              .Select(r => r.GetComponentInChildren<PlayerController>())
                              .FirstOrDefault(p => p != null);
            var tilemap = cena.GetRootGameObjects()
                              .Select(r => r.GetComponentInChildren<Tilemap>())
                              .FirstOrDefault(t => t != null);

            var pos = jogador.transform.position;
            var celula = tilemap.WorldToCell(pos);
            Assert.IsNull(tilemap.GetTile(celula),
                $"em {caminho} a Kaida nasce dentro de um bloco sólido");
        }
    }

    [Test]
    public void Transicoes_ApontamParaCenasQueExistem()
    {
        foreach (var caminho in Cenas)
        {
            var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
            var transicoes = cena.GetRootGameObjects()
                                 .SelectMany(r => r.GetComponentsInChildren<RoomTransition>());

            foreach (var t in transicoes)
            {
                Assert.IsFalse(string.IsNullOrEmpty(t.targetScene),
                    $"transição sem destino em {caminho}");
                bool existe = Cenas.Any(c => c.EndsWith($"/{t.targetScene}.unity"));
                Assert.IsTrue(existe,
                    $"em {caminho} a passagem aponta para '{t.targetScene}', que não existe");
            }
        }
    }

    [Test]
    public void CadaRegiao_TemPeloMenosUmCheckpoint()
    {
        foreach (var caminho in Cenas)
        {
            var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
            var marcos = cena.GetRootGameObjects()
                             .SelectMany(r => r.GetComponentsInChildren<Checkpoint>())
                             .Count();
            Assert.Greater(marcos, 0, $"{caminho} não tem marco de descanso: morrer voltaria longe demais");
        }
    }

    [Test]
    public void AsHabilidadesDoGdd_EstaoNoMundo()
    {
        var encontradas = new List<string>();
        foreach (var caminho in Cenas)
        {
            var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
            encontradas.AddRange(cena.GetRootGameObjects()
                .SelectMany(r => r.GetComponentsInChildren<PickupAbility>())
                .Select(p => p.abilityId));
        }

        Assert.Contains("double_jump", encontradas, "o Pulo Duplo não está em lugar nenhum do mapa");
    }

    [TestCase("Assets/Scenes/01_OrlaDaVila.unity")]
    [TestCase("Assets/Scenes/02_FlorestaSilente.unity")]
    [TestCase("Assets/Scenes/03_LagoSilente.unity")]
    [TestCase("Assets/Scenes/04_CavernaMusgosa.unity")]
    [TestCase("Assets/Scenes/05_SantuarioEsquecido.unity")]
    public void OChao_TemColisaoDeVerdade(string caminho)
    {
        // `usedByComposite` só pega se o CompositeCollider2D já existir no
        // objeto. Se a ordem estiver trocada, o chão fica sem colisão nenhuma
        // e a Kaida atravessa o mapa no primeiro frame.
        var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
        var tilemap = cena.GetRootGameObjects()
                          .Select(r => r.GetComponentInChildren<Tilemap>())
                          .FirstOrDefault(t => t != null);
        Assert.IsNotNull(tilemap);

        var colisor = tilemap.GetComponent<TilemapCollider2D>();
        var composto = tilemap.GetComponent<CompositeCollider2D>();
        var corpo = tilemap.GetComponent<Rigidbody2D>();

        Assert.IsNotNull(colisor, "o tilemap não tem TilemapCollider2D");
        Assert.IsNotNull(composto, "o tilemap não tem CompositeCollider2D");
        Assert.IsNotNull(corpo, "o chão precisa de Rigidbody2D estático");
        Assert.AreEqual(RigidbodyType2D.Static, corpo.bodyType);

        Assert.IsTrue(colisor.usedByComposite,
            "o TilemapCollider2D não está ligado ao composto: o chão não colide");

        // A geometria em si não é conferida aqui: o CompositeCollider2D só a
        // constrói quando a física roda, e EditMode não simula física. Quem
        // verifica de verdade é ChaoDasCenas_SustentaAKaida, em PlayMode.
    }

    [TestCase("Assets/Scenes/01_OrlaDaVila.unity")]
    [TestCase("Assets/Scenes/02_FlorestaSilente.unity")]
    [TestCase("Assets/Scenes/03_LagoSilente.unity")]
    [TestCase("Assets/Scenes/04_CavernaMusgosa.unity")]
    [TestCase("Assets/Scenes/05_SantuarioEsquecido.unity")]
    public void OJogador_NasceEmCimaDeChaoFirme(string caminho)
    {
        var cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
        var raizes = cena.GetRootGameObjects();
        var jogador = raizes.Select(r => r.GetComponentInChildren<PlayerController>())
                            .FirstOrDefault(p => p != null);
        var tilemap = raizes.Select(r => r.GetComponentInChildren<Tilemap>())
                            .FirstOrDefault(t => t != null);

        var pos = jogador.transform.position;

        // procura chão em algum ponto abaixo do jogador
        bool achouChao = false;
        for (int desce = 1; desce <= 25; desce++)
        {
            var celula = tilemap.WorldToCell(pos + Vector3.down * desce);
            if (tilemap.GetTile(celula) != null) { achouChao = true; break; }
        }

        Assert.IsTrue(achouChao,
            $"em {caminho} a Kaida nasce sobre o vazio e cai assim que o jogo começa");
    }

    [TestCase("Assets/Prefabs/Checkpoint.prefab", 4f)]
    [TestCase("Assets/Prefabs/PickupHabilidade.prefab", 3f)]
    [TestCase("Assets/Prefabs/FragmentoDeLumen.prefab", 3f)]
    [TestCase("Assets/Prefabs/NoduloDeVida.prefab", 3f)]
    [TestCase("Assets/Prefabs/LumenBeam.prefab", 2f)]
    public void ObjetosDeMundo_TemSpriteRecortado_NaoAFolhaInteira(string caminho, float alturaMaxima)
    {
        // Uma folha de props inteira mede mais de 18 unidades. Usada como
        // sprite de um objeto, ela cobre a tela de pedras e runas soltas.
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
        Assert.IsNotNull(prefab, $"{caminho} não foi gerado");

        var sr = prefab.GetComponentInChildren<SpriteRenderer>();
        Assert.IsNotNull(sr, "falta SpriteRenderer");
        Assert.IsNotNull(sr.sprite, $"{caminho} está sem sprite");

        var tamanho = sr.sprite.bounds.size * prefab.transform.localScale.y;
        Assert.LessOrEqual(tamanho.y, alturaMaxima,
            $"{caminho} usa um sprite de {tamanho.y:F1} unidades de altura - " +
            "parece a folha inteira em vez de uma peça recortada");
        Assert.LessOrEqual(tamanho.x, alturaMaxima + 1f,
            $"{caminho} tem {tamanho.x:F1} unidades de largura");
    }

    [Test]
    public void OChefe_EstaNaCenaFinal()
    {
        var cena = EditorSceneManager.OpenScene(CenaFinal, OpenSceneMode.Single);
        var boss = cena.GetRootGameObjects()
                       .Select(r => r.GetComponentInChildren<GuardianBoss>())
                       .FirstOrDefault(b => b != null);

        Assert.IsNotNull(boss, "o Guardião não está no Santuário");
        Assert.IsNotNull(boss.player, "o chefe não sabe quem perseguir");
        Assert.Greater(boss.maxHealth, 0, "o chefe nasceu sem vida");
    }

    // ----------------------------------------------------------- utilidades
    static List<string> EstadosDoController(string caminho)
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(caminho);
        Assert.IsNotNull(controller, $"controller não encontrado: {caminho}");

        var nomes = new List<string>();
        foreach (var layer in controller.layers)
            foreach (var s in layer.stateMachine.states)
                nomes.Add(s.state.name);
        return nomes;
    }
}
