# Guia rápido — Integrando os assets escolhidos

## Hero Knight / Platformer Character Pack (personagem)
1. Importe o pacote em `Assets/Art/Player/HeroKnight/` (ou
   `PlatformerCharacterPack/`).
2. Localize o Sprite Sheet principal, confirme o fatiamento no Sprite Editor.
3. Veja quais Animation Clips já vêm prontos no pacote (a maioria desses
   assets já inclui `.anim` prontos — não precisa desenhar keyframes).
4. Crie um Animator Controller (`Player.controller`), arraste os clipes,
   nomeie os estados exatamente como: `idle`, `run`, `jump`, `fall`,
   `dash`, `attack`, `hurt`, `death` (ou ajuste os nomes chamados em
   `PlayerController`/estados — ver `CLAUDE_CODE_BRIEF.md` seção 2).
5. Arraste o Animator Controller para o componente `Animator` do Player.

## Village Props / Mossy Cavern / Forest Sidescroller (ambiente)
1. Importe cada pacote na subpasta correspondente em `Assets/Art/Environment/`.
2. Para tiles: Window > 2D > Tile Palette > Create New Palette, escolha o
   sprite sheet do pacote como fonte.
3. Crie um `Tilemap` (GameObject > 2D Object > Tilemap > Rectangular) por
   camada (fundo decorativo, chão sólido, plataformas).
4. No Tilemap de chão sólido: adicione `Tilemap Collider 2D` +
   `Composite Collider 2D`, camada de física `Ground`.
5. Pinte o nível usando a paleta.
6. Props soltos (barris, placas, tochas) do Village Props: arraste como
   sprites normais (não precisam de Tilemap).

## Depois de importar
Rode o jogo (Play) numa cena de teste simples primeiro (só chão + Player)
antes de montar as regiões completas — confirma que a física e as
animações estão certas antes de investir tempo no level design.
