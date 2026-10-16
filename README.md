# 🕵️‍♂️ LlamaNoir — Sala de Interrogatório Policial (1947)

[![Versão](https://img.shields.io/badge/versão-v0.1.0-blue.svg)](https://github.com/kelvynzucco/llama-noir/releases)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![MonoGame](https://img.shields.io/badge/MonoGame-3.8.2-E11925?logo=monogame&logoColor=white)](https://monogame.net/)
[![IA Local](https://img.shields.io/badge/IA_Local-Qwen_2.5_3B_(Llamafile)-22c55e?logo=mozilla&logoColor=white)](https://github.com/Mozilla-Ocho/llamafile)
[![Licença](https://img.shields.io/badge/Licen%C3%A7a-MIT-green)](LICENSE)

**LlamaNoir** é um jogo de investigação policial e interrogatório psicológico com atmosfera de *Film Noir* (década de 1940/1950), desenvolvido em **C# (.NET 8)** com **MonoGame**.

O jogador assume o papel de um detetive de homicídios da Delegacia Central, conduzindo inquéritos contra suspeitos cujos depoimentos, reações corporais, álibis e estados emocionais são simulados dinamicamente em tempo real através de um **modelo de Inteligência Artificial local (Qwen 2.5 3B Instruct via Mozilla Llamafile)**.

Nenhum dado sai do seu computador: o jogo opera de forma **100% privada, offline e confidencial**.

---

## 🖥️ Requisitos de Sistema

| Requisito | Mínimo | Recomendado |
| :--- | :--- | :--- |
| **Sistema Operacional** | Windows 10 (64-bit) | Windows 11 (64-bit) |
| **Processador (CPU)** | Intel Core i3 / AMD Ryzen 3 (4 núcleos) | Intel Core i5 / AMD Ryzen 5 ou superior (6+ núcleos) |
| **Memória RAM** | 8 GB RAM | 16 GB RAM |
| **Armazenamento** | 4 GB de espaço livre em disco | 4 GB de espaço livre em SSD |
| **Placa de Vídeo (GPU)** | Gráficos integrados com suporte a Vulkan/DirectX 12 | GPU dedicada (NVIDIA GTX 1060 / AMD RX 580 ou superior) com 4 GB+ VRAM |
| **Conexão de Internet** | Necessária **apenas** no primeiro início para baixar os arquivos da IA (~2.1 GB) | Banda larga para download rápido |

> **Nota de Performance**: Caso o computador possua GPU dedicada, o jogo aloca automaticamente as camadas neurais do modelo para acelerar as respostas em tempo real. Em computadores sem GPU dedicada, o modelo roda automaticamente via processamento multithread de CPU.

---

## ⚡ Como Baixar e Jogar (Versão Pronta para Jogar)

Não é necessário instalar ferramentas de programação nem configurar servidores manualmente:

1. Acesse a aba **[Releases](https://github.com/kelvynzucco/llama-noir/releases)** do projeto.
2. Baixe o arquivo **`LlamaNoir-v0.1.0-win-x64.zip`**.
3. Extraia o conteúdo do arquivo `.zip` em qualquer pasta do seu computador.
4. Dê um duplo clique em **`LlamaNoir.exe`**.
5. No primeiro início, clique em **`[ 📥 BAIXAR AGENTE DE IA AUTOMATICAMENTE ]`**. O jogo cuidará de baixar o motor Llamafile e o modelo Qwen 2.5 3B diretamente para a pasta local.
6. Assim que a barra de download atingir 100%, o interrogatório começa imediatamente!

---

## 🔍 Mecânicas do Jogo

### 🧩 Inquéritos Criminais e Suspeitos
* **Culpados vs. Inocentes Incriminados**: Casos alternam entre criminosos ardilosos (objetivo: quebrar contradições e extrair uma confissão formal) e vítimas de armações (objetivo: desmascarar a farsa, provar a inocência e identificar o mandante).
* **Ficha Criminal Procedural**: Retratos gerados em pixel-art com silhueta sob lâmpada de delegacia e histórico de antecedentes policiais.
* **Polígrafo Forense em Tempo Real**: Osciloscópio biométrico com frequência cardíaca (BPM) reagindo à pressão psicológica e blefes.

### 💼 Dossiê Forense de Evidências (Uso Único)
* **4 Provas Periciais**: Chaves, relatórios de balística, documentos adulterados e evidências recolhidas na cena do crime.
* **Sinergia Investigativa Secreta**: Ao confrontar o suspeito com uma prova, ela é fixada ao depoimento e descartada. O resultado da pertinência só é revelado no prontuário após o confronto: evidências compatíveis desestabilizam o suspeito, enquanto provas desconexas destroem a credibilidade do detetive!

### 🎙️ Linhas Estratégicas de Inquirição
* `[EMPATIA]`: Conquista a confiança do depoente, reduz o estresse e facilita acordos judiciais.
* `[PRESSÃO]`: Intimida e confronta depoimentos, acelerando o avanço do caso com risco de colapso emocional.
* `[CONFRONTAÇÃO]`: Questiona álibis e detalha incongruências temporais.
* `[REVELAÇÃO / FINAL]`: Desfecho acusatório para selar a confissão ou inocentar a vítima.

### 📜 Linha do Tempo e Arquivo Taquigráfico
* **Histórico de Turnos**: Clique nos botões `[T1]`, `[T2]`, ... na barra superior para revisar exatamente o que o suspeito alegou em perguntas passadas.
* **Controle da Fita Gravadora**: Use o scroll do mouse ou a tecla `[F3]` para alternar entre o foco no turno corrente e a rolagem completa do inquérito.

---

## 🕹️ Controles e Atalhos

| Comando | Descrição |
| :--- | :--- |
| **Clique / Teclas [1] a [4]** | Seleciona e executa uma linha de inquirição |
| **Clique na Prova (Dossiê)** | Fixa ou desafixa uma evidência para o próximo confronto |
| **Scroll do Mouse** | Rola o prontuário e arquivo taquigráfico |
| **[F3]** | Alterna entre o turno atual e o rolo de depoimentos completo |
| **[F5]** | Conclui/arquiva o caso atual e inicia um novo inquérito criminal |
| **[Esc]** | Minimiza o aplicativo |

---

## 🛠️ Compilação a partir do Código-Fonte (Desenvolvedores)

Se você deseja modificar o código-fonte em C#:

```bash
# Clone o repositório
git clone https://github.com/kelvynzucco/llama-noir.git
cd llama-noir

# Compile e execute
dotnet run
```

---

## 🛡️ Identidade Visual
O jogo utiliza oficialmente o **Distintivo Policial Histórico** (`assets/Distintivo.ico` e `assets/Distintivo.png`) como ícone de aplicativo no Windows Explorer, na barra de tarefas e no cabeçalho da Delegacia Central.

---

## 📄 Licença
Distribuído sob a licença **MIT**. Sinta-se livre para jogar, inspecionar e aprimorar o código!
