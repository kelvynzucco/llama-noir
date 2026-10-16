# Modelos Locais (Llamafile)

Esta pasta é reservada para a execução do modelo local via [Mozilla Llamafile](https://github.com/Mozilla-Ocho/llamafile).

Por restrições de tamanho de arquivo do GitHub (arquivos > 100 MB), os binários do modelo **não são versionados** no repositório.

## Como configurar o Llamafile (Opcional)

O jogo funciona **100% offline mesmo sem o Llamafile**, utilizando o motor de geração procedural e simulação narrativa embutido (`ProceduralCaseGenerator`).

Se desejar ativar a inteligência artificial local com **Qwen 2.5 3B**:

1. Baixe o executável `llamafile.exe` no repositório oficial da Mozilla:
   * [https://github.com/Mozilla-Ocho/llamafile/releases](https://github.com/Mozilla-Ocho/llamafile/releases)
2. Baixe o modelo quantizado `.gguf` de sua preferência (ex: `Qwen2.5-3B-Instruct-abliterated.Q4_K_M.gguf` no Hugging Face) e coloque nesta pasta (`llamafile/`).
3. Inicie o servidor localmente:
   ```powershell
   .\llamafile.exe -m .\Qwen2.5-3B-Instruct-abliterated.Q4_K_M.gguf --port 8080 --embedding
   ```
4. No jogo, pressione **[F2]** para alternar entre o motor local procedural e o modelo de IA do Llamafile.
