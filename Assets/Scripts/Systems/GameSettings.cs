using UnityEngine;

public enum Dificuldade
{
    Facil = 0,
    Normal = 1,
    Dificil = 2
}

/// <summary>
/// Preferências que valem para a partida inteira e sobrevivem entre cenas.
/// Guardadas em PlayerPrefs porque precisam existir antes de qualquer save
/// de progresso - a dificuldade é escolhida na tela inicial, antes de o
/// jogador tocar no mundo.
/// </summary>
public static class GameSettings
{
    const string ChaveDificuldade = "kaida.dificuldade";
    const string ChaveVolume = "kaida.volume";

    /// <summary>
    /// Volume da trilha, de 0 a 1. Começa baixo: é som ambiente, feito para
    /// ficar embaixo do jogo, não à frente dele.
    /// </summary>
    public static float Volume
    {
        get => Mathf.Clamp01(PlayerPrefs.GetFloat(ChaveVolume, 0.6f));
        set
        {
            PlayerPrefs.SetFloat(ChaveVolume, Mathf.Clamp01(value));
            PlayerPrefs.Save();
            if (TrilhaSonora.Instance != null) TrilhaSonora.Instance.AplicarVolumeGeral();
        }
    }

    public static string NomeDoVolume()
    {
        int passos = Mathf.RoundToInt(Volume * 4f);
        switch (passos)
        {
            case 0: return "Sem som";
            case 1: return "Bem baixo";
            case 2: return "Baixo";
            case 3: return "Médio";
            default: return "Alto";
        }
    }

    public static Dificuldade Atual
    {
        get => (Dificuldade)PlayerPrefs.GetInt(ChaveDificuldade, (int)Dificuldade.Normal);
        set
        {
            PlayerPrefs.SetInt(ChaveDificuldade, (int)value);
            PlayerPrefs.Save();
        }
    }

    public static string Nome(Dificuldade d)
    {
        switch (d)
        {
            case Dificuldade.Facil:   return "Fácil";
            case Dificuldade.Dificil: return "Difícil";
            default:                  return "Normal";
        }
    }

    public static string Descricao(Dificuldade d)
    {
        switch (d)
        {
            case Dificuldade.Facil:
                return "Mais vida e uma janela de recuperação longa.\nPara conhecer o vale sem pressa.";
            case Dificuldade.Dificil:
                return "Pouca vida, recuperação curta, inimigos mais rápidos\ne o chefe com a vida cheia. Cada erro custa caro.";
            default:
                return "Vida folgada e boa janela de recuperação.\nA caminhada inteira sem cobrar demais.";
        }
    }

    /// <summary>
    /// Aplica a dificuldade sobre uma cópia dos stats do jogador.
    /// Recebe sempre um clone: mexer no asset original gravaria a alteração
    /// no disco e vazaria de uma partida para a outra.
    /// </summary>
    public static void Aplicar(PlayerStats stats)
    {
        if (stats == null) return;

        // Rebalanceado depois de jogar: o Normal estava cobrando como
        // dificuldade alta. Cinco de vida com um segundo de invulnerabilidade
        // não dá margem para aprender o padrão de um inimigo - o jogador leva
        // o segundo golpe antes de entender o primeiro. O que subiu mais foi a
        // janela de recuperação, que é o que dá tempo de sair de perto.
        switch (Atual)
        {
            case Dificuldade.Facil:
                stats.maxHealth = 9;
                stats.invulnTime = 1.9f;
                stats.dashCooldown *= 0.75f;
                break;

            case Dificuldade.Dificil:
                stats.maxHealth = 4;
                stats.invulnTime = 0.85f;
                stats.dashCooldown *= 1.15f;
                break;

            default:
                stats.maxHealth = 7;
                stats.invulnTime = 1.35f;
                break;
        }
    }

    /// <summary>Multiplicador de velocidade dos inimigos por dificuldade.</summary>
    public static float VelocidadeDosInimigos()
    {
        switch (Atual)
        {
            case Dificuldade.Facil:   return 0.75f;
            case Dificuldade.Dificil: return 1.2f;
            default:                  return 0.88f;
        }
    }

    /// <summary>No difícil os inimigos enxergam mais longe.</summary>
    public static float AlcanceDeVisao()
    {
        switch (Atual)
        {
            case Dificuldade.Facil:   return 0.7f;
            case Dificuldade.Dificil: return 1.3f;
            default:                  return 0.85f;
        }
    }

    /// <summary>
    /// Quanto da vida cheia o chefe usa.
    ///
    /// A luta final é longa por natureza: chegar até lá já custou a caminhada
    /// inteira, e um confronto que se arrasta transforma erro em recomeço caro.
    /// No Normal ele perde um quinto da vida.
    /// </summary>
    public static float VidaDoChefe()
    {
        switch (Atual)
        {
            case Dificuldade.Facil:   return 0.65f;
            case Dificuldade.Dificil: return 1f;
            default:                  return 0.8f;
        }
    }
}
