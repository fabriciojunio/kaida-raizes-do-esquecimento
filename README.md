# Kaida — Raízes do Esquecimento

Metroidvania 2D em Unity. **Comece por `docs/CLAUDE_CODE_BRIEF.md`** se
você (ou o Claude Code) está retomando este projeto — ele lista o que já
existe e as tarefas em ordem.

## Leitura recomendada, nesta ordem
1. `docs/GDD_HISTORIA_E_DESIGN.md` — a história, o mundo, os inimigos, a
   progressão de habilidades. O "porquê" do jogo.
2. `docs/CLAUDE_CODE_BRIEF.md` — o "o que fazer", passo a passo, para
   quem for continuar o desenvolvimento.
3. `docs/MCP_SETUP.md` — como conectar o Claude Code ao editor Unity.
4. `docs/GUIA_INTEGRACAO_ASSETS.md` — como importar os assets escolhidos.

## Status atual (importante)
Todo o código em `Assets/Scripts/` foi escrito e **checado sintaticamente**
(compilei contra uma simulação da API Unity — 0 erros de sintaxe/tipo),
mas **nunca foi aberto dentro da Unity de verdade**. Não existem cenas
`.unity` ainda. Isso é um scaffold de arquitetura, não um jogo jogável
hoje. Veja `docs/CLAUDE_CODE_BRIEF.md` para o que falta.

## Requisitos
- Unity 2022.3 LTS (ou mais recente) com suporte 2D
- Pacotes: 2D Animation, 2D Tilemap Editor, 2D Sprite, Test Framework
  (listados em `Packages/manifest.json`)
- Os asset packs escolhidos (ver `docs/GUIA_INTEGRACAO_ASSETS.md` para links)

## Engine
Unity (C#). Projeto versionado com git.
