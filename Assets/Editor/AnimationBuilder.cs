using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Monta os AnimationClips a partir das folhas já fatiadas e junta tudo num
/// AnimatorController por personagem.
///
/// Os nomes dos estados batem com o que o código chama em PlayAnim():
/// idle, run, jump, fall, dash, attack, hurt, death, wallcling.
/// Mudar um nome aqui quebra a animação em jogo — o Animator falha em
/// silêncio quando o estado não existe.
/// </summary>
public static class AnimationBuilder
{
    const string PastaAnim = "Assets/Animations";

    /// <summary>Um clipe: de qual folha, quais frames, a que velocidade.</summary>
    struct Clipe
    {
        public string estado;      // nome do estado no Animator
        public string folha;       // nome do arquivo sem extensão
        public int de, ate;        // intervalo de frames (inclusivo)
        public float fps;
        public bool loop;

        public Clipe(string estado, string folha, int de, int ate, float fps, bool loop)
        {
            this.estado = estado; this.folha = folha;
            this.de = de; this.ate = ate; this.fps = fps; this.loop = loop;
        }
    }

    [MenuItem("Kaida/2. Gerar animações")]
    public static void GerarTudo()
    {
        Pasta(PastaAnim);

        // --- Kaida ---------------------------------------------------------
        // O pulo e a queda saem da mesma folha (Jump-All): os primeiros frames
        // são a subida, os últimos a descida.
        Construir("Kaida", "Assets/Art/Player/Kaida", new[]
        {
            new Clipe("idle",      "Kaida-Idle",      0, 3,  8f,  true),
            new Clipe("run",       "Kaida-Run",       0, 7,  14f, true),
            new Clipe("jump",      "Kaida-JumpAir",   2, 7,  14f, false),
            new Clipe("fall",      "Kaida-JumpAir",   8, 13, 10f, true),
            new Clipe("dash",      "Kaida-JumpAir",   5, 6,  16f, true),
            new Clipe("attack",    "Kaida-Attack",    0, 7,  28f, false),
            new Clipe("hurt",      "Kaida-Dead",      0, 1,  10f, false),
            new Clipe("death",     "Kaida-Dead",      0, 7,  10f, false),
            new Clipe("wallcling", "Kaida-JumpAir",   9, 10, 6f,  true),
        });

        // --- Javali-Casca ---------------------------------------------------
        Construir("Boar", "Assets/Art/Enemies/Boar", new[]
        {
            new Clipe("idle",  "Boar-Idle", 0, 3, 6f,  true),
            new Clipe("walk",  "Boar-Walk", 0, 5, 8f,  true),
            new Clipe("run",   "Boar-Run",  0, 5, 14f, true),
            new Clipe("hurt",  "Boar-Hit",  0, 3, 12f, false),
            new Clipe("death", "Boar-Hit",  0, 3, 8f,  false),
        });

        // --- Abelha-Eco ------------------------------------------------------
        Construir("Bee", "Assets/Art/Enemies/Bee", new[]
        {
            new Clipe("idle",   "Bee-Fly",    0, 3, 14f, true),
            new Clipe("walk",   "Bee-Fly",    0, 3, 14f, true),
            new Clipe("fly",    "Bee-Fly",    0, 3, 14f, true),
            new Clipe("attack", "Bee-Attack", 0, 3, 16f, true),
            new Clipe("hurt",   "Bee-Hit",    0, 3, 14f, false),
            new Clipe("death",  "Bee-Hit",    0, 3, 8f,  false),
        });

        // --- Caracol-Rastejante ----------------------------------------------
        Construir("Snail", "Assets/Art/Enemies/Snail", new[]
        {
            new Clipe("idle",  "Snail-Walk", 0, 1, 4f,  true),
            new Clipe("walk",  "Snail-Walk", 0, 7, 8f,  true),
            new Clipe("hide",  "Snail-Hide", 0, 7, 12f, false),
            new Clipe("hurt",  "Snail-Hide", 0, 3, 12f, false),
            new Clipe("death", "Snail-Dead", 0, 7, 10f, false),
        });

        // --- Guardião do Lúmen -----------------------------------------------
        // Sem sprite próprio de chefe no pacote: reaproveita a abelha em escala
        // grande com tint no prefab. Fica coerente com a paleta do resto.
        Construir("Guardian", "Assets/Art/Enemies/Bee", new[]
        {
            new Clipe("idle",   "Bee-Fly",    0, 3, 8f,  true),
            new Clipe("attack", "Bee-Attack", 0, 3, 12f, true),
            new Clipe("hurt",   "Bee-Hit",    0, 3, 10f, false),
            new Clipe("death",  "Bee-Hit",    0, 3, 6f,  false),
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Kaida] Animações e controllers gerados.");
    }

    static void Construir(string personagem, string pastaArte, Clipe[] clipes)
    {
        string destino = $"{PastaAnim}/{personagem}";
        Pasta(destino);

        var controller = AnimatorController.CreateAnimatorControllerAtPath($"{destino}/{personagem}.controller");
        var maquina = controller.layers[0].stateMachine;
        bool primeiro = true;

        foreach (var c in clipes)
        {
            var sprites = CarregarSprites($"{pastaArte}/{c.folha}.png");
            if (sprites.Length == 0)
            {
                Debug.LogWarning($"[Kaida] Folha não encontrada ou não fatiada: {pastaArte}/{c.folha}.png");
                continue;
            }

            int de = Mathf.Clamp(c.de, 0, sprites.Length - 1);
            int ate = Mathf.Clamp(c.ate, de, sprites.Length - 1);

            var clip = CriarClipe(sprites, de, ate, c.fps, c.loop, $"{personagem}_{c.estado}");
            AssetDatabase.CreateAsset(clip, $"{destino}/{personagem}_{c.estado}.anim");

            var estado = maquina.AddState(c.estado);
            estado.motion = clip;
            estado.writeDefaultValues = false;

            if (primeiro) { maquina.defaultState = estado; primeiro = false; }
        }

        EditorUtility.SetDirty(controller);
    }

    /// <summary>Carrega os sprites de uma folha já fatiada, em ordem de frame.</summary>
    static Sprite[] CarregarSprites(string caminho)
    {
        var todos = AssetDatabase.LoadAllAssetsAtPath(caminho);
        return todos.OfType<Sprite>()
                    .OrderBy(s => IndiceDoNome(s.name))
                    .ToArray();
    }

    /// <summary>Os frames se chamam "Folha_0", "Folha_10"... ordenar por texto erraria a ordem.</summary>
    static int IndiceDoNome(string nome)
    {
        int corte = nome.LastIndexOf('_');
        if (corte < 0 || corte == nome.Length - 1) return 0;
        return int.TryParse(nome.Substring(corte + 1), out int n) ? n : 0;
    }

    static AnimationClip CriarClipe(Sprite[] sprites, int de, int ate, float fps, bool loop, string nome)
    {
        var clip = new AnimationClip { name = nome, frameRate = fps };

        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        int total = ate - de + 1;
        var keys = new ObjectReferenceKeyframe[total];
        for (int i = 0; i < total; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / fps,
                value = sprites[de + i]
            };
        }
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        return clip;
    }

    static void Pasta(string caminho)
    {
        if (AssetDatabase.IsValidFolder(caminho)) return;
        string pai = System.IO.Path.GetDirectoryName(caminho).Replace('\\', '/');
        string nome = System.IO.Path.GetFileName(caminho);
        if (!AssetDatabase.IsValidFolder(pai)) Pasta(pai);
        AssetDatabase.CreateFolder(pai, nome);
    }
}
