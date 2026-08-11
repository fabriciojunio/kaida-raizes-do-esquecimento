# Brief para o Claude Code — Projeto "Kaida — Raízes do Esquecimento"

Este documento é o ponto de partida para você (Claude Code) continuar este
projeto. Leia também `GDD_HISTORIA_E_DESIGN.md` (a visão criativa completa)
e `MCP_SETUP.md` (se o usuário já configurou o MCP da Unity).

## Contexto importante

Este projeto foi **estruturado e escrito fora da Unity** (por outra
instância do Claude, num ambiente sem acesso ao editor). Todos os scripts
C# foram **compilados e checados sintaticamente** contra stubs da API
Unity — ou seja, a sintaxe e os tipos estão corretos — **mas NADA foi
aberto, testado ou jogado dentro da Unity de verdade**. Trate isso como um
scaffold sólido, não como um jogo pronto. Sua primeira tarefa é abrir o
projeto na Unity e ver o que quebra.

## O que já existe (não recrie do zero)

```
Assets/Scripts/
  Player/
    PlayerStats.cs          — ScriptableObject com toda a config de movimento/combate
    PlayerController.cs     — corpo do jogador (Rigidbody2D), máquina de estados
    States/
      PlayerIdleState.cs, PlayerRunState.cs, PlayerJumpState.cs,
      PlayerFallState.cs, PlayerDashState.cs, PlayerAttackState.cs,
      PlayerHurtState.cs, PlayerDeadState.cs
  Enemies/
    EnemyController.cs      — inimigo base (patrulha/persegue/ataca por contato)
  Systems/
    State.cs, StateMachine.cs  — máquina de estados genérica reutilizável
    GameManager.cs           — singleton, checkpoint/respawn
    SaveSystem.cs             — save em JSON, habilidades e coletados
    CameraFollow2D.cs         — câmera 2D com smoothing
  UI/
    HealthUI.cs               — vida em pips, gerado por código
Assets/Tests/
  PlayerLogicTests.cs         — testes NUnit (lógica pura, sem cena)
Assets/Art/
  Player/HeroKnight/, Player/PlatformerCharacterPack/  — pastas vazias, aguardando import
  Environment/VillageProps/, Environment/MossyCavern/, Environment/ForestSidescroller/
docs/
  GDD_HISTORIA_E_DESIGN.md  — história, mundo, inimigos, progressão completos
  MCP_SETUP.md
  CLAUDE_CODE_BRIEF.md      — este arquivo
```

**Não existe nenhuma cena (`.unity`) ainda.** Essa é a primeira lacuna real.

## Tarefas, em ordem recomendada

### 1. Setup inicial (faça isso primeiro, sempre)
- [ ] Abrir o projeto na Unity 2022.3 LTS (ou mais recente com 2D URP/Built-in).
- [ ] Confirmar que os 15 scripts compilam sem erro no Console. Se algo
      quebrar, é porque meus stubs de checagem não cobriam 100% da API —
      corrija o script real (a lógica pretendida está nos comentários).
- [ ] Importar os pacotes 2D necessários se não vierem por padrão:
      2D Animation, 2D Tilemap Editor, 2D Sprite, Test Framework
      (já listados em `Packages/manifest.json`, mas confirme na Unity).

### 2. Importar os assets do usuário
Os links exatos estão em `Assets/Art/Player/README.md` e
`Assets/Art/Environment/README.md`. Passos gerais por pacote:
- [ ] Importar o `.unitypackage` (Asset Store) ou os arquivos soltos (itch.io)
      na subpasta correspondente.
- [ ] Para o personagem (Hero Knight / Platformer Character Pack): abrir o
      Sprite Editor, confirmar o fatiamento (slicing) das folhas de sprite,
      e ver quais animações já vêm prontas (geralmente idle/run/jump/fall/
      attack/hurt/death — os nomes exatos variam por asset).
- [ ] Criar um **Animator Controller** para o jogador com esses clipes.
      Os nomes dos estados do Animator devem bater com o que
      `PlayerController.PlayAnim(string)` chama: `"idle"`, `"run"`,
      `"jump"`, `"fall"`, `"dash"`, `"attack"`, `"hurt"`, `"death"`.
      Se o asset usar nomes diferentes, ajuste as chamadas em
      `Assets/Scripts/Player/States/*.cs` (é um find-replace simples) —
      **não** mude a arquitetura para isso.
- [ ] Para o ambiente (Village Props / Mossy Cavern / Forest Sidescroller):
      criar um `Tile Palette` + `Tilemap` por região, usando os tiles do
      pacote. Adicionar `Tilemap Collider 2D` + `Composite Collider 2D` na
      camada `Ground` para o chão/paredes.

### 3. Montar o Player prefab
- [ ] Criar um GameObject "Player" com: `Rigidbody2D` (gravityScale=0,
      freezeRotation=true — o script controla a gravidade manualmente),
      um `Collider2D` (CapsuleCollider2D funciona bem), `SpriteRenderer`,
      `Animator`, e o script `PlayerController`.
- [ ] Criar dois filhos vazios: `GroundCheck` (nos pés) e `AttackPoint`
      (à frente do personagem) e arrastar para os campos correspondentes
      no Inspector do `PlayerController`.
- [ ] Criar/atribuir o asset `PlayerStats` (botão direito no Project >
      Create > Metroidvania > Player Stats).
- [ ] Configurar as camadas de física: `Player`, `Enemy`, `Ground`,
      `PlayerHitbox`, `EnemyHitbox` (já pré-cadastradas em
      `ProjectSettings/TagManager.asset` — confirme no Project Settings
      > Tags and Layers).
- [ ] Configurar o Input Manager (Edit > Project Settings > Input Manager)
      com os eixos/botões usados no código: `Horizontal` (já existe por
      padrão), `Jump` (Space), `Fire1` (já existe, botão de ataque),
      e a tecla de dash está direto no código como `KeyCode.LeftShift`
      (pode migrar para o novo Input System depois, não é prioridade).

### 4. Montar as cenas
Seguindo `docs/GDD_HISTORIA_E_DESIGN.md`, seção 3 (regiões do mundo):
- [ ] Cena `Scenes/01_OrlaDaVila.unity` — tutorial/hub.
- [ ] Cena `Scenes/02_FlorestaSilente.unity`
- [ ] Cena `Scenes/03_CavernaMusgosa.unity`
- [ ] Cena `Scenes/04_SantuarioEsquecido.unity` — chefe final.
- [ ] Cada cena precisa: Tilemap(s) montado com o tileset certo, o Player
      prefab posicionado num spawn point, uma instância do `GameManager`
      (ou um autoload — considere um GameObject `_Systems` com
      `GameManager` + `SaveSystem`, marcado `DontDestroyOnLoad`, presente
      só na primeira cena carregada), e uma Main Camera com
      `CameraFollow2D` mirando o Player.
- [ ] Transições entre cenas: crie um script simples `RoomTransition.cs`
      (trigger de borda que chama `SceneManager.LoadScene`) — ainda não
      existe, é uma tarefa nova.

### 5. Inimigos por região (ver GDD seção 3 para o design de cada um)
`EnemyController.cs` é a base (patrulha, persegue, ataca por contato).
Para os inimigos com padrões diferentes (arqueiro à distância, morcego
voador, rastejante que emerge do chão), crie **subclasses ou variantes**
via composição (novo script que herda de `EnemyController` e sobrescreve
o necessário, ou um componente adicional tipo `RangedAttackModule`).
Não reescreva a base — estenda.

### 6. Habilidades desbloqueáveis
`SaveSystem.cs` já tem `UnlockAbility`/`HasAbility`. Faltam:
- [ ] Estado `PlayerDoubleJumpState` (ou modificar `PlayerJumpState` para
      checar `SaveSystem.Instance.HasAbility("double_jump")` e permitir
      um segundo pulo no ar).
- [ ] Estado/modificação para escalada de parede (`wall_climb`) —
      detectar parede com um `wallCheck` (adicionar em `PlayerController`),
      novo estado `PlayerWallClingState`.
- [ ] Objetos coletáveis de habilidade no mundo (um `PickupAbility.cs`
      simples: trigger que chama `SaveSystem.Instance.UnlockAbility(id)`).

### 7. Chefe final
O Guardião do Lúmen (GDD seção 3.4) — três fases, cada uma testando uma
habilidade diferente. Recomendo implementar como uma máquina de estados
própria (reaproveite `State`/`StateMachine`), com uma fase por vez.

### 8. Testes
- [ ] Rodar `Assets/Tests/PlayerLogicTests.cs` no Test Runner
      (Window > General > Test Runner > EditMode > Run All) — devem
      passar (são testes de lógica pura, sem depender de cena).
- [ ] Adicionar testes de **PlayMode** (que dependem de cena/física) à
      medida que as cenas forem montadas — esses eu não pude escrever
      sem uma cena real para referenciar.

## Convenções de código a manter
- Português nos comentários e nomes de conceitos de design (histórias,
  GDD); inglês ou português consistente no código é aceitável — o
  scaffold atual está em inglês nos nomes técnicos, português nos
  comentários. Mantenha esse padrão.
- Um estado por arquivo, um responsável por classe.
- Toda nova mecânica de jogador vira um novo `State`, não um `if` dentro
  de um estado existente.
- Valores de balanceamento (velocidade, dano, tempos) vão em
  `PlayerStats` ou similar ScriptableObject — nunca hardcoded dentro da
  lógica.

## Se algo no scaffold estiver errado
É esperado que existam pequenos erros — nunca rodei isso na Unity de
verdade. Prioridade ao corrigir: **preserve a arquitetura** (máquina de
estados, ScriptableObject de stats, singletons de sistema) mesmo que
precise corrigir detalhes de implementação. Se um erro for grande o
suficiente para sugerir repensar uma parte, pare e pergunte ao usuário
antes de refatorar largamente.
