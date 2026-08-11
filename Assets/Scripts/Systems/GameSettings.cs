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
/// de progresso — a dificuldade é escolhida na tela inicial, antes de o
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
                return "Pouca vida, recuperação curta, inimigos mais rápidos.\nCada erro custa caro.";
            default:
                return "O jogo como foi balanceado.";
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

        switch (Atual)
        {
            case Dificuldade.Facil:
                stats.maxHealth = 7;
                stats.invulnTime = 1.5f;
                stats.dashCooldown *= 0.8f;
                break;

            case Dificuldade.Dificil:
                stats.maxHealth = 3;
                stats.invulnTime = 0.65f;
                stats.dashCooldown *= 1.2f;
                break;

            default:
                stats.maxHealth = 5;
                stats.invulnTime = 1.0f;
                break;
        }
    }

    /// <summary>Multiplicador de velocidade dos inimigos por dificuldade.</summary>
    public static float VelocidadeDosInimigos()
    {
        switch (Atual)
        {
            case Dificuldade.Facil:   return 0.85f;
            case Dificuldade.Dificil: return 1.25f;
            default:                  return 1f;
        }
    }

    /// <summary>No difícil os inimigos enxergam mais longe.</summary>
    public static float AlcanceDeVisao()
    {
        switch (Atual)
        {
            case Dificuldade.Facil:   return 0.8f;
            case Dificuldade.Dificil: return 1.3f;
            default:                  return 1f;
        }
    }
}
