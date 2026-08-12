using UnityEngine;

/// <summary>
/// Habilidade largada no mundo (Pulo Duplo no fim da Floresta, Escalada de
/// Parede no fim da Caverna). Some para sempre depois de pego - o save guarda
/// tanto a habilidade quanto o item, para não reaparecer ao voltar na sala.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PickupAbility : MonoBehaviour
{
    [Tooltip("Identificador usado pelo SaveSystem: double_jump, wall_climb...")]
    public string abilityId = "double_jump";

    [Tooltip("Texto mostrado ao pegar.")]
    [TextArea] public string mensagem = "Pulo Duplo\nO ar segura você por um instante a mais.";

    public float bobHeight = 0.15f;
    public float bobSpeed = 2f;

    Vector3 basePos;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        basePos = transform.position;
    }

    void Start()
    {
        // já pegou numa sessão anterior: não renasce
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasAbility(abilityId))
            gameObject.SetActive(false);
    }

    void Update()
    {
        transform.position = basePos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;

        if (SaveSystem.Instance != null) SaveSystem.Instance.UnlockAbility(abilityId);
        p.RefreshAirAbilities();   // libera o pulo duplo na hora, sem precisar tocar o chão
        MessageUI.Show(mensagem);
        gameObject.SetActive(false);
    }
}
