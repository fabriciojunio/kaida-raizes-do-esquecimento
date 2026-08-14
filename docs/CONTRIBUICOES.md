# Divisão de tarefas

> **Antes de entregar:** confiram e ajustem esta tabela para refletir o que
> cada um fez de fato. O professor avalia a distribuição de tarefas e pede
> que cada integrante apresente suas contribuições na defesa oral.

**Equipe:** Camila Pereira Raimundo · Fabrício Júnio Almeida Dias ·
Kauã Limão Nunes · Luan Miranda Padilha

## Áreas do projeto

O projeto está dividido nas frentes abaixo. Cada uma corresponde a pastas e
arquivos identificáveis no repositório, o que facilita mostrar o código na
apresentação.

### 1. Personagem e controles

**Arquivos:** `Assets/Scripts/Player/`

Máquina de estados do jogador (9 estados, um arquivo cada), física de
movimento com gravidade assimétrica, pulo de altura variável, dash com
invulnerabilidade, coyote time e jump buffer. Configuração de balanceamento
em `PlayerStats`.

**Responsável:** Camila Pereira Raimundo

### 2. Inimigos e chefe

**Arquivos:** `Assets/Scripts/Enemies/` e `Assets/Scripts/Enemies/Boss/`

Inimigo base com patrulha, detecção por linha de visão e dano por contato.
Três comportamentos derivados (javali, abelha, caracol) e o Guardião do
Lúmen, com máquina de estados própria e barra de vida única.

**Responsável:** Fabrício Júnio Almeida Dias

### 3. Mundo, progressão e save

**Arquivos:** `Assets/Scripts/World/` e `Assets/Scripts/Systems/`

Checkpoints, transições entre regiões com ponto de chegada, coletáveis,
perigos, parallax, sistema de save em JSON e controle de dificuldade.

**Responsável:** Kauã Limão Nunes

### 4. Interface e áudio

**Arquivos:** `Assets/Scripts/UI/` e `Assets/Scripts/Systems/TrilhaSonora.cs`

Menu principal com cenário desfocado ao fundo, menu de pausa, telas de
morte e vitória, HUD de vida, barra do chefe, tela de créditos e a trilha
sonora gerada por síntese.

**Responsável:** Luan Miranda Padilha

### 5. Level design e geração de cenas

**Arquivos:** `Assets/Editor/`

Pipeline que monta o jogo por código: fatiamento de sprites com detecção de
frames, geração de animações e prefabs, recorte de tiles e construção das
seis cenas a partir de mapas em texto.

**Responsável:** Camila Pereira Raimundo

### 6. Testes e qualidade

**Arquivos:** `Assets/Tests/`

138 casos automatizados em EditMode e PlayMode, incluindo o validador de
alcance dos mapas e os testes de colisão do chão nas cenas reais.

**Responsável:** Fabrício Júnio Almeida Dias

## Sugestão de fala na apresentação

O professor reserva 15 minutos por grupo, cobrindo o jogo, o código-fonte e
as contribuições de cada integrante. Uma divisão que cabe no tempo:

| Momento | Duração | Conteúdo |
|---|---|---|
| Abertura | 1 min | O que é o jogo, gênero e referências |
| Demonstração | 5 min | Jogar do menu até o chefe, mostrando as habilidades |
| Código | 6 min | Cada integrante mostra a sua frente |
| Fechamento | 2 min | Testes, dificuldades encontradas e o que aprenderam |
| Perguntas | 1 min | - |

### Pontos fortes que vale destacar no código

1. **Máquina de estados do jogador** - mostra separação de
   responsabilidades: cada estado num arquivo, sem condicionais aninhadas.

2. **Cenas geradas a partir de mapas em texto** - mostrar
   `SceneBuilder.cs`, editar um mapa ao vivo e regenerar a região é uma
   demonstração forte.

3. **Trilha gerada por síntese** - nenhum pacote trazia áudio, e em vez de
   deixar o jogo mudo a música é construída por código, com escala menor e
   tônica diferente por região.

4. **Testes que nasceram de bug real** - o validador de alcance dos mapas
   reprovou as cinco regiões da primeira versão, e o teste de colisão do
   chão pegou um defeito que deixava a personagem atravessar o cenário sem
   que nada aparecesse na tela.

### Dificuldades que valem ser contadas

Vale mencionar os problemas enfrentados, porque mostram processo:

- O tamanho de frame das folhas de sprite varia por animação (48 a 96 px),
  então foi preciso detectá-lo contando pixels em vez de fixar um valor.
- O pivô precisava ser medido no primeiro frame de cada folha: medindo a
  folha inteira, o rastro do golpe puxava o pivô e a personagem subia meia
  unidade ao atacar.
- O chefe, como corpo dinâmico, pousava numa plataforma da arena e ficava
  alto demais para o ataque corpo a corpo. Virou corpo cinemático.
- Virar a personagem de lado só troca o flipX do sprite: os marcadores
  presos a ela continuavam à direita, então virada para a esquerda o golpe
  saía pelas costas. Nenhum teste pegou porque todos punham o inimigo à
  direita.
