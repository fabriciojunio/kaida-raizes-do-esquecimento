using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trilha sonora gerada por síntese, em tempo de execução.
///
/// O pacote de arte não traz nenhum áudio, então em vez de deixar o jogo mudo
/// a música é construída aqui: escala menor, arpejo lento e um baixo longo por
/// baixo. Cada região recebe uma tônica e um andamento diferentes, o que dá
/// identidade sonora sem precisar de arquivo nenhum.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class TrilhaSonora : MonoBehaviour
{
    public static TrilhaSonora Instance { get; private set; }

    [Header("Caráter da região")]
    [Tooltip("Nota base em Hz. Mais grave = mais pesado.")]
    public float tonica = 220f;              // lá
    [Tooltip("Segundos por nota do arpejo.")]
    public float duracaoDaNota = 0.72f;
    [Range(0f, 1f)] public float volume = 0.07f;
    [Tooltip("Compassos antes de repetir.")]
    public int compassos = 8;

    const int TaxaDeAmostragem = 44100;

    AudioSource fonte;

    void Awake()
    {
        // uma trilha só atravessa as cenas; ao trocar de região a nova
        // instância ajusta os parâmetros da que já está tocando
        if (Instance != null && Instance != this)
        {
            Instance.Reconfigurar(tonica, duracaoDaNota, compassos, volume);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        fonte = GetComponent<AudioSource>();
        fonte.loop = true;
        fonte.playOnAwake = false;
        fonte.clip = Gerar();
        AplicarVolumeGeral();
        fonte.Play();
    }

    /// <summary>
    /// Junta o volume da região com o ajuste escolhido pelo jogador nas
    /// opções. Chamado de novo sempre que ele mexe no controle.
    /// </summary>
    public void AplicarVolumeGeral()
    {
        if (fonte == null) return;
        fonte.volume = volume * GameSettings.Volume;
    }

    public void Reconfigurar(float novaTonica, float novaDuracao, int novosCompassos, float novoVolume)
    {
        bool mudou = !Mathf.Approximately(tonica, novaTonica)
                  || !Mathf.Approximately(duracaoDaNota, novaDuracao)
                  || compassos != novosCompassos;

        tonica = novaTonica;
        duracaoDaNota = novaDuracao;
        compassos = novosCompassos;
        volume = novoVolume;

        if (fonte == null) return;
        AplicarVolumeGeral();
        if (!mudou) return;

        fonte.Stop();
        fonte.clip = Gerar();
        fonte.Play();
    }

    /// <summary>Monta o clipe inteiro: arpejo por cima, baixo sustentado embaixo.</summary>
    AudioClip Gerar()
    {
        // Escala menor natural, em semitons. É o que dá o tom melancólico
        // que combina com um vale que esqueceu de si mesmo.
        int[] escalaMenor = { 0, 2, 3, 5, 7, 8, 10, 12 };

        // desenho do arpejo: sobe, hesita, desce
        int[] desenho = { 0, 2, 4, 7, 4, 2, 3, 0 };

        int notasPorCompasso = desenho.Length;
        int totalDeNotas = notasPorCompasso * compassos;
        int amostrasPorNota = Mathf.RoundToInt(TaxaDeAmostragem * duracaoDaNota);
        int total = amostrasPorNota * totalDeNotas;

        var dados = new float[total];
        var aleatorio = new System.Random(1987);

        // --- baixo: uma nota longa por compasso ---
        int[] grausDoBaixo = { 0, 5, 3, 4 };
        int amostrasPorCompasso = amostrasPorNota * notasPorCompasso;
        for (int c = 0; c < compassos; c++)
        {
            int grau = escalaMenor[grausDoBaixo[c % grausDoBaixo.Length]];
            float freq = tonica * 0.5f * Mathf.Pow(2f, grau / 12f);
            Somar(dados, c * amostrasPorCompasso, amostrasPorCompasso, freq, 0.22f, 0.35f, ondaQuadrada: false);
        }

        // --- arpejo ---
        for (int n = 0; n < totalDeNotas; n++)
        {
            int compassoAtual = n / notasPorCompasso;
            int passo = desenho[n % desenho.Length];

            // a cada duas voltas, sobe uma oitava: evita que fique monótono
            int oitava = (compassoAtual >= compassos / 2) ? 1 : 0;
            int grau = escalaMenor[passo % escalaMenor.Length];
            float freq = tonica * Mathf.Pow(2f, (grau + 12 * oitava) / 12f);

            // pequena variação de volume, para não soar mecânico
            float ganho = 0.16f + (float)aleatorio.NextDouble() * 0.05f;
            Somar(dados, n * amostrasPorNota, amostrasPorNota, freq, ganho, 0.12f, ondaQuadrada: true);
        }

        Normalizar(dados);

        var clip = AudioClip.Create("TrilhaKaida", total, 1, TaxaDeAmostragem, false);
        clip.SetData(dados, 0);
        return clip;
    }

    /// <summary>Soma uma nota com envelope suave, para não estalar no ataque.</summary>
    static void Somar(float[] dados, int inicio, int duracao, float frequencia,
                      float ganho, float ataque, bool ondaQuadrada)
    {
        // queda longa: as notas se dissolvem umas nas outras em vez de
        // pontuarem o tempo, o que soa mais como ambiente e menos como música
        int amostrasDeAtaque = Mathf.Max(1, (int)(duracao * ataque));
        int amostrasDeQueda = Mathf.Max(1, (int)(duracao * 0.7f));

        for (int i = 0; i < duracao; i++)
        {
            int pos = inicio + i;
            if (pos < 0 || pos >= dados.Length) continue;

            float t = (float)i / TaxaDeAmostragem;
            float fase = 2f * Mathf.PI * frequencia * t;

            // Quase senoidal pura. Os harmônicos ficam bem discretos porque
            // som ambiente não pode ter borda áspera: o que chama atenção
            // numa trilha de fundo é justamente o brilho dos agudos.
            float onda = ondaQuadrada
                ? Mathf.Sin(fase) + 0.08f * Mathf.Sin(fase * 2f) + 0.03f * Mathf.Sin(fase * 3f)
                : Mathf.Sin(fase) + 0.12f * Mathf.Sin(fase * 0.5f);

            // Envelope em curva, não em rampa reta: a entrada e a saída de
            // cada nota ficam macias, sem o "clique" que uma rampa linear
            // deixa audível em volume baixo.
            float envelope;
            if (i < amostrasDeAtaque)
            {
                float x = (float)i / amostrasDeAtaque;
                envelope = x * x;
            }
            else if (i > duracao - amostrasDeQueda)
            {
                float x = (float)(duracao - i) / amostrasDeQueda;
                envelope = x * x;
            }
            else envelope = 1f;

            dados[pos] += onda * ganho * envelope;
        }
    }

    static void Normalizar(float[] dados)
    {
        float pico = 0f;
        foreach (var v in dados) pico = Mathf.Max(pico, Mathf.Abs(v));
        if (pico < 0.0001f) return;

        float fator = 0.85f / pico;
        for (int i = 0; i < dados.Length; i++) dados[i] *= fator;
    }
}
