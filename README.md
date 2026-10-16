# 🕵️‍♂️ LlamaNoir: Interrogatório Policial com IA Local

Um jogo de investigação criminal em estilo **Noir**, desenvolvido em **C# (.NET 8)** com **MonoGame**. O jogo simula uma sala de interrogatório policial dos anos 1940/50 com retrato procedural, medidores biométricos e diálogos reativos orientados por inteligência artificial local ou pelo motor narrativo procedural.

---

## 🎮 Mecânicas do Jogo

* **Árvore de Diálogos Estruturada**: Opções dinâmicas de fala do detetive divididas em categorias estratégicas:
  * `[CONFRONTO]`: Questionamento de fatos e checagem de álibis.
  * `[EMPATIA]`: Acolhimento, redução de estresse e proteção policial.
  * `[PRESSÃO]`: Intimidação e confronto de contradições (alto risco de estresse).
  * `[REVELAÇÃO / FINAL]`: Desmascarar o mandante, confissão de culpa ou exoneração de inocente.
* **Dossiê de Evidências com Sinergia**:
  * Anexe pistas do inventário para reforçar perguntas (`★ REFORÇO CERTEIRO`).
  * Evidências desconexas são identificadas pelo suspeito como blefe amador (`⚠ DESCONEXA`), derrubando a confiança e elevando o estresse.
* **Culpados e Inocentes**:
  * Os casos gerados proceduralmente podem conter **suspeitos culpados** (objetivo: confissão e localização do corpo de delito) ou **inocentes incriminados por terceiros** (objetivo: desmascarar a farsa e exonerar o cidadão).
  * Pressionar cegamente um inocente até 100% de estresse causa **Falso Testemunho por Coação** e anulação do caso.
* **Medidores Psicológicos**:
  * **Confiança (0 a 100%)**: Se cair a zero, o suspeito recusa-se a cooperar.
  * **Estresse (0 a 100%)**: Se atingir 100%, o interrogatório colapsa.
  * **Progresso Investigativo (0 a 100%)**: Chegue a 100% antes do Turno 10 para vencer.
* **Limite de 10 Turnos**: O tempo corre contra você — no décimo turno, o advogado de defesa chega para retirar o suspeito da sala.

---

## ⌨️ Controles e Atalhos

* **[Teclas 1, 2, 3, 4]** ou **Clique do Mouse**: Selecionar opção de diálogo.
* **Clique nas Pistas (lado esquerdo)**: Selecionar e anexar pista à pergunta atual.
* **[F2]**: Alternar entre o motor local de IA (**Mozilla Llamafile - Qwen 2.5 3B**) e o **Simulador Procedural Offline**.
* **[F3]**: Alternar modo de visualização (Turno Atual em destaque vs. Histórico Completo rolável).
* **[F5]**: Gerar um Novo Caso Procedural inédito imediatamente.
* **Roda do Mouse**: Rolar o histórico da conversa.

---

## 🚀 Como Executar

### Pré-requisitos
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado na máquina.

### Execução Simples
Abra o terminal na pasta do projeto e execute:
```bash
dotnet run
```
Ou dê dois cliques no arquivo `rodar_jogo.bat` (no Windows).

---

## 🧠 Modo com Inteligência Artificial Local (Opcional)

O jogo funciona perfeitamente **sem necessidade de baixar o modelo**, utilizando o gerador procedural embutido.

Caso queira habilitar a enriquecimento de narrativa em tempo real com o modelo **Qwen 2.5 3B**:
1. Consulte o guia em [`llamafile/README.md`](./llamafile/README.md).
2. Coloque o `llamafile.exe` e o modelo `.gguf` dentro da pasta `llamafile/`.
3. Inicie o servidor localmente na porta `8080`.
4. Pressione **[F2]** durante o jogo para conectar.

---

## 📄 Licença
Distribuído sob a licença MIT. Sinta-se livre para customizar e expandir!
