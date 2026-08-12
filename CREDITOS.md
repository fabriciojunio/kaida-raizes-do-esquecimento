# Créditos de arte

Todo o código deste repositório é do projeto. **A arte não**: vem de pacotes
de terceiros. Esta página registra a origem de cada peça.

## Legacy Fantasy - High Forest

- **Autor:** Anokolisa
- **Página:** https://anokolisa.itch.io/sidescroller-pixelart-sprites-asset-pack-forest-16x16
- **Licença:** gratuito, uso comercial permitido. O autor não cobra nada -
  pede apenas uma avaliação honesta na página do pacote.
- **Tile:** 16x16 pixels

É a base visual de praticamente tudo no jogo.

| No jogo | De onde vem |
|---------|-------------|
| Kaida (protagonista) | `Character/` - Idle, Run, Jump-Start, Jump-All, Jump-End, Attack-01, Dead |
| Javali-Casca | `Mob/Boar/` |
| Abelha-Eco | `Mob/Small Bee/` |
| Caracol-Rastejante | `Mob/Snail/` |
| Guardião do Lúmen | `Mob/Small Bee/` reaproveitado em escala grande, com tint |
| Chão das cinco regiões | `Assets/Tiles.png` |
| Água do Lago Silente | `Assets/Tiles.png` |
| Tocha, chave, medalhão e frascos | `Assets/Tiles.png`, recortados peça a peça |
| Árvores (5 cores, uma por região) | `Trees/` |
| Arbustos e cogumelos | `Assets/Tree-Assets.png` |
| Casas e portas da Vila | `Assets/Buildings.png` |
| Mata ao fundo | `Trees/Background.png` |
| Céu | `Background/Background.png` |

## Stringstar Fields

Fundos usados na Caverna Musgosa e no Santuário Esquecido
(`background_1.png` e `background_2.png`). Também pixel art 16x16, mesma
escala do resto.

Confirme a licença na página de origem antes de publicar o jogo fora da
faculdade - o pacote não veio com arquivo de licença.

## Música

**Não é de terceiros.** Nenhum dos pacotes trazia áudio, então a trilha é
gerada por síntese em tempo de execução (`Assets/Scripts/Systems/TrilhaSonora.cs`):
escala menor, arpejo lento e baixo sustentado, com tônica e andamento
diferentes por região.

## O que ficou de fora

| Pacote | Motivo |
|--------|--------|
| Mossy Assets | Não é pixel art na mesma escala - as folhas têm 4096x4096 contra tiles de 16x16. Misturar quebraria a coerência visual. |
| BlueWizard Animations | Mesma questão de escala e estilo. |
| Plant Animations | Idem. |
| Slimes | Idem. |
| sample (idle & walk) | É a personagem de referência escolhida no começo, mas o arquivo só tem duas animações (idle e walk) e um metroidvania de ação precisa de oito. Se você conseguir o pacote completo dela, dá para trocar só o Animator do prefab da Kaida. |

## Ao publicar

Para entrega de faculdade, citar a origem no relatório e nos créditos do
jogo é suficiente. Se for publicar em algum lugar público (itch.io, Steam),
vale reler os termos na página do Anokolisa e creditá-lo pelo nome.
