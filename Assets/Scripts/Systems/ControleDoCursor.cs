using UnityEngine;

/// <summary>
/// Some com o cursor durante o jogo e devolve nos menus.
///
/// O ataque é no botão do mouse, então o ponteiro fica parado no meio da
/// tela atrapalhando a leitura da cena. Nos menus ele volta, senão não há
/// como clicar em nada.
/// </summary>
public class ControleDoCursor : MonoBehaviour
{
    [Tooltip("Ligado nas cenas de menu, desligado nas regiões jogáveis.")]
    public bool mostrarCursor = false;

    void OnEnable() => Aplicar();

    void Update()
    {
        // qualquer tela que exija clique devolve o ponteiro: pausa, morte
        // ou vitória. Antes só a pausa contava, e o cursor sumia justamente
        // na hora de escolher o que fazer depois de morrer.
        bool precisaDoCursor = mostrarCursor || TelaModal.AlgumaAberta;

        if (Cursor.visible != precisaDoCursor) Aplicar(precisaDoCursor);
    }

    void OnDisable()
    {
        // nunca deixar o jogador sem cursor ao sair
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void OnApplicationFocus(bool temFoco)
    {
        if (!temFoco) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }
        else Aplicar();
    }

    void Aplicar() => Aplicar(mostrarCursor || TelaModal.AlgumaAberta);

    void Aplicar(bool visivel)
    {
        Cursor.visible = visivel;
        // Confined em vez de Locked: o jogo é 2D e não usa o movimento do
        // mouse, mas prender o ponteiro na janela evita clicar fora sem querer
        // em quem joga com dois monitores.
        Cursor.lockState = visivel ? CursorLockMode.None : CursorLockMode.Confined;
    }
}
