# Como conectar o Claude Code à Unity via MCP

**Importante primeiro:** isso conecta o **Claude Code rodando no seu
computador** ao **editor Unity aberto no seu computador**. É diferente do
Claude no chat (que escreveu este projeto) — aqui, na sua máquina, com o
MCP configurado, o Claude Code passa a poder criar objetos, anexar scripts,
rodar o jogo e ler o console de erros dentro do próprio editor Unity.

Existem hoje (2026) mais de uma opção. Nenhuma delas eu testei pessoalmente
(não tenho acesso à Unity neste ambiente) — estas instruções vêm de
documentação pública dos próprios projetos. Escolha UMA:

## Opção A — Unity MCP Server oficial (beta da própria Unity)

A Unity lançou em beta um servidor MCP oficial que dá a agentes de IA em
IDEs (Claude Code, Cursor, VS Code Copilot) acesso ao projeto Unity aberto:
hierarquia de cena, GameObjects, valores de componentes, console, e
permite editar scripts e disparar ações do editor.

- Faz parte do **Unity AI (beta)** — veja a documentação oficial da Unity
  para instruções de ativação mais recentes (busque "Unity AI MCP Server"
  no site da Unity, já que isso está em beta e pode mudar).
- Vantagem: é da própria Unity, tende a ser mais estável a longo prazo.
- Requer uma versão recente da Unity com o Unity AI Assistant habilitado.

## Opção B — Coplay MCP (comunidade, ativamente mantido)

1. No Unity Package Manager, adicione o pacote via Git URL:
   `https://github.com/CoplayDev/unity-plugin.git#beta`
2. Confirme que a extensão Coplay está habilitada no editor.
3. No terminal (fora do Claude Code), com o Claude Code CLI já instalado,
   rode o comando de registro do servidor MCP (consulte a documentação do
   Coplay em docs.coplay.dev — o comando usa `claude mcp add` com o
   pacote `coplay-mcp-server`).
4. Reinicie o Claude Code para carregar a conexão.

## Opção C — unity-mcp (comunidade, open source)

1. No Unity Package Manager: **Add package from git URL** e cole a URL do
   repositório `unity-mcp` (pesquise "unity-mcp github" para achar o fork
   mais atualizado — há mais de um mantido pela comunidade).
2. Isso instala um servidor HTTP local dentro do editor (geralmente em
   `http://localhost:8080`).
3. No terminal, registre o servidor no Claude Code:
   ```
   claude mcp add-json unityMCP '{"type":"http","url":"http://localhost:8080/mcp"}' --scope user
   ```
4. No Unity, abra a janela do plugin (geralmente em algo como
   **Window > MCP for Unity**) e clique em **Start Server**.
5. Reinicie o Claude Code.

## Depois de conectado — como validar

No terminal, dentro da pasta do projeto, rode `claude` e peça algo simples
para testar, por exemplo:
> "Liste os objetos na cena atual do Unity"

Se ele conseguir listar (em vez de dizer que não tem acesso), a conexão
está funcionando.

## Cuidados

- O servidor MCP roda em `localhost` — não é acessível de outras máquinas,
  o que é bom para segurança.
- Algumas operações (editar prefabs, mexer na cena) não funcionam com o
  jogo em modo Play — pause ou pare antes de pedir mudanças estruturais.
- Se o Claude Code disser que uma ferramenta falhou, peça para ele listar
  os objetos da cena de novo antes de tentar a operação — nomes errados
  são a causa mais comum de erro.
- Depois de qualquer mudança de script, dê um tempo para a Unity recompilar
  antes de rodar o jogo (o MCP consegue checar o status de compilação).

## Se você não quiser configurar o MCP agora

Sem o MCP, o Claude Code **ainda funciona bem** para este projeto: ele lê e
edita os arquivos `.cs`, `.tscn`... digo, `.unity`/`.prefab` diretamente
(são texto), e você testa manualmente apertando Play na Unity. Você perde
a automação de "criar objeto/testar/ver erro" num único pedido, mas o
desenvolvimento continua.
