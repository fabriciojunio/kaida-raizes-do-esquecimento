using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ponto de chegada numa sala. As transições procuram por id, então o mesmo
/// mapa pode ser percorrido nos dois sentidos sem duplicar cena.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    public string id = "default";

    static readonly List<SpawnPoint> ativos = new List<SpawnPoint>();

    void OnEnable()  { if (!ativos.Contains(this)) ativos.Add(this); }
    void OnDisable() { ativos.Remove(this); }

    public static SpawnPoint Find(string id)
    {
        // a lista estática pode conter restos de uma cena anterior
        ativos.RemoveAll(s => s == null);
        var achado = ativos.Find(s => s.id == id);
        if (achado != null) return achado;

        foreach (var s in Object.FindObjectsOfType<SpawnPoint>())
            if (s.id == id) return s;
        return null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.4f, 1f, 0.6f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.35f);
    }
}
