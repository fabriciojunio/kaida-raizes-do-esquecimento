using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Lógica pura, sem cena nem física. Roda em EditMode.
/// </summary>
public class PlayerLogicTests
{
    PlayerStats NovoStats()
    {
        var s = ScriptableObject.CreateInstance<PlayerStats>();
        s.jumpHeight = 4f;
        s.jumpTimeToPeak = 0.38f;
        s.jumpTimeToDescent = 0.32f;
        return s;
    }

    // ------------------------------------------------------------ gravidade
    [Test]
    public void VelocidadeDePulo_ChegaNaAlturaPedida()
    {
        var s = NovoStats();
        // com aceleração constante, a altura do topo é v² / (2g)
        float alturaAlcancada = (s.JumpVelocity * s.JumpVelocity) / (2f * s.JumpGravity);
        Assert.AreEqual(s.jumpHeight, alturaAlcancada, 0.01f,
            "a fórmula de pulo deve entregar exatamente a altura configurada");
    }

    [Test]
    public void QuedaEhMaisRapidaQueSubida_QuandoDescidaEhMaisCurta()
    {
        var s = NovoStats();   // descida 0,32s contra subida 0,38s
        Assert.Greater(s.FallGravity, s.JumpGravity,
            "descida mais curta que a subida exige gravidade de queda maior");
    }

    [Test]
    public void Gravidades_SaoPositivas_ParaQualquerAltura()
    {
        for (int i = 0; i < 10; i++)
        {
            var s = NovoStats();
            s.jumpHeight = 2f + i;
            Assert.Greater(s.JumpGravity, 0f);
            Assert.Greater(s.FallGravity, 0f);
            Assert.Greater(s.JumpVelocity, 0f);
        }
    }

    // -------------------------------------------------------- máquina de estados
    [Test]
    public void MaquinaDeEstados_TransitaEIgnoraNomeInvalido()
    {
        var m = new StateMachine();
        m.Add("idle", new EstadoEspiao());
        m.Add("run", new EstadoEspiao());

        m.SetInitial("idle");
        Assert.AreEqual("idle", m.CurrentName);

        m.ChangeState("run");
        Assert.AreEqual("run", m.CurrentName);

        m.ChangeState("naoexiste");
        Assert.AreEqual("run", m.CurrentName, "estado inválido deve ser ignorado");
    }

    [Test]
    public void MaquinaDeEstados_ChamaSaidaEEntradaNaOrdemCerta()
    {
        var origem = new EstadoEspiao();
        var destino = new EstadoEspiao();
        var m = new StateMachine();
        m.Add("a", origem);
        m.Add("b", destino);

        m.SetInitial("a");
        Assert.AreEqual(1, origem.entradas);

        m.ChangeState("b");
        Assert.AreEqual(1, origem.saidas, "o estado anterior precisa receber Exit");
        Assert.AreEqual(1, destino.entradas, "o novo estado precisa receber Enter");
    }

    [Test]
    public void MaquinaDeEstados_TrocarParaOMesmoEstado_NaoReinicia()
    {
        var estado = new EstadoEspiao();
        var m = new StateMachine();
        m.Add("a", estado);
        m.SetInitial("a");

        m.ChangeState("a");
        Assert.AreEqual(1, estado.entradas, "trocar para o estado atual não deve reentrar");
        Assert.AreEqual(0, estado.saidas);
    }

    [Test]
    public void MaquinaDeEstados_NomeEhIndiferenteAMaiusculas()
    {
        var m = new StateMachine();
        m.Add("Idle", new EstadoEspiao());
        m.SetInitial("IDLE");
        Assert.AreEqual("idle", m.CurrentName);
    }

    // -------------------------------------------------------------- save
    [Test]
    public void Save_HabilidadesEItens_NaoDuplicam()
    {
        var go = new GameObject("SaveTeste");
        var ss = go.AddComponent<SaveSystem>();
        ss.Data = new SaveData();

        Assert.IsFalse(ss.HasAbility("double_jump"));
        ss.Data.unlockedAbilities.Add("double_jump");
        Assert.IsTrue(ss.HasAbility("double_jump"));

        ss.MarkCollected("frag_01");
        ss.MarkCollected("frag_01");
        Assert.AreEqual(1, ss.Data.collectedItems.Count, "item coletado não pode duplicar");

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Save_GravaERecarregaHabilidades()
    {
        var go = new GameObject("SaveTeste");
        var ss = go.AddComponent<SaveSystem>();
        ss.DeleteSave();

        ss.UnlockAbility("wall_climb");     // UnlockAbility já grava em disco
        ss.Data = new SaveData();           // esquece tudo em memória
        Assert.IsFalse(ss.HasAbility("wall_climb"));

        Assert.IsTrue(ss.LoadGame(), "deveria existir um save gravado");
        Assert.IsTrue(ss.HasAbility("wall_climb"), "a habilidade precisa sobreviver ao ciclo salvar/carregar");

        ss.DeleteSave();
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Save_SemArquivo_NaoQuebra()
    {
        var go = new GameObject("SaveTeste");
        var ss = go.AddComponent<SaveSystem>();
        ss.DeleteSave();

        Assert.IsFalse(ss.HasSave());
        Assert.IsFalse(ss.LoadGame(), "carregar sem arquivo deve retornar falso, não estourar");

        Object.DestroyImmediate(go);
    }

    class EstadoEspiao : State
    {
        public int entradas, saidas;
        public override void Enter() { entradas++; }
        public override void Exit() { saidas++; }
    }
}
