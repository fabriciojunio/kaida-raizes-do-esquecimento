# Kaida — Raízes do Esquecimento

Metroidvania 2D feito em Unity 2022.3. Kaida acorda na orla de um vale que
esqueceu de si mesmo e vai atrás do que restou da própria memória.

## Jogar

Se você só quer jogar, rode `Build\Kaida.exe`.

## Abrir no editor

1. Abra o projeto no Unity **2022.3.62f3** (Unity Hub → Add → aponte para
   esta pasta).
2. Na barra de menus aparece um menu **Kaida**. Clique em **Kaida → MONTAR
   TUDO**. Isso fatia os sprites, gera as animações, os prefabs, os tiles e
   as seis cenas.
3. Abra `Assets/Scenes/00_MenuPrincipal.unity` e aperte Play.

Pela linha de comando dá para fazer tudo de uma vez, incluindo os testes:

```powershell
.\montar.ps1            # monta e roda os testes
.\montar.ps1 -Build     # monta, testa e gera Build\Kaida.exe
.\montar.ps1 -SoTestes  # só os testes
```

## Controles

| Ação | Tecla |
|------|-------|
| Andar | setas ou A / D |
| Pular | espaço |
| Atacar | Ctrl esquerdo ou botão esquerdo do mouse |
| Dash | Shift esquerdo |
| Pausar | Esc |

Pulo duplo e escalada de parede só funcionam depois de encontrar a
habilidade no mapa — é um metroidvania.

## Telas

- **Menu principal**: novo jogo, continuar, dificuldade, controles, sair.
  O botão *Continuar* fica desligado enquanto não existe um save.
- **Dificuldade**: Fácil, Normal e Difícil. Muda a vida máxima da Kaida, o
  tamanho da janela de invulnerabilidade e a velocidade e o alcance de
  visão dos inimigos.
- **Pausa** (Esc): continuar, reiniciar a região, voltar ao menu, sair.
- **Morte**: volta ao último marco de descanso ou ao menu.
- **Vitória**: aparece ao derrotar o Guardião, com o balanço de fragmentos,
  nódulos e habilidades encontrados.

## O mapa

```
Orla da Vila → Floresta Silente → Lago Silente → Caverna Musgosa → Santuário
 (tutorial)      (pulo duplo)      (travessia)    (escalada parede)   (chefe)
```

As passagens funcionam nos dois sentidos: dá para voltar e alcançar o que
antes estava fora de alcance.

## Estrutura

```
Assets/
  Scripts/
    Player/       PlayerController + um arquivo por estado (idle, run, jump,
                  fall, dash, attack, hurt, dead, wallcling)
    Enemies/      EnemyController base + javali, abelha, caracol
    Enemies/Boss/ O Guardião do Lúmen e suas três fases
    Systems/      máquina de estados, save, checkpoint, troca de cena
    World/        coletáveis, perigos, transições, parallax
    UI/           vida, mensagens, barra do chefe
  Editor/         os geradores (sprites, animações, prefabs, tiles, cenas)
  Tests/
    EditMode/     lógica pura e integridade do projeto
    PlayMode/     comportamento com física rodando
  Art/            os sprites já organizados por personagem/região
docs/             GDD, brief e guias
```

### Sobre a pasta `Editor/`

As cenas não foram montadas à mão: são geradas por código a partir de mapas
em texto dentro de `Assets/Editor/SceneBuilder.cs`. Cada caractere é um tile.

```
"....P....C..........B...................B.....................>.",
"#######################....#####################################",
```

Editar o mapa e rodar **Kaida → MONTAR TUDO** de novo regenera a região
inteira. É bem mais rápido do que arrastar objetos na tela, e o level design
fica versionado como texto em vez de binário.

## Testes

138 casos, todos passando.

```
EditMode   75/75
PlayMode   63/63
```

**EditMode** — lógica sem cena, e verificação de que o projeto gerado bate
com o que o código espera. O caso mais útil: conferir que todo nome passado
para `PlayAnim("...")` existe mesmo no Animator. A Unity não reclama quando
falta um estado, a animação só não toca.

**PlayMode** — comportamento real, com a física rodando: a queda, a altura
do pulo, o alcance do dash, o knockback, o respawn, o inimigo que não anda
para fora da plataforma, as três fases do chefe, a dificuldade e a pausa.

Boa parte dos testes nasceu de defeito encontrado jogando, e ficou para
não deixar voltar:

- **alcance dos mapas**: um validador percorre as superfícies a partir do
  ponto de partida e reprova a região se alguma plataforma, item ou
  passagem ficar ilhado. Ele reprovou as cinco regiões da primeira versão
- **chão com colisão**: carrega cada região, solta a Kaida e mede se ela
  cai. Chão sem colisão não aparece em captura nenhuma — o cenário fica
  perfeito e o jogador atravessa o mundo
- **confronto com o chefe**: na cena real, não com um chefe montado à mão.
  O simplificado passava enquanto o do jogo era intocável
- **pivô das animações**: confere que os pés ficam alinhados entre idle,
  corrida e ataque, senão a personagem sobe meia unidade ao bater

```powershell
.\montar.ps1 -SoTestes
```

Ou pelo editor: **Window → General → Test Runner**.

## Requisitos

- Unity **2022.3.62f3** com suporte 2D
- Pacotes (já em `Packages/manifest.json`): 2D Tilemap, 2D Sprite,
  Test Framework, uGUI

## Documentação

- `docs/GDD_HISTORIA_E_DESIGN.md` — história, mundo, inimigos, progressão
- `docs/ESTADO_DO_PROJETO.md` — o que existe e o que ainda dá para fazer
- `CREDITOS.md` — de onde vem cada asset e sob qual licença

## Trilha sonora

Os pacotes de arte não trazem áudio, então a música é **gerada por síntese**
em tempo de execução (`Assets/Scripts/Systems/TrilhaSonora.cs`): escala
menor, arpejo lento e um baixo sustentado embaixo. Cada região tem tônica e
andamento próprios — a Vila mais clara, o Santuário grave e arrastado.

O volume é baixo de propósito, e dá para ajustar em **Tela e som**, no menu.

## Em qualquer tela

O jogo abre assumindo a resolução do monitor onde estiver rodando. A câmera
calcula a margem pela proporção real da tela, então nem um ultrawide 21:9
nem um projetor 4:3 mostram para fora do cenário; a interface acompanha a
altura, e nada é cortado.

## Créditos de arte

A arte vem do pacote **Legacy Fantasy — High Forest**, de **Anokolisa**
(gratuito, uso comercial permitido). Detalhes em `CREDITOS.md`.
