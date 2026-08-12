using UnityEngine;

/// <summary>
/// Conta quantas telas que exigem clique estão abertas (pausa, morte,
/// vitória).
///
/// O cursor precisa aparecer sempre que houver uma delas na frente, e sumir
/// assim que todas fecharem. Antes disso, só a pausa era considerada — e o
/// ponteiro voltava a sumir bem na hora de clicar em "voltar ao marco" na
/// tela de morte.
/// </summary>
public static class TelaModal
{
    static int abertas;

    public static bool AlgumaAberta => abertas > 0;

    public static void Abriu() => abertas++;

    public static void Fechou() => abertas = Mathf.Max(0, abertas - 1);

    /// <summary>Zera ao trocar de cena, senão a contagem vaza entre regiões.</summary>
    public static void Zerar() => abertas = 0;
}
