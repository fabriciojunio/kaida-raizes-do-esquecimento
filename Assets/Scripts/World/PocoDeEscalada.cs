using UnityEngine;

/// <summary>
/// Marca um poço vertical que só se vence com a escalada de parede: duas
/// paredes de frente uma para a outra e a saída lá em cima.
///
/// Existe por dois motivos. O primeiro é de projeto: sem um trecho que cobre a
/// habilidade, a escalada era um item que o jogador pegava e nunca usava,
/// porque dava para andar reto até a saída da região.
///
/// O segundo é de teste. O validador de alcance dos mapas só sabe pular e
/// cair; um poço de treze unidades passaria por "plataforma ilhada" e
/// reprovaria a região inteira. Em vez de afrouxar o limite de subida para
/// todo mundo - o que deixaria passar buraco de verdade em qualquer outro
/// mapa -, o poço fica declarado aqui, no lugar onde de fato existe.
/// </summary>
public class PocoDeEscalada : MonoBehaviour
{
    [Tooltip("Área do poço em unidades de mundo.")]
    public Rect area = new Rect(0f, 0f, 8f, 15f);

    public bool Contem(Vector2 ponto) => area.Contains(ponto);

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.45f, 0.85f, 1f, 0.85f);
        Gizmos.DrawWireCube(area.center, new Vector3(area.width, area.height, 0f));
    }
}
