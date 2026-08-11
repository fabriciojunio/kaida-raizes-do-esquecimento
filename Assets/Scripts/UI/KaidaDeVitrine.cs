using UnityEngine;

/// <summary>
/// A Kaida do fundo do menu: anda de um lado para o outro sozinha, para e
/// olha em volta, e volta a andar. Não tem física nem controle — é só
/// aparência, e por isso não usa o PlayerController.
/// </summary>
public class KaidaDeVitrine : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Tooltip("Até onde vai para cada lado, a partir de onde nasceu.")]
    public float alcance = 7f;
    public float velocidade = 2.2f;

    [Tooltip("Faixa de tempo parada entre uma caminhada e outra.")]
    public Vector2 tempoParada = new Vector2(1.4f, 3f);
    public Vector2 tempoAndando = new Vector2(2f, 4f);

    float origemX;
    int direcao = 1;
    float trocaEm;
    bool andando = true;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        origemX = transform.position.x;
        Agendar();
        Tocar();
    }

    void Update()
    {
        if (Time.time >= trocaEm)
        {
            andando = !andando;
            if (andando) direcao = -direcao;
            Agendar();
            Tocar();
        }

        if (!andando) return;

        transform.position += Vector3.right * direcao * velocidade * Time.deltaTime;

        // chegou na ponta do trecho: volta
        if (Mathf.Abs(transform.position.x - origemX) > alcance)
        {
            direcao = -direcao;
            var p = transform.position;
            p.x = origemX + Mathf.Sign(p.x - origemX) * alcance;
            transform.position = p;
            Aplicar();
        }
        Aplicar();
    }

    void Agendar()
    {
        var faixa = andando ? tempoAndando : tempoParada;
        trocaEm = Time.time + Random.Range(faixa.x, faixa.y);
    }

    void Tocar()
    {
        if (animator != null) animator.Play(andando ? "run" : "idle");
    }

    void Aplicar()
    {
        if (spriteRenderer != null) spriteRenderer.flipX = direcao < 0;
    }
}
