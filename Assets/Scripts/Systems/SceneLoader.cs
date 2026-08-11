using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Carrega salas e reposiciona o jogador no ponto de chegada certo.
/// Guarda o destino num campo estático porque a cena nova só existe depois
/// que o carregamento termina.
/// </summary>
public static class SceneLoader
{
    static string pendingSpawnId;
    static bool subscribed;

    public static void LoadRoom(string sceneName, string spawnId = "default")
    {
        pendingSpawnId = spawnId;
        if (!subscribed)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            subscribed = true;
        }
        SceneManager.LoadScene(sceneName);
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(pendingSpawnId)) return;

        var spawn = SpawnPoint.Find(pendingSpawnId);
        var player = Object.FindObjectOfType<PlayerController>();
        if (spawn != null && player != null)
        {
            player.transform.position = spawn.transform.position;
            player.SetVelocity(0f, 0f);
            if (GameManager.Instance != null)
                GameManager.Instance.SetCheckpoint(spawn.transform.position, scene.name);
        }
        pendingSpawnId = null;
    }
}
