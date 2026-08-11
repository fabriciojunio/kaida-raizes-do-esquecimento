# KAIDA — Raízes do Esquecimento
### Game Design Document — Metroidvania 2D

---

## 1. Visão Geral

Um metroidvania 2D de ação e exploração, inspirado estilisticamente em jogos
como *Hollow Knight* (atmosfera, progressão por habilidades, mapa
interconectado) e com movimento ágil e preciso inspirado em jogos de
plataforma/parkour como a série *Prince of Persia* (não reutilizamos
personagens, história ou qualquer conteúdo da Ubisoft — a inspiração é
puramente na *sensação* do movimento: fluidez, precisão, quedas calculadas).

Protagonista original, criada para este jogo. O visual final veio do pacote
**Legacy-Fantasy High Forest**, escolhido por ser o único com o conjunto
completo de animações que um metroidvania de ação exige (idle, corrida,
pulo em três partes, ataque e morte) e por trazer, no mesmo pacote e na
mesma paleta, os inimigos, os tiles e o fundo. A coerência visual pesou
mais que a referência original de cor — um personagem com metade das
animações faltando quebraria o combate, que é o centro do jogo.

---

## 2. A História

### Premissa

Há muito tempo, o **Vale de Myrrhen** era próspero — vilas conectadas por
estradas de pedra, uma floresta sagrada que guardava a memória do povo, e
cavernas profundas onde os antigos mineravam um mineral chamado **lúmen**,
capaz de guardar lembranças dentro de si.

Um evento conhecido apenas como **o Esquecimento** varreu o vale. Ninguém
sabe exatamente o que aconteceu — só que, da noite para o dia, os moradores
pararam de se lembrar de quem eram. Alguns definharam. Outros se corromperam,
viraram cascas ocas movidas por instinto, vagando pelas ruínas das próprias
casas. A floresta escureceu. As cavernas murcharam, cobertas de musgo doente.

### A Protagonista: Kaida

**Kaida** acorda na orla da floresta sem lembranças de como chegou ali —
só uma espada nas costas, um casaco azul surrado, e a sensação de já ter
estado naquele vale antes. Ela não é do lugar, ou talvez seja — essa é
uma das perguntas que o jogo responde aos poucos, através de **Fragmentos
de Lúmen** (colecionáveis narrativos, opcionais, que revelam pedaços da
história de quem ela é e do que houve no vale).

Kaida segue adiante não porque sabe o que procura, mas porque **parar
também é uma forma de esquecer** — e algo nela se recusa a isso.

### O que está em jogo

No coração do vale, sob a Caverna Musgosa, dorme o **Guardião**: uma
entidade que antes protegia a memória coletiva do povo de Myrrhen através
do lúmen. Algo a corrompeu, e agora ela é a fonte do Esquecimento — consome
memórias das criaturas e do próprio vale para se manter "inteira", mas
está se perdendo no processo, e o que resta dela some cada vez mais rápido.

Kaida precisa alcançá-la — para detê-la, ou para se lembrar de si mesma
através dela. O jogo não entrega qual é o caso até o final.

### Tom

Melancólico, silencioso, com momentos de beleza (a floresta ainda tem
brotos de luz; a vila ainda tem ecos de quem já viveu ali) no meio da
decadência. Não é sombrio pelo sombrio — é sobre memória, perda e
persistência.

---

## 3. Estrutura do Mundo (Fases / Regiões)

O mundo é **interconectado** (metroidvania clássico) — não são fases
lineares separadas, mas regiões que se conectam, com atalhos que abrem
conforme Kaida ganha habilidades. Ordem de progressão pretendida:

### 3.1 — Orla da Vila (tutorial / hub inicial)
- **Asset:** Legacy-Fantasy High Forest (tiles com tint neutro)
- **Função:** ensina movimento, pulo, ataque e dash num espaço seguro.
  Funciona como hub — de lá dá para ver (mas não alcançar ainda) caminhos
  para a Floresta e a Caverna.
- **Inimigos:** Javalis-Casca (moradores corrompidos: recuam, bufam e
  disparam numa investida reta — ensinam o padrão de "observar o aviso,
  desviar, atacar na abertura").
- **Dificuldade:** baixa. Foco em ensinar sem dizer explicitamente.

### 3.2 — Floresta Silente
- **Asset:** Legacy-Fantasy High Forest (tint verde dessaturado)
- **Função:** primeira área "real". Plataforming horizontal, copas de
  árvores como plataformas, quedas que exigem o dash para não tomar dano
  de espinhos no chão.
- **Inimigos:** Javalis-Casca em maior número e mais espalhados, e o
  primeiro Caracol-Rastejante (se fecha na casca ao apanhar e fica imune —
  obriga o jogador a esperar a abertura em vez de socar o botão).
- **Habilidade obtida ao final:** **Pulo Duplo** (encontrado com um
  guardião menor da floresta, opcional mas recomendado antes da Caverna).
- **Dificuldade:** média. Introduz combate contra múltiplos inimigos.

### 3.3 — Lago Silente
- **Asset:** Legacy-Fantasy High Forest (tint claro, árvores douradas) com
  a camada de água do mesmo tileset.
- **Função:** respiro entre a Floresta e a Caverna. A travessia é por cima
  da água, saltando entre plataformas — é onde o Pulo Duplo recém-obtido
  deixa de ser opcional e vira ferramenta.
- **Água:** não afoga. Cair custa o caminho já andado, não a vida. A punição
  é de paciência, não de dano, porque a região vem logo depois da primeira
  habilidade e serve para o jogador se acostumar com ela.
- **Inimigos:** Abelhas-Eco sobre o lago, atacando de cima enquanto Kaida
  está no ar entre duas plataformas.
- **Dificuldade:** média. Introduz plataforma sobre vazio.

### 3.4 — Caverna Musgosa
- **Asset:** Legacy-Fantasy High Forest (tint azul-acinzentado) + fundo do
  Stringstar Fields
- **Função:** verticalidade — a caverna desce em camadas. Poços de esporos
  tóxicos (hazard ambiental), plataformas que desmoronam.
- **Inimigos:** Abelhas-Eco (voam em padrão senoidal e atacam em mergulho
  diagonal — obrigam a olhar para cima, não só para os lados) e
  Caracóis-Rastejantes em grupo.
- **Habilidade obtida ao final:** **Escalada de Parede** (wall cling +
  wall jump) — abre atalhos de volta para a Vila e a Floresta.
- **Dificuldade:** alta. Combina hazards ambientais com inimigos.

### 3.5 — Santuário Esquecido (fase final)
- **Asset:** os mesmos tiles das outras regiões com tint violeta — é a
  versão "corrompida" da Caverna, mesma paleta distorcida. Foi assim que
  ficou implementado: cada região usa o mesmo tileset com uma cor
  diferente no Tilemap, o que mantém a coerência visual e evita depender
  de um pacote de arte por região.
- **Função:** confronto final contra o Guardião.
- **Chefe:** **O Guardião do Lúmen** — três fases:
  1. Ataques de longo alcance (feixes de "memória" que Kaida precisa
     desviar/dashar através).
  2. Invoca ecos dos inimigos já enfrentados (versões com metade da vida
     dos Javalis, Abelhas e Caracóis) — testa tudo que o jogador aprendeu.
     Enquanto há ecos em campo ele fica fora de alcance.
  3. Fase final corpo a corpo, rápida, exige uso combinado de dash +
     pulo duplo + parede para não ser encurralado.

---

## 4. Habilidades e Progressão (a espinha dorsal do metroidvania)

| Habilidade       | Onde é obtida        | O que libera                              |
|-------------------|----------------------|--------------------------------------------|
| Movimento + Ataque + Dash | Início (Vila) | Já implementado na base técnica |
| Pulo Duplo        | Fim da Floresta       | Alcançar plataformas altas, atalhos na Vila |
| Escalada de Parede | Fim da Caverna        | Subir poços, atalhos de volta, área secreta |
| (Opcional) Upgrade de lâmina | Colecionável escondido | Aumenta dano de ataque |

Isso já é suportado pelo `SaveSystem.cs` da base técnica
(`UnlockAbility`/`HasAbility`) — as portas/bloqueios do mapa devem checar
essas flags antes de permitir passagem (ver tarefas no `ESTADO_DO_PROJETO.md`).

## 5. Colecionáveis

- **Fragmentos de Lúmen** (lore, opcionais): pequenos textos/memórias que
  montam a história de Kaida e do vale. Não afetam a jogabilidade.
- **Nódulos de Vida**: aumentam a vida máxima (4 no jogo — cada um +1 pip).

## 6. Curva de Dificuldade (resumo)

```
Vila (tutorial) → Floresta (combate + grupos) → Lago (plataforma sobre vazio) →
Caverna (hazards + verticalidade) → Santuário (chefe, 3 fases)
```
Cada região introduz UM conceito novo por vez (nunca dois ao mesmo tempo),
e o chefe final testa todos eles juntos — padrão clássico de design de
metroidvania.

## 7. Direção de Arte

- Paleta fria e um pouco dessaturada (musgo, pedra, azul-acinzentado),
  com pontos de luz quente (o lúmen, tochas, os olhos de Kaida) para guiar
  o olho do jogador e simbolizar "memória viva" em meio à decadência.
- Câmera relativamente próxima (estilo Hollow Knight), mundo legível
  mesmo com poucos pixels.
