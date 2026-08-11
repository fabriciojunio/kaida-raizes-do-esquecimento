using UnityEngine;

/// <summary>
/// Recipiente para um sprite recortado de uma folha maior.
/// Um Sprite criado por código não vira arquivo sozinho; guardado aqui dentro
/// ele ganha um caminho próprio no projeto e pode ser referenciado pelos
/// geradores de prefab.
/// </summary>
public class SpriteRecortado : ScriptableObject
{
    public Sprite sprite;
}
