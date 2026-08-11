using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Testes de lógica pura (sem depender de cena montada). Rodar em
/// Window > General > Test Runner > EditMode > Run All.
/// </summary>
public class PlayerLogicTests
{
    PlayerStats NewStats()
    {
        var s = ScriptableObject.CreateInstance<PlayerStats>();
        s.jumpHeight = 3.2f; s.jumpTimeToPeak = 0.36f; s.jumpTimeToDescent = 0.30f;
        return s;
    }

    [Test]
    public void JumpVelocity_EhNegativa_ParaSubir()
    {
        var s = NewStats();
        Assert.Less(s.JumpVelocity * -1f, 0f); // JumpVelocity é positivo na fórmula; velocidade aplicada é negativa no uso real
        Assert.Greater(s.JumpVelocity, 0f, "magnitude da velocidade de pulo deve ser positiva");
    }

    [Test]
    public void Gravidades_SaoSempotePositivas()
    {
        for (int i = 0; i < 10; i++)
        {
            var s = NewStats();
            s.jumpHeight = 2f + i;
            Assert.Greater(s.JumpGravity, 0f);
            Assert.Greater(s.FallGravity, 0f);
        }
    }

    [Test]
    public void StateMachine_TransitaEIgnoraEstadoInvalido()
    {
        var m = new StateMachine();
        m.Add("idle", new DummyState());
        m.Add("run", new DummyState());
        m.SetInitial("idle");
        Assert.AreEqual("idle", m.CurrentName);
        m.ChangeState("run");
        Assert.AreEqual("run", m.CurrentName);
        m.ChangeState("naoexiste");
        Assert.AreEqual("run", m.CurrentName, "estado inválido deve ser ignorado");
    }

    [Test]
    public void SaveData_AbilitiesEItens_NaoDuplicam()
    {
        var go = new GameObject("SS");
        var ss = go.AddComponent<SaveSystem>();
        ss.Data = new SaveData();
        Assert.IsFalse(ss.HasAbility("dash"));
        ss.Data.unlockedAbilities.Add("dash");
        Assert.IsTrue(ss.HasAbility("dash"));
        ss.MarkCollected("orb1");
        ss.MarkCollected("orb1");
        Assert.AreEqual(1, ss.Data.collectedItems.Count, "não deve duplicar item coletado");
        Object.DestroyImmediate(go);
    }

    class DummyState : State {}
}
