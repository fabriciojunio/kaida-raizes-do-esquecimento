# Kaida - Raízes do Esquecimento

Metroidvania 2D desenvolvido em Unity 2022.3 para a disciplina de
Desenvolvimento de Jogos Digitais, do curso de Ciência da Computação.

Kaida acorda na orla de um vale que esqueceu de si mesmo e vai atrás do que
restou da própria memória.

## Integrantes do grupo

- Fabrício Júnio Almeida Dias
- Camila Pereira Raimundo
- Luan Miranda Padilha
- Kauã Limão Nunes

## Como jogar

### Opção 1 - baixar o jogo pronto (recomendado)

Baixe o arquivo `Kaida-Windows.zip` na página de
[**Releases**](https://github.com/fabriciojunio/kaida-raizes-do-esquecimento/releases/latest),
extraia a pasta e execute **`Kaida.exe`**.

Não é preciso instalar mais nada. O jogo é para **Windows 64 bits**.

> Se o Windows exibir o aviso "O Windows protegeu o seu computador", clique
> em **Mais informações → Executar assim mesmo**. Isso acontece porque o
> executável não tem assinatura digital paga, não porque haja algo errado
> com o arquivo.

### Opção 2 - abrir o projeto na engine

1. Instale o **Unity 2022.3.62f3** pelo Unity Hub.
2. No Unity Hub, clique em **Add** e aponte para a pasta deste projeto.
3. Abra o projeto. Na barra de menus aparece o menu **Kaida**.
4. Clique em **Kaida → MONTAR TUDO**. Esse passo fatia os sprites, gera as
   animações, os prefabs, os tiles e as seis cenas.
5. Abra `Assets/Scenes/00_MenuPrincipal.unity` e aperte **Play**.

O passo 4 é necessário porque o projeto gera os assets a partir do código
(veja *Como o jogo é montado*, mais abaixo).

## Requisitos mínimos

| | |
|---|---|
| Sistema | Windows 10 ou 11, 64 bits |
| Processador | Dual core 2.0 GHz |
| Memória | 4 GB de RAM |
| Vídeo | Placa integrada com suporte a DirectX 11 |
| Espaço | 250 MB livres |

O jogo assume a resolução do monitor onde for aberto, de 4:3 a ultrawide.

## Controles

| Ação | Tecla |
|------|-------|
| Andar | setas ou A / D |
| Pular | espaço |
| Atacar | Ctrl esquerdo ou botão esquerdo do mouse |
| Dash | Shift esquerdo |
| Pausar | Esc |

O pulo duplo só funciona depois de encontrar a habilidade no mapa.

## O jogo

Cinco regiões conectadas, percorridas nos dois sentidos:

```
Orla da Vila → Floresta Silente → Lago Silente → Caverna Musgosa → Santuário
 (tutorial)      (pulo duplo)      (travessia)     (verticalidade)     (chefe)
```

- **3 tipos de inimigo**, cada um com padrão próprio: o javali telegrafa e
  investe, a abelha mergulha em diagonal, o caracol se fecha na casca e
  fica imune por um instante
- **Chefe final** com barra de vida única, alternando feixes e investidas
- **Três dificuldades**, escolhidas ao começar a partida, que mudam vida,
  invulnerabilidade e o comportamento dos inimigos
- **Colecionáveis**: fragmentos de lore e nódulos que aumentam a vida máxima
- **Save automático** nos marcos de descanso

## Documentação

- [`docs/GDD.md`](docs/GDD.md) - Game Design Document
- [`docs/CONTRIBUICOES.md`](docs/CONTRIBUICOES.md) - divisão de tarefas
- [`CREDITOS.md`](CREDITOS.md) - origem e licença dos assets

Os créditos também aparecem **dentro do jogo**, pelo menu principal.

## Como o jogo é montado

As cenas não foram montadas arrastando objetos na tela: são geradas por
código a partir de mapas em texto, em `Assets/Editor/SceneBuilder.cs`. Cada
caractere é um tile de uma unidade.

```
"..............P....C..................B.............B........>..",
"################################################################",
```

Editar o mapa e rodar **Kaida → MONTAR TUDO** regenera a região inteira.
Além de ser mais rápido que posicionar objeto por objeto, o level design
fica versionado como texto, o que permite revisar mudanças no Git.

## Estrutura do projeto

```
Assets/
  Scripts/
    Player/       PlayerController e um arquivo por estado
    Enemies/      inimigo base, os três tipos e o chefe
    Systems/      máquina de estados, save, câmera, trilha, cursor
    World/        coletáveis, perigos, transições, parallax
    UI/           menus, HUD, créditos, telas de fim de jogo
  Editor/         geradores de sprites, animações, prefabs, tiles e cenas
  Tests/          EditMode e PlayMode
  Art/            sprites organizados por personagem e região
docs/             GDD e divisão de tarefas
```

## Testes

138 casos automatizados, todos passando.

```
EditMode   75/75      lógica e integridade do projeto
PlayMode   63/63      comportamento com a física rodando
```

Para rodar:

```powershell
.\montar.ps1 -SoTestes
```

Ou pelo editor: **Window → General → Test Runner**.

Boa parte dos testes nasceu de defeito encontrado jogando, e ficou para não
deixar voltar:

- **alcance dos mapas** - percorre as superfícies a partir do ponto de
  partida e reprova a região se alguma plataforma, item ou passagem ficar
  fora de alcance
- **chão com colisão** - carrega cada região, solta a personagem e mede se
  ela cai. Chão sem colisão não aparece em captura de tela nenhuma
- **confronto com o chefe** - na cena real, porque um chefe montado à mão
  passava enquanto o do jogo estava inalcançável
- **pivô das animações** - confere que os pés ficam alinhados entre idle,
  corrida e ataque

## Trilha sonora

Os pacotes de arte não incluem áudio, então a música é **gerada por síntese
em tempo de execução** (`Assets/Scripts/Systems/TrilhaSonora.cs`): escala
menor, arpejo lento e baixo sustentado, com tônica e andamento próprios
para cada região. O volume pode ser ajustado no menu.

## Créditos de arte

A arte vem do pacote **Legacy Fantasy - High Forest**, de **Anokolisa**
(gratuito, uso comercial permitido), e do **Stringstar Fields**. Detalhes em
[`CREDITOS.md`](CREDITOS.md).
