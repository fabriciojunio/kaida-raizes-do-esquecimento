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

Protagonista original, criada para este jogo, com visual baseado na
referência que você escolheu (cabelo claro, casaco azul, botas vermelhas,
espada/katana) — os personagens e ambientes finais vêm dos asset packs que
você selecionou (Hero Knight, Platformer Character Pack, Village Props,
Mossy Cavern, Forest Sidescroller).

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
- **Asset:** Village Props (Unity Asset Store)
- **Função:** ensina movimento, pulo, ataque e dash num espaço seguro.
  Funciona como hub — de lá dá para ver (mas não alcançar ainda) caminhos
  para a Floresta e a Caverna.
- **Inimigos:** Cascas (moradores corrompidos, lentos, previsíveis —
  ensinam o padrão de "observar telegraph, atacar na abertura").
- **Dificuldade:** baixa. Foco em ensinar sem dizer explicitamente.

### 3.2 — Floresta Silente
- **Asset:** Sidescroller Pixelart Forest 16x16
- **Função:** primeira área "real". Plataforming horizontal, copas de
  árvores como plataformas, quedas que exigem o dash para não tomar dano
  de espinhos no chão.
- **Inimigos:** Lobos-Cinza (rápidos, avançam em grupo), Arqueiros-Musgo
  (atacam à distância, obrigam Kaida a fechar distância ou desviar).
- **Habilidade obtida ao final:** **Pulo Duplo** (encontrado com um
  guardião menor da floresta, opcional mas recomendado antes da Caverna).
- **Dificuldade:** média. Introduz combate contra múltiplos inimigos.

### 3.3 — Caverna Musgosa
- **Asset:** Mossy Cavern
- **Função:** verticalidade — a caverna desce em camadas. Poços de esporos
  tóxicos (hazard ambiental), plataformas que desmoronam.
- **Inimigos:** Morcegos-Eco (voam em padrão senoidal, atacam em mergulho),
  Rastejantes (emergem do chão, exigem atenção ao redor, não só à frente).
- **Habilidade obtida ao final:** **Escalada de Parede** (wall cling +
  wall jump) — abre atalhos de volta para a Vila e a Floresta.
- **Dificuldade:** alta. Combina hazards ambientais com inimigos.

### 3.4 — Santuário Esquecido (fase final)
- **Asset:** combinação/adaptação dos anteriores (é a versão "corrompida"
  da Caverna — mesma paleta, mais saturada/distorcida) — pode reusar tiles
  da Mossy Cavern com um shader/tint diferente, sem precisar de asset novo.
- **Função:** confronto final contra o Guardião.
- **Chefe:** **O Guardião do Lúmen** — três fases:
  1. Ataques de longo alcance (feixes de "memória" que Kaida precisa
     desviar/dashar através).
  2. Invoca ecos dos inimigos já enfrentados (versões fracas de Cascas,
     Lobos, Morcegos) — testa tudo que o jogador aprendeu.
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
essas flags antes de permitir passagem (ver tarefas no `CLAUDE_CODE_BRIEF.md`).

## 5. Colecionáveis

- **Fragmentos de Lúmen** (lore, opcionais): pequenos textos/memórias que
  montam a história de Kaida e do vale. Não afetam a jogabilidade.
- **Nódulos de Vida**: aumentam a vida máxima (4 no jogo — cada um +1 pip).

## 6. Curva de Dificuldade (resumo)

```
Vila (tutorial) → Floresta (combate básico + grupos) →
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
