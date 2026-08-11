# Estado do projeto — Kaida: Raízes do Esquecimento

Documento de retomada: o que existe, o que foi verificado, o que falta.
Leia junto com `GDD_HISTORIA_E_DESIGN.md` (a visão criativa).

## O que mudou desde o scaffold inicial

A primeira versão deste projeto era só código, sem cenas, escrito fora da
Unity. Agora existe o jogo montado por completo, mais um pipeline que
regenera tudo a partir do código.

**Verificado:** todos os 53 arquivos `.cs` compilam contra as DLLs reais da
Unity 2022.3.62f3 (não contra stubs) — gameplay, scripts de editor e os dois
assemblies de teste, sem erros e sem avisos.

**Não verificado ainda:** nada foi executado dentro da Unity, porque a
máquina não tem licença ativada. Ver "Pendência" no fim.

## Bugs corrigidos no scaffold original

| Onde | Problema |
|------|----------|
| `PlayerController.StartInvulnWindow` | Nunca ligava `isInvulnerable`. A janela de recuperação depois de apanhar não existia na prática. |
| `PlayerHurtState` | Chamava `ChangeState` antes de abrir a janela de invulnerabilidade, deixando uma brecha de um frame. |
| `GameManager.RespawnPlayer` | Não zerava a velocidade (reaparecia caindo) nem avisava a HUD (barra de vida ficava zerada depois de morrer). |
| `SaveSystem.LoadGame` | Acessava `GameManager.Instance` sem checar nulo e estourava com save corrompido. |
| `EnemyController.Patrol` | Comparava `Vector3` com `==`, e ignorava `groundCheck`/`wallCheck` — os inimigos andavam para fora das plataformas. |
| `EnemyController.FindPlayer` | Detectava o jogador através de paredes. |
| `KaidaBuild` (novo) | `MenuItem(..., priority = 0)` não compila; a propriedade nomeada não existe. |
| `SpriteSheetSetup` (novo) | `TextureImporter.spritesheet` foi **removido** na 2022.3: compila, mas não fatia nada. Migrado para `ISpriteEditorDataProvider`. |

## O que existe agora

### Gameplay
- **Jogador**: 9 estados (idle, run, jump, fall, dash, attack, hurt, dead,
  wallcling), com coyote time, jump buffer, pulo variável, pulo duplo e
  salto de parede.
- **Inimigos**: base + três comportamentos distintos — javali (investida com
  telegrafia), abelha (voo senoidal e mergulho), caracol (se fecha na casca
  e fica imune).
- **Chefe**: Guardião do Lúmen com três fases numa máquina de estados
  própria, mais o projétil (`LumenBeam`) e a barra de vida.
- **Mundo**: checkpoints, transições entre cenas com ponto de chegada,
  perigos, habilidades coletáveis, fragmentos de lore, nódulos de vida,
  parallax.
- **Sistemas**: save em JSON, gerenciador de cena, HUD que se monta sozinha.

### Pipeline (`Assets/Editor/`)
Menu **Kaida → MONTAR TUDO** roda cinco etapas em ordem:

1. `SpriteSheetSetup` — fatia as folhas. Descobre o tamanho de frame
   contando ilhas de pixels (as folhas variam de 48 a 96 px de largura) e
   calcula o pivô por folha para os pés ficarem alinhados entre animações.
   Sem isso a Kaida afunda uma unidade ao pular.
2. `AnimationBuilder` — gera os clipes e os Animator Controllers.
3. `PrefabBuilder` — gera o `PlayerStats`, a Kaida, os inimigos, o chefe e
   os objetos de mundo.
4. `TileSetup` — recorta os tiles do tileset.
5. `SceneBuilder` — monta as quatro cenas a partir de mapas em texto.

### Testes (69 casos)
- **EditMode**: fórmulas de pulo, máquina de estados, ciclo de save; e
  integridade — se cada `PlayAnim("x")` tem estado correspondente no
  Animator, se os prefabs têm tudo ligado, se as cenas têm chão e câmera,
  se as transições apontam para cenas existentes.
- **PlayMode**: com física rodando — queda e pouso, altura do pulo, alcance
  do dash, dash barrado por parede, dano e knockback, invulnerabilidade,
  morte e respawn, ataque, inimigo que não cai da plataforma, caracol imune
  na casca, coletáveis, perigos, as três fases do chefe.

## Pendência: licença da Unity

O Unity 2022.3.62f3 está instalado, mas sem licença ativada
(`No ULF license found`). Sem ela não dá para compilar dentro da Unity,
montar as cenas nem rodar os testes.

**Como resolver (grátis, ~1 minuto):** abrir o Unity Hub → ícone de conta →
entrar com uma conta Unity → Preferences → Licenses → Add → *Get a free
personal license*.

Depois disso, na pasta do projeto:

```powershell
.\montar.ps1 -Build
```

Isso monta o jogo, roda os 69 testes, imprime o resumo e gera
`Build\Kaida.exe`.

## O que dá para fazer depois

- Áudio: não há nenhum som no projeto (nem música, nem efeitos).
- Menu inicial e tela de pausa.
- Trocar `Input.GetAxisRaw` pelo novo Input System, se quiser suporte a
  controle.
- Mais salas por região: o formato de mapa em texto no `SceneBuilder`
  torna isso barato.
- Trocar a personagem, se conseguir o pacote completo daquela do
  `sample(idle&walk)` — só o Animator do prefab muda.

## Convenções mantidas

- Uma mecânica nova de jogador vira um `State` novo, não um `if` dentro de
  um estado existente.
- Valores de balanceamento ficam no `PlayerStats`, nunca no meio da lógica.
- Comentários em português; nomes técnicos em inglês.
- Comentário explica *por quê*, não *o quê*.
