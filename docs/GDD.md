# KAIDA - RAÍZES DO ESQUECIMENTO
### Game Design Document

**Equipe:** Fabrício Júnio Almeida Dias · Camila Pereira Raimundo ·
Luan Miranda Padilha · Kauã Limão Nunes

**Disciplina:** Desenvolvimento de Jogos Digitais - Ciência da Computação

---

## Sumário

1. [Introdução](#1-introdução)
2. [Público-alvo](#2-público-alvo)
3. [Sinopse](#3-sinopse)
4. [Plataforma e requisitos mínimos](#4-plataforma-e-requisitos-mínimos)
5. [Personagens](#5-personagens)
6. [Cenários](#6-cenários)
7. [Narrativa](#7-narrativa)
8. [Progressão e habilidades](#8-progressão-e-habilidades)
9. [Itens e recompensas](#9-itens-e-recompensas)
10. [Vitória e derrota](#10-vitória-e-derrota)
11. [Estilo de arte](#11-estilo-de-arte)
12. [Sonorização](#12-sonorização)
13. [Mecânicas e jogabilidade](#13-mecânicas-e-jogabilidade)
14. [Regras](#14-regras)

---

## 1. Introdução

**Kaida - Raízes do Esquecimento** é um metroidvania 2D de ação e
exploração, em que a jogadora controla Kaida, uma espadachim que acorda sem
memória num vale amaldiçoado e precisa alcançar a criatura responsável pelo
Esquecimento.

O gênero metroidvania foi escolhido porque ele liga diretamente **level
design e progressão**: o mapa é um só, interconectado, e áreas que estavam
fora de alcance passam a ser acessíveis conforme a personagem ganha
habilidades novas. Isso exige que o cenário seja pensado em função do que a
personagem consegue fazer, e não como fases isoladas.

As referências principais são *Hollow Knight* (atmosfera, progressão por
habilidades, mapa interconectado) e a série *Prince of Persia* (a sensação
do movimento: fluidez, precisão, quedas calculadas). Nenhum conteúdo desses
jogos foi reutilizado - a inspiração é de design.

## 2. Público-alvo

Pessoas a partir de 12 anos, de qualquer gênero, com alguma familiaridade
com jogos de plataforma. Não há conteúdo gráfico violento: o combate é
estilizado, em pixel art, sem sangue.

## 3. Sinopse

O **Vale de Myrrhen** era próspero: vilas ligadas por estradas de pedra,
uma floresta que guardava a memória do povo e cavernas de onde se extraía o
**lúmen**, um mineral capaz de reter lembranças.

Um evento conhecido apenas como **o Esquecimento** varreu o vale. Da noite
para o dia, os moradores pararam de lembrar quem eram. Alguns definharam;
outros se corromperam, viraram cascas movidas por instinto, vagando pelas
ruínas das próprias casas.

**Kaida** acorda na orla da floresta sem lembrança de como chegou ali - só
uma espada nas costas e a sensação de já ter estado naquele vale. No coração
do mapa dorme o **Guardião do Lúmen**, que antes protegia a memória coletiva
e hoje a consome para se manter inteiro.

Kaida precisa alcançá-lo: para detê-lo, ou para se lembrar de si mesma
através dele. O jogo não entrega qual é o caso até o fim.

## 4. Plataforma e requisitos mínimos

**Plataforma:** PC (Windows 64 bits)
**Engine:** Unity 2022.3.62f3

| Requisito | Mínimo |
|---|---|
| Sistema operacional | Windows 10 |
| Processador | Dual core 2.0 GHz |
| Memória RAM | 4 GB |
| Vídeo | Integrada com DirectX 11 |
| Espaço em disco | 250 MB |

O jogo assume a resolução nativa do monitor e funciona de 4:3 a ultrawide
21:9 sem cortar interface.

## 5. Personagens

### 5.1 Protagonista

**Kaida** - espadachim sem memória. Move-se com agilidade: corre, dá dash,
pula com altura variável e, conforme avança, ganha o pulo duplo. Começa com
5 pontos de vida no modo Normal.

### 5.2 Inimigos

| Inimigo | Comportamento | Vida | O que ensina |
|---|---|---|---|
| **Javali-Casca** | Recua, bufa e dispara numa investida reta. Levar dano durante a corrida interrompe o ataque | 3 | Ler o aviso, desviar, atacar na abertura |
| **Abelha-Eco** | Voa em onda senoidal e mergulha em diagonal. Depois do mergulho, paira ao alcance | 2 | Olhar para cima, não só para os lados |
| **Caracol-Rastejante** | A cada dois golpes se fecha na casca e fica imune por 1,1 s | 3 | Esperar o tempo certo em vez de socar o botão |

### 5.3 Chefe

**O Guardião do Lúmen** - criatura flutuante corrompida pelo mineral que
deveria proteger. Um confronto só, com **20 pontos de vida numa barra
única**. Ele alterna dois padrões:

| Padrão | Comportamento | O que cobra |
|---|---|---|
| Feixes | Salvas em leque na direção da jogadora, a cada 2,4 s | Dash, para atravessar na janela de invulnerabilidade |
| Investida | Avança em linha reta até a posição onde a Kaida estava | Leitura do aviso e reposicionamento |

Entre um e outro ele acompanha a altura de quem está jogando, para que a
espada sempre tenha como alcançá-lo. Durante a abertura é intocável, e cada
golpe recebido o faz piscar.

O confronto já foi dividido em três fases, com invocação de inimigos entre
elas. Na prática virava matar horda enquanto o chefe assistia de longe, e o
jogador chegava ao fim sem sentir que tinha enfrentado alguém. A arena tem
**três inimigos comuns, um de cada tipo, colocados no mapa** - não são
invocados nem repostos.

## 6. Cenários

Cinco regiões conectadas, percorríveis nos dois sentidos. Todas usam o
mesmo tileset com cor diferente, o que mantém a paleta coerente e dá
identidade a cada área.

| Região | Identidade visual | Função no jogo |
|---|---|---|
| **Orla da Vila** | Neutra, casas, árvores verdes | Tutorial: ensina andar, pular, atacar e dash num espaço seguro |
| **Floresta Silente** | Verde dessaturado, mata densa | Primeira área real. Ao fim, o **Pulo Duplo** |
| **Lago Silente** | Claro, árvores douradas, água | Travessia por cima da água. Cair devolve o caminho andado |
| **Caverna Musgosa** | Azul-acinzentado, sem árvores | Verticalidade: escadas de plataformas dos dois lados |
| **Santuário Esquecido** | Violeta, árvores vermelhas | Arena do confronto final |

O fundo de cada região tem quatro camadas de mata com parallax próprio,
deslocadas entre si para que o céu apareça apenas em frestas entre as
copas.

## 7. Narrativa

A história não é contada por cutscenes, e sim por **Fragmentos de Lúmen**:
colecionáveis opcionais espalhados fora da rota principal. Quem só corre
até o fim termina o jogo; quem explora entende o que aconteceu no vale.

| Região | Fragmento |
|---|---|
| Orla da Vila | *"Acordei na orla sem saber o próprio nome. A espada nas costas parecia me conhecer melhor do que eu."* |
| Floresta Silente | *"A floresta guardava a memória do povo. Quando o Esquecimento veio, ela escureceu primeiro - como quem fecha os olhos."* |
| Lago Silente | *"Tiravam lúmen daqui. Diziam que a pedra segurava lembranças. Ninguém perguntou de quem eram."* |
| Caverna Musgosa | *"Parar também é uma forma de esquecer."* |

Ao derrotar o Guardião, a tela final diz apenas que *"o vale volta a
lembrar"*, sem esclarecer se Kaida o deteve ou se lembrou de si mesma
através dele. O desfecho fica em aberto de propósito.

## 8. Progressão e habilidades

| Habilidade | Onde é obtida | O que libera |
|---|---|---|
| Movimento, ataque e dash | Início | Base do jogo |
| **Pulo Duplo** | Alto da Floresta Silente | Plataformas e coletáveis fora da rota principal |

A habilidade fica gravada no save e coletáveis já pegos não reaparecem ao
revisitar uma sala.

O Pulo Duplo é opcional por decisão de projeto: abre atalhos e alcança os
nódulos de vida escondidos, mas nenhum caminho obrigatório depende dele.
Todas as cinco regiões são atravessáveis com o movimento básico, e é um
teste automatizado que garante isso a cada alteração de mapa.

## 9. Itens e recompensas

| Item | Efeito | Onde |
|---|---|---|
| **Fragmento de Lúmen** | Narrativo, não afeta jogabilidade | Fora da rota principal, um por região |
| **Nódulo de Vida** | +1 de vida máxima, permanente | Escondido em plataformas altas |
| **Marco de descanso** | Salva o progresso e define o ponto de retorno | Início e meio de cada região |

## 10. Vitória e derrota

**Vitória:** derrotar o Guardião no Santuário Esquecido. A
tela final mostra o balanço da jornada - fragmentos encontrados, nódulos
reunidos e habilidades despertadas - e oferece jogar de novo ou voltar ao
menu.

**Derrota:** a vida chega a zero. A tela de morte conta as quedas da sessão
e deixa voltar ao último marco ou ao menu. Não há limite de tentativas nem
perda de progresso.

## 11. Estilo de arte

Pixel art de 16 pixels por unidade, com filtro *Point* e sem compressão,
para os pixels ficarem nítidos. A personagem tem cerca de 3 tiles de
altura.

**Paleta:** fria e dessaturada (musgo, pedra, azul-acinzentado), com pontos
de luz quente - o lúmen, as tochas - para guiar o olhar e representar
"memória viva" no meio da decadência. Cada região aplica um tom próprio
sobre o mesmo tileset.

**Interface:** montada por código, com fundo escuro translúcido e texto cor
de osso. O menu principal mostra um trecho de cenário rodando ao fundo,
desfocado por shader, com a personagem andando sozinha.

## 12. Sonorização

A trilha é **gerada por síntese em tempo de execução**, sem arquivos de
áudio. Usa escala menor natural, com arpejo lento por cima e um baixo
sustentado embaixo, envelope em curva para as notas se dissolverem umas nas
outras.

Cada região tem tônica e andamento próprios:

| Região | Tônica | Duração da nota |
|---|---|---|
| Menu | Lá (220 Hz) | 1,15 s |
| Orla da Vila | Si (246,94 Hz) | 1,05 s |
| Floresta Silente | Sol (196 Hz) | 1,20 s |
| Lago Silente | Dó (261,63 Hz) | 1,10 s |
| Caverna Musgosa | Fá (174,61 Hz) | 1,40 s |
| Santuário Esquecido | Ré (146,83 Hz) | 1,55 s |

Quanto mais fundo no vale, mais grave e arrastada a música. O volume é
baixo de propósito - é som ambiente - e ajustável no menu.

## 13. Mecânicas e jogabilidade

### 13.1 Movimento

| Ação | Tecla | Detalhe |
|---|---|---|
| Andar | Setas / A / D | Aceleração e desaceleração diferentes no chão e no ar |
| Pular | Espaço | Altura variável: soltar cedo corta a subida |
| Atacar | Ctrl esq. / clique | Área retangular à frente, alcança inimigos voadores |
| Dash | Shift esq. | Invulnerável durante o deslocamento |
| Pausar | Esc | Congela o jogo e devolve o cursor |

**Assistências de game feel**, invisíveis mas decisivas:

- **Coyote time** (0,11 s): dá para pular por um instante após sair da borda
- **Jump buffer** (0,13 s): o pulo apertado pouco antes de aterrissar vale
- **Gravidade assimétrica**: a queda é mais rápida que a subida, o que dá
  peso ao salto

### 13.2 Arquitetura do jogador

O comportamento é uma **máquina de estados**, com um arquivo por estado:
`idle`, `run`, `jump`, `fall`, `dash`, `attack`, `hurt`, `dead` e
`wallcling`. Mecânica nova vira estado novo, nunca um `if` dentro de um
estado existente.

Os valores de balanceamento ficam num ScriptableObject (`PlayerStats`), e
nunca no meio da lógica.

### 13.3 Combate

O ataque procura qualquer coisa que implemente `IDamageable` numa área à
frente da personagem - assim o mesmo golpe serve para inimigo comum e para
o chefe, sem o jogador precisar conhecer cada tipo.

Ao apanhar, Kaida recebe empurrão na direção contrária e uma janela de
invulnerabilidade de 1 segundo, com o sprite piscando para o estado ficar
legível.

### 13.4 Câmera

Segue a personagem com suavização e trava nos limites da região. A margem é
calculada a partir da proporção real da tela, então nem um monitor
ultrawide mostra o vazio além da borda do cenário.

### 13.5 Plataformas

As plataformas são atravessáveis por baixo (`PlatformEffector2D`): sobe-se
através delas e pousa-se em cima. Como blocos sólidos, qualquer plataforma
no meio do caminho viraria teto.

### 13.6 Dificuldade

| | Fácil | Normal | Difícil |
|---|---|---|---|
| Vida máxima | 7 | 5 | 3 |
| Invulnerabilidade | 1,5 s | 1,0 s | 0,65 s |
| Velocidade dos inimigos | 85% | 100% | 125% |
| Alcance de visão deles | 80% | 100% | 130% |

A escolha aparece ao clicar em *Novo jogo*, e não num submenu à parte: a
dificuldade vale para a partida inteira, então é uma pergunta que o jogo
precisa fazer antes de começar, não uma opção que dá para nunca abrir.

### 13.7 Salvamento

O progresso é gravado em JSON ao tocar um marco de descanso: posição,
habilidades desbloqueadas e itens coletados. O botão *Continuar* do menu
fica desativado enquanto não existe um save.

## 14. Regras

1. O pulo duplo só funciona depois de encontrado no mapa.
2. Coletáveis já pegos não reaparecem ao revisitar uma sala.
3. Cair na água do lago custa vida e devolve ao último marco.
4. Dentro da casca, o caracol não recebe dano.
5. Durante a abertura, o Guardião é intocável.
6. O Guardião não invoca inimigos: quem está na arena está no mapa.
7. Morrer devolve ao último marco tocado, sem perder habilidades ou itens.
8. Não há limite de tentativas.
