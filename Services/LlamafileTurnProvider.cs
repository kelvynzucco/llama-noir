using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LlamaInterrogation.Models;

namespace LlamaInterrogation.Services;

/// <summary>
/// Cliente HTTP + Diretor de Jogo Noir + Gerenciador automático do servidor local Llamafile (Qwen 2.5 3B Instruct Abliterated).
/// Arquitetura Híbrida (Diretor C# + Ator LLM):
/// - Garante casos Noir 100% coerentes, com pistas reais (nunca placeholders como "Título Curto") e segredos lógicos.
/// - Analisa exatamente o que o jogador digitou no chat para direcionar a resposta do suspeito no LLM sem alucinações.
/// - Usa aceleração por GPU limitada em ~80% (-ngl 29 de 37 camadas) e 80% das threads de CPU, com fallback automático para CPU.
/// </summary>
public class LlamafileTurnProvider : ILlmTurnProvider, IDisposable
{
    private static readonly Random Rng = new();
    private readonly HttpClient _httpClient;
    private readonly string _endpointUrl;
    private readonly SemaphoreSlim _serverLock = new(1, 1);
    private Process? _serverProcess;
    private bool _isServerVerifiedOnline;
    private readonly HashSet<string> _usedCaseTitles = new(StringComparer.OrdinalIgnoreCase);
    private NoirCaseBlueprint? _activeBlueprint;

    // 37 camadas totais no Qwen2.5-3B -> 29 camadas em GPU = ~78.4% (limite de 80% de uso da GPU,
    // mantendo a camada final de saída de 151k tokens na CPU para máxima estabilidade e baixo uso de VRAM).
    private const int GpuLayers80Percent = 29;

    private static readonly string[] SkinTones = { "clara", "media", "morena", "escura" };
    private static readonly string[] HairColors = { "grisalho", "preto", "castanho", "loiro", "ruivo" };
    private static readonly string[] HairStyles = { "curto", "longo", "calvo", "topete" };
    private static readonly string[] OutfitStyles = { "terno", "jaqueta", "vinho", "uniforme", "jaleco" };

    public string ProviderName => "Qwen 2.5 3B Noir (Llamafile • GPU 80%)";

    public LlamafileTurnProvider(string baseUrl = "http://127.0.0.1:8080")
    {
        _endpointUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(45)
        };
    }

    private static int GetOptimalThreadCount()
    {
        int logicalCores = Environment.ProcessorCount;
        // Limita o uso de CPU em ~80% dos núcleos lógicos (ex: 6 de 8 threads)
        int targetThreads = (int)Math.Round(logicalCores * 0.80);
        return Math.Clamp(targetThreads, 2, 12);
    }

    public async Task<bool> EnsureServerRunningAsync(Action<string>? onStatusUpdate = null, CancellationToken cancellationToken = default)
    {
        if (_isServerVerifiedOnline && _serverProcess is { HasExited: false })
            return true;

        await _serverLock.WaitAsync(cancellationToken);
        try
        {
            if (_isServerVerifiedOnline && _serverProcess is { HasExited: false })
                return true;

            if (await IsServerHealthyAsync(cancellationToken))
            {
                _isServerVerifiedOnline = true;
                return true;
            }

            KillAllLlamafileProcesses();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string currentDir = Directory.GetCurrentDirectory();
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));

            string llamafileExe = FindFile("llamafile.exe", currentDir, baseDir, projectRoot);
            string ggufModel = FindModelFile(currentDir, baseDir, projectRoot);

            if (string.IsNullOrEmpty(llamafileExe) || string.IsNullOrEmpty(ggufModel))
            {
                onStatusUpdate?.Invoke("Arquivos do llamafile/modelo não encontrados na pasta 'llamafile/'.");
                return false;
            }

            int threads = GetOptimalThreadCount();
            string workDir = Path.GetDirectoryName(llamafileExe) ?? projectRoot;

            // 1ª Tentativa: GPU limitada em ~80% (-ngl 29) + CPU em 80%
            onStatusUpdate?.Invoke($"Iniciando Llamafile com GPU limitada a 80% (-ngl {GpuLayers80Percent}, {threads} threads)...");
            if (await TryLaunchLlamafileProcessAsync(
                llamafileExe,
                $"-m \"{ggufModel}\" --server --nobrowser --host 127.0.0.1 --port 8080 -c 2048 -b 256 -ngl {GpuLayers80Percent} -t {threads} -tb {threads} --log-disable",
                workDir,
                cancellationToken))
            {
                _isServerVerifiedOnline = true;
                onStatusUpdate?.Invoke("Servidor Llamafile online (GPU 80% ativa)!");
                return true;
            }

            // Fallback automático para CPU caso não haja GPU dedicada compatível
            KillAllLlamafileProcesses();
            onStatusUpdate?.Invoke($"GPU dedicada indisponível. Iniciando em modo CPU 80% ({threads} threads)...");
            if (await TryLaunchLlamafileProcessAsync(
                llamafileExe,
                $"-m \"{ggufModel}\" --server --nobrowser --host 127.0.0.1 --port 8080 -c 2048 -b 256 -ngl 0 --gpu disable -t {threads} -tb {threads} --log-disable",
                workDir,
                cancellationToken))
            {
                _isServerVerifiedOnline = true;
                onStatusUpdate?.Invoke("Servidor Llamafile online (Modo CPU 80%)!");
                return true;
            }

            _isServerVerifiedOnline = false;
            return false;
        }
        finally
        {
            _serverLock.Release();
        }
    }

    private async Task<bool> TryLaunchLlamafileProcessAsync(
        string llamafileExe,
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = llamafileExe,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _serverProcess = Process.Start(psi);
        if (_serverProcess != null)
        {
            try
            {
                _serverProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
            }
            catch
            {
                // Ignora caso o SO restrinja alteração de prioridade
            }
        }

        for (int attempt = 0; attempt < 35; attempt++)
        {
            if (_serverProcess is { HasExited: true })
                return false;

            await Task.Delay(300, cancellationToken);
            if (await IsServerHealthyAsync(cancellationToken))
                return true;
        }

        return false;
    }

    private async Task<bool> IsServerHealthyAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(2.0));
            using var resp = await _httpClient.GetAsync($"{_endpointUrl}/v1/models", cts.Token);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string FindFile(string fileName, params string[] roots)
    {
        foreach (string root in roots)
        {
            string candidate1 = Path.Combine(root, "llamafile", fileName);
            if (File.Exists(candidate1)) return candidate1;

            string candidate2 = Path.Combine(root, fileName);
            if (File.Exists(candidate2)) return candidate2;
        }
        return string.Empty;
    }

    private static string FindModelFile(params string[] roots)
    {
        string preferred = FindFile("Qwen2.5-3B-Instruct-abliterated.Q4_K_M.gguf", roots);
        if (!string.IsNullOrEmpty(preferred))
            return preferred;

        foreach (string root in roots)
        {
            string dir = Path.Combine(root, "llamafile");
            if (Directory.Exists(dir))
            {
                var anyGguf = Directory.GetFiles(dir, "*.gguf").OrderBy(f => new FileInfo(f).Length).FirstOrDefault();
                if (!string.IsNullOrEmpty(anyGguf))
                    return anyGguf;
            }
        }
        return string.Empty;
    }

    public async Task<LlmGeneratedCase> GenerateNewCaseAsync(
        string? customTheme = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureServerRunningAsync(null, cancellationToken);

        var (bp, isProcedural) = ProceduralCaseGenerator.GetUniqueBlueprint(_usedCaseTitles);
        _activeBlueprint = bp;

        int caseNumber = Rng.Next(101, 999);
        var generated = ProceduralCaseGenerator.BuildCaseFromBlueprint(bp, caseNumber);

        // Se o Llamafile estiver online, enriquecemos a atmosfera da sala de interrogatório com texto do modelo
        if (_isServerVerifiedOnline)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(5.5));

                string prompt = $"""
                Você é o narrador de um inquérito policial Noir em 1947.
                Descreva em exatamente 2 frases atmosféricas e poéticas o ambiente pesado e a conduta corporal de {bp.NpcName} ({bp.NpcRole}) sob a lâmpada fria da delegacia.
                Caso investigado: {bp.Title}.
                Responda apenas com a narração em 3ª pessoa, sem aspas e sem introdução.
                """;

                string enrichedGm = await RequestCleanDialogueAsync(prompt, bp.NpcName, 120, cts.Token);
                if (!string.IsNullOrWhiteSpace(enrichedGm) && enrichedGm.Length > 20 && !enrichedGm.StartsWith("{"))
                {
                    generated.OpeningGmNarration = enrichedGm.Trim();
                }

                string promptNpc = $"""
                Você interpreta o suspeito {bp.NpcName} ({bp.NpcRole}) em um interrogatório policial noir em 1947.
                Personalidade: {bp.NpcPersonality}.
                Acusação: {bp.Context}.
                Escreva exatamente uma frase em primeira pessoa de sua resposta defensiva inicial ao detetive na sala de interrogatório:
                """;

                string enrichedNpc = await RequestCleanDialogueAsync(promptNpc, bp.NpcName, 80, cts.Token);
                if (!string.IsNullOrWhiteSpace(enrichedNpc) && enrichedNpc.Length > 15 && !enrichedNpc.StartsWith("{"))
                {
                    generated.OpeningNpcDialogue = FormatWithQuotes(enrichedNpc.Trim().Trim('\"'));
                }
            }
            catch
            {
                // Mantém a narração e o diálogo procedural de alta qualidade sem travar o jogo
            }
        }

        return generated;
    }

    public async Task<LlmTurnOutput> ProcessPlayerTurnAsync(
        InterrogationState state,
        string playerInput,
        CancellationToken cancellationToken = default)
    {
        // Se a fala corresponde exatamente a uma opção da ramificação do diálogo:
        var matchingOption = state.DialogueTree.FirstOrDefault(o =>
            o.PlayerDialogue.Equals(playerInput, StringComparison.OrdinalIgnoreCase) ||
            o.Id.Equals(playerInput, StringComparison.OrdinalIgnoreCase));

        if (matchingOption != null)
        {
            return new LlmTurnOutput
            {
                GmNarration = matchingOption.GmNarration,
                NpcDialogue = matchingOption.NpcDialogue,
                NpcEmotion = matchingOption.NpcEmotion.ToString(),
                TrustDelta = matchingOption.TrustDelta,
                StressDelta = matchingOption.StressDelta,
                ProgressDelta = matchingOption.ProgressDelta,
                NewClueTitle = matchingOption.NewClueTitle,
                NewClueDescription = matchingOption.NewClueDescription
            };
        }

        if (!await EnsureServerRunningAsync(null, cancellationToken))
            throw new InvalidOperationException("Servidor Llamafile indisponível.");

        var bp = FindBlueprintForState(state);

        // 1. O Diretor C# analisa exatamente o que o jogador escreveu no chat + o estado atual
        // para determinar a reação emocional, o avanço do caso e o fato exato que o suspeito deve abordar.
        var turnPlan = AnalyzePlayerTurn(state, bp, playerInput);

        // 2. O modelo 3B gera a fala conversacional respondendo diretamente à pergunta do jogador
        // guiado pela diretriz de 1ª pessoa do turno (sem precisar gerenciar 12 campos JSON).
        string npcDialogue = turnPlan.DirectSpeechFallback;
        try
        {
            string recentExchange = BuildRecentDialogueContext(state);
            string userPrompt = turnPlan.IsFreeformConversational
                ? $$"""
                Estamos em um diálogo de interrogatório policial noir em Português do Brasil.
                Você interpreta o suspeito {{state.NpcName}} ({{state.NpcRole}}).
                Personalidade: {{state.NpcPersonality}}
                Contexto do caso: {{bp.Context}}
                Sua versão atual: {{bp.AlibiSpeech}}
                {{recentExchange}}
                Agora o Detetive disse/perguntou: "{{playerInput}}"

                Escreva APENAS a fala de resposta de {{state.NpcName}} em primeira pessoa (1 a 2 frases curtas), respondendo diretamente ao que o Detetive acabou de perguntar ou comentar, mantendo seu personagem noir:
                """
                : $$"""
                Estamos em um diálogo de interrogatório policial noir em Português do Brasil.
                Suspeito interrogado: {{state.NpcName}} ({{state.NpcRole}}).
                Crime: {{bp.Context}}
                {{recentExchange}}
                Agora o Detetive disse: "{{playerInput}}"

                Escreva APENAS a resposta direta de {{state.NpcName}} para o Detetive em primeira pessoa (1 a 2 frases curtas).
                Na resposta, {{state.NpcName}} responde ao Detetive usando esta ideia: "{{turnPlan.FirstPersonCoreArgument}}"
                Resposta de {{state.NpcName}}:
                """;

            string rawReply = await RequestCleanDialogueAsync(userPrompt, state.NpcName, maxTokens: 75, cancellationToken);
            if (IsCoherentReply(rawReply, state.NpcName) && !IsEchoingPlayer(rawReply, playerInput))
            {
                npcDialogue = FormatWithQuotes(EnsureContainsKeyFact(rawReply, turnPlan));
            }
        }
        catch
        {
            // Usa o fallback conversacional preparado pelo Diretor Noir
        }

        var output = new LlmTurnOutput
        {
            GmNarration = turnPlan.GmNarration,
            NpcDialogue = npcDialogue,
            NpcEmotion = turnPlan.NextEmotion.ToString(),
            TrustDelta = turnPlan.TrustDelta,
            StressDelta = turnPlan.StressDelta,
            ProgressDelta = turnPlan.ProgressDelta,
            NewClueTitle = turnPlan.UnlockedClueTitle,
            NewClueDescription = turnPlan.UnlockedClueDescription
        };

        output.SuggestedPrompts = BuildSmartSuggestionsForTurn(state, bp, output);
        return output;
    }

    /// <summary>
    /// Envia a instrução usando "You are a helpful assistant." no papel system e cache_prompt = false,
    /// o que evita a corrupção de atenção dos modelos Qwen2.5-3B-abliterated e o bug de KV-cache híbrido do llamafile.
    /// </summary>
    private async Task<string> RequestCleanDialogueAsync(
        string userPrompt,
        string npcName,
        int maxTokens,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = "LLaMA_CPP",
            temperature = 0.28,
            top_p = 0.85,
            repeat_penalty = 1.04,
            max_tokens = maxTokens,
            cache_prompt = false,
            messages = new object[]
            {
                new { role = "system", content = "You are a helpful assistant." },
                new { role = "user", content = userPrompt }
            }
        };

        string jsonRequest = JsonSerializer.Serialize(payload);
        using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        try
        {
            using var response = await _httpClient.PostAsync($"{_endpointUrl}/v1/chat/completions", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseBody);

            string? raw = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return SanitizeNpcDialogue(raw ?? string.Empty, npcName);
        }
        catch
        {
            _isServerVerifiedOnline = false;
            throw;
        }
    }

    private static string BuildRecentDialogueContext(InterrogationState state)
    {
        var lastRecord = state.TurnRecords.LastOrDefault();
        if (lastRecord == null)
            return string.Empty;

        string prevNpc = lastRecord.NpcDialogue.Trim('"');
        if (!string.IsNullOrWhiteSpace(lastRecord.PlayerAction))
        {
            return $"Turno anterior - Detetive: \"{lastRecord.PlayerAction}\" | {state.NpcName}: \"{prevNpc}\"";
        }
        return $"Abertura - {state.NpcName}: \"{prevNpc}\"";
    }

    private static string SanitizeNpcDialogue(string raw, string npcName)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        string text = raw.Trim();

        // Remove blocos markdown ou tags se houver
        text = Regex.Replace(text, @"^```[a-zA-Z]*\s*|\s*```$", "", RegexOptions.Multiline).Trim();

        // Pega apenas a primeira linha/parágrafo não vazio relevante
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l) &&
                        !l.StartsWith("NARRAÇÃO", StringComparison.OrdinalIgnoreCase) &&
                        !l.StartsWith("AÇÃO", StringComparison.OrdinalIgnoreCase) &&
                        !l.StartsWith("Detetive:", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (lines.Count > 0)
            text = string.Join(" ", lines);

        // Remove prefixos como "Resposta de Vicente Moretti:", "Vicente Moretti:", "FALA:", "Eu:"
        string escapedName = Regex.Escape(npcName);
        string firstName = Regex.Escape(npcName.Split(' ')[0]);
        text = Regex.Replace(text, $@"^(?:Resposta\s+de\s+{escapedName}|Fala\s+de\s+{escapedName}|{escapedName}|{firstName}|Suspeito|FALA|Eu)\s*[:,]\s*", "", RegexOptions.IgnoreCase).Trim();
        text = Regex.Replace(text, @"^.*?respondeu\s+em\s+primeira\s+pessoa\s*:\s*", "", RegexOptions.IgnoreCase).Trim();

        // Remove aspas externas duplicadas para padronizar depois
        text = text.Trim('"', '\'', '“', '”', ' ');

        // Limita a no máximo 2 frases completas para cortar qualquer divagação no final da geração
        text = KeepAtMostTwoSentences(text);

        return text.Trim();
    }

    private static string KeepAtMostTwoSentences(string text)
    {
        var matches = Regex.Matches(text, @"[^.!?]+[.!?]+");
        if (matches.Count >= 2)
        {
            return (matches[0].Value + " " + matches[1].Value).Trim();
        }
        if (matches.Count == 1)
        {
            return matches[0].Value.Trim();
        }
        return text;
    }

    private static bool IsCoherentReply(string cleanedText, string npcName)
    {
        if (string.IsNullOrWhiteSpace(cleanedText) || cleanedText.Length < 18 || cleanedText.Length > 340)
            return false;

        // Rejeita vazamentos de instruções ou erros gramaticais típicos de alucinação
        string[] badPatterns =
        {
            "Título Curto", "primeira pessoa", "Diretriz", "Obrigado pelo casamento",
            "Obrigado pelo isqueiro", "Obrigado pelo envelope", "Eu atirou", "Eu matou",
            "sopro da lua", "câmera estava aceso", "Detetive disse:", "Escreva APENAS",
            "<", ">", "{", "}"
        };

        foreach (string bad in badPatterns)
        {
            if (cleanedText.Contains(bad, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Não deve falar de si mesmo em 3ª pessoa pelo nome completo
        if (cleanedText.Contains(npcName, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    private static bool IsEchoingPlayer(string reply, string playerInput)
    {
        string cleanReply = reply.Trim('"', '\'', ' ').ToLowerInvariant();
        string cleanInput = playerInput.Trim('"', '\'', ' ').ToLowerInvariant();
        if (cleanInput.Length > 15 && cleanReply.Contains(cleanInput))
            return true;
        return false;
    }

    private static string EnsureContainsKeyFact(string llmReply, TurnPlan plan)
    {
        string clean = llmReply.Trim('"', ' ');
        if (plan.RequireExactFact && !string.IsNullOrWhiteSpace(plan.MustContainKeyword))
        {
            if (!clean.Contains(plan.MustContainKeyword, StringComparison.OrdinalIgnoreCase))
            {
                string firstSentence = Regex.Match(clean, @"^[^.!?]+[.!?]").Value.Trim();
                if (!string.IsNullOrWhiteSpace(firstSentence) &&
                    !plan.FirstPersonCoreArgument.StartsWith(firstSentence, StringComparison.OrdinalIgnoreCase))
                {
                    return $"{firstSentence} {plan.FirstPersonCoreArgument}";
                }
                return plan.FirstPersonCoreArgument;
            }
        }
        return clean;
    }

    private static string FormatWithQuotes(string text)
    {
        string trimmed = text.Trim().Trim('"', '“', '”');
        return $"\"{trimmed}\"";
    }

    private NoirCaseBlueprint FindBlueprintForState(InterrogationState state)
    {
        if (_activeBlueprint != null)
            return _activeBlueprint;

        var (bp, _) = ProceduralCaseGenerator.GetUniqueBlueprint();
        _activeBlueprint = bp;
        return bp;
    }

    private static TurnPlan AnalyzePlayerTurn(InterrogationState state, NoirCaseBlueprint bp, string playerInput)
    {
        string lower = playerInput.ToLowerInvariant();
        string firstName = state.NpcName.Split(' ')[0];

        bool isAggressive = ContainsAny(lower, "bater", "bato", "mesa", "grit", "cadeia", "prisão", "prisao", "apodrecer", "ameaç", "ameac", "cala a boca", "mentiroso", "mentira", "desgraçado", "vagabundo", "assassino");
        bool isEmpathetic = ContainsAny(lower, "acordo", "proteção", "protecao", "proteger", "ajudar", "ajudo", "redução", "reducao", "pena", "calma", "água", "agua", "cigarro", "café", "cafe", "entendo", "compreendo", "inocente", "forçado", "forcado", "medo");
        bool mentionsWeakPoint = ContainsAny(lower, bp.WeakPointKeywords);
        bool asksWhoOrWhere = ContainsAny(lower, "quem matou", "quem atirou", "quem mandou", "mandante", "cúmplice", "cumplice", "chefe", "onde está", "onde esta", "onde escondeu", "esconderijo", "cadê", "cade", "confess");
        bool asksIdentityOrAlibi = ContainsAny(lower, "quem é você", "quem e voce", "o que você faz", "o que voce faz", "onde você estava", "onde voce estava", "álibi", "alibi", "meia-noite", "horário", "horario", "conhecia", "versão", "versao");

        // Verifica se o jogador mencionou alguma das 3 pistas do dossiê (ou a 4ª pista desbloqueada)
        ClueBlueprint? matchedClue = null;
        for (int i = 0; i < bp.Clues.Length; i++)
        {
            var c = bp.Clues[i];
            if (lower.Contains(c.Title.ToLowerInvariant()) || ContainsAny(lower, c.Keywords))
            {
                matchedClue = c;
                break;
            }
        }

        bool mentionsUnlockedClue = !string.IsNullOrWhiteSpace(bp.BonusClueTitle) &&
                                    (lower.Contains(bp.BonusClueTitle.ToLowerInvariant()) || ContainsAny(lower, bp.BonusClueKeywords));

        // 1. JOGADOR TOCOU NO PONTO FRACO OU OFERECEU ACORDO DE PROTEÇÃO
        if (mentionsWeakPoint || (isEmpathetic && state.CaseProgress >= 50))
        {
            int progDelta = state.CaseProgress >= 65 ? 40 : 30;
            bool willConfessNow = state.CaseProgress + progDelta >= 100;

            if (willConfessNow)
            {
                return new TurnPlan
                {
                    TrustDelta = +20,
                    StressDelta = -5,
                    ProgressDelta = 100 - state.CaseProgress,
                    NextEmotion = EmotionalState.Cooperativo,
                    GmNarration = $"{state.NpcName} desaba os ombros sobre a mesa metálica, vencido pelo medo e pela promessa de proteção, decidindo revelar toda a verdade.",
                    FirstPersonCoreArgument = bp.ConfessionSpeech,
                    DirectSpeechFallback = FormatWithQuotes(bp.ConfessionSpeech),
                    RequireExactFact = true,
                    MustContainKeyword = bp.KeyConfessionWord
                };
            }

            bool unlockNow = !state.Clues.Any(c => c.Title.Equals(bp.BonusClueTitle, StringComparison.OrdinalIgnoreCase));
            return new TurnPlan
            {
                TrustDelta = +18,
                StressDelta = -5,
                ProgressDelta = progDelta,
                NextEmotion = EmotionalState.Cooperativo,
                GmNarration = $"A menção a {bp.WeakPointLabel} atinge {firstName} em cheio; ele engole em seco e baixa a guarda pela primeira vez na noite.",
                FirstPersonCoreArgument = bp.WeakPointReactionSpeech,
                DirectSpeechFallback = FormatWithQuotes(bp.WeakPointReactionSpeech),
                UnlockedClueTitle = unlockNow ? bp.BonusClueTitle : null,
                UnlockedClueDescription = unlockNow ? bp.BonusClueDescription : null,
                RequireExactFact = true,
                MustContainKeyword = bp.KeyAccompliceWord
            };
        }

        // 2. JOGADOR PRESSIONOU PELA CONFISSÃO FINAL / ESCONDERIJO / MANDANTE (OU USOU A 4ª PISTA DESBLOQUEADA)
        if (mentionsUnlockedClue || (asksWhoOrWhere && state.CaseProgress >= 55))
        {
            int progDelta = 38;
            bool willConfessNow = state.CaseProgress + progDelta >= 95;
            return new TurnPlan
            {
                TrustDelta = isAggressive ? -5 : +10,
                StressDelta = isAggressive ? +15 : +8,
                ProgressDelta = willConfessNow ? Math.Max(progDelta, 100 - state.CaseProgress) : progDelta,
                NextEmotion = EmotionalState.Acuado,
                GmNarration = $"Sem saída diante das provas acumuladas, {state.NpcName} passa a mão trêmula pelo rosto suado e finalmente abre o jogo.",
                FirstPersonCoreArgument = bp.ConfessionSpeech,
                DirectSpeechFallback = FormatWithQuotes(bp.ConfessionSpeech),
                RequireExactFact = true,
                MustContainKeyword = bp.KeyConfessionWord
            };
        }

        // 3. JOGADOR CONFRONTOU COM UMA PISTA ESPECÍFICA DO DOSSIÊ
        if (matchedClue != null)
        {
            bool cracksOnClue = state.CaseProgress >= 45 || state.Stress >= 55 || state.CurrentTurn >= 4;
            bool unlockNow = cracksOnClue && !state.Clues.Any(c => c.Title.Equals(bp.BonusClueTitle, StringComparison.OrdinalIgnoreCase));

            if (cracksOnClue)
            {
                int progDelta = 28;
                bool willConfessNow = state.CaseProgress + progDelta >= 100;
                string coreSpeech = willConfessNow ? bp.ConfessionSpeech : matchedClue.AdmissionSpeech;

                return new TurnPlan
                {
                    TrustDelta = isAggressive ? -8 : +5,
                    StressDelta = isAggressive ? +18 : +12,
                    ProgressDelta = progDelta,
                    NextEmotion = EmotionalState.Acuado,
                    GmNarration = $"Confrontado diretamente com a prova '{matchedClue.Title}', {firstName} perde a firmeza na voz e deixa escapar parte do que escondia.",
                    FirstPersonCoreArgument = coreSpeech,
                    DirectSpeechFallback = FormatWithQuotes(coreSpeech),
                    UnlockedClueTitle = unlockNow ? bp.BonusClueTitle : null,
                    UnlockedClueDescription = unlockNow ? bp.BonusClueDescription : null,
                    RequireExactFact = true,
                    MustContainKeyword = willConfessNow ? bp.KeyConfessionWord : matchedClue.Keywords[0]
                };
            }
            else
            {
                return new TurnPlan
                {
                    TrustDelta = isAggressive ? -10 : 0,
                    StressDelta = isAggressive ? +18 : +10,
                    ProgressDelta = 20,
                    NextEmotion = EmotionalState.Defensivo,
                    GmNarration = $"{state.NpcName} fixa o olhar na evidência '{matchedClue.Title}' sobre a mesa e responde rápido, tentando sustentar sua versão.",
                    FirstPersonCoreArgument = matchedClue.ExcuseSpeech,
                    DirectSpeechFallback = FormatWithQuotes(matchedClue.ExcuseSpeech),
                    RequireExactFact = true,
                    MustContainKeyword = matchedClue.Keywords[0]
                };
            }
        }

        // 4. JOGADOR ADOTOU TOM EMPÁTICO / OFERECEU ACORDO / CIGARRO / ÁGUA NO INÍCIO
        if (isEmpathetic)
        {
            return new TurnPlan
            {
                TrustDelta = +15,
                StressDelta = -10,
                ProgressDelta = +20,
                NextEmotion = EmotionalState.Calmo,
                GmNarration = $"{state.NpcName} respira mais fundo com a sua abordagem menos hostil, inclinando-se sobre a mesa para conversar.",
                FirstPersonCoreArgument = bp.EmpathyEarlySpeech,
                DirectSpeechFallback = FormatWithQuotes(bp.EmpathyEarlySpeech),
                RequireExactFact = false
            };
        }

        // 5. JOGADOR FOI AGRESSIVO / BATEU NA MESA / AMEAÇOU SEM PROVA ESPECÍFICA
        if (isAggressive)
        {
            bool highStress = state.Stress + 22 >= 75;
            return new TurnPlan
            {
                TrustDelta = -15,
                StressDelta = +22,
                ProgressDelta = +10,
                NextEmotion = highStress ? EmotionalState.Acuado : EmotionalState.Hostil,
                GmNarration = $"O impacto da sua pressão faz {state.NpcName} fechar a expressão e crispar os dedos na borda da cadeira.",
                FirstPersonCoreArgument = highStress
                    ? $"Gritar comigo desse jeito só vai me fazer exigir meu advogado agora mesmo! Se quer que eu fale sobre {bp.KeyAccompliceWord}, precisa garantir que não vou morrer na cela!"
                    : $"Bater na mesa e me ameaçar não vai colar comigo, detetive! {bp.AlibiSpeech}",
                DirectSpeechFallback = FormatWithQuotes(highStress
                    ? $"Gritar comigo desse jeito só vai me fazer exigir meu advogado agora mesmo! Se quer que eu fale sobre {bp.KeyAccompliceWord}, precisa garantir que não vou morrer na cela!"
                    : $"Bater na mesa e me ameaçar não vai colar comigo, detetive! {bp.AlibiSpeech}"),
                RequireExactFact = false
            };
        }

        // 6. JOGADOR PERGUNTOU SOBRE QUEM MATOU / ESCONDERIJO CEDO DEMAIS
        if (asksWhoOrWhere)
        {
            return new TurnPlan
            {
                TrustDelta = +4,
                StressDelta = +12,
                ProgressDelta = +18,
                NextEmotion = EmotionalState.Nervoso,
                GmNarration = $"{state.NpcName} olha de relance para a porta trancada da sala de interrogatório, claramente temendo quem está lá fora.",
                FirstPersonCoreArgument = bp.EarlyWhoWhereSpeech,
                DirectSpeechFallback = FormatWithQuotes(bp.EarlyWhoWhereSpeech),
                RequireExactFact = false
            };
        }

        // 7. JOGADOR PERGUNTOU ESPECIFICAMENTE SOBRE ÁLIBI / IDENTIDADE / ONDE ESTAVA
        if (asksIdentityOrAlibi)
        {
            return new TurnPlan
            {
                TrustDelta = +6,
                StressDelta = +6,
                ProgressDelta = +15,
                NextEmotion = EmotionalState.Calmo,
                GmNarration = $"{state.NpcName} ajeita a postura na cadeira sob a luz baixa da sala e responde diretamente à sua pergunta.",
                FirstPersonCoreArgument = bp.AlibiSpeech,
                DirectSpeechFallback = FormatWithQuotes(bp.AlibiSpeech),
                RequireExactFact = false
            };
        }

        // 8. CONVERSA LIVRE NO CHAT (Qualquer comentário ou pergunta aberta feita pelo jogador)
        return new TurnPlan
        {
            TrustDelta = +5,
            StressDelta = +8,
            ProgressDelta = +16,
            NextEmotion = EmotionalState.Defensivo,
            GmNarration = $"{state.NpcName} escuta o que você disse com atenção tensa na penumbra da delegacia e responde em seguida.",
            FirstPersonCoreArgument = bp.AlibiSpeech,
            DirectSpeechFallback = FormatWithQuotes($"Olha, detetive, sobre isso... {bp.AlibiSpeech}"),
            RequireExactFact = false,
            IsFreeformConversational = true
        };
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        foreach (string kw in keywords)
        {
            if (!string.IsNullOrWhiteSpace(kw) && text.Contains(kw, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static List<string> BuildSmartSuggestionsForNewCase(LlmGeneratedCase generated, NoirCaseBlueprint bp)
    {
        return new List<string>
        {
            $"Confrontar {generated.NpcName} sobre a pista '{bp.Clues[0].Title}'",
            $"Exigir explicação para a pista '{bp.Clues[1].Title}'",
            $"Oferecer acordo de proteção se {generated.NpcName.Split(' ')[0]} colaborar agora"
        };
    }

    private static List<string> BuildSmartSuggestionsForTurn(InterrogationState state, NoirCaseBlueprint bp, LlmTurnOutput output)
    {
        var suggestions = new List<string>();

        if (!string.IsNullOrWhiteSpace(output.NewClueTitle))
        {
            suggestions.Add($"Pressionar com a nova pista '{output.NewClueTitle}' para fechar o cerco");
        }
        else if (state.Clues.Count > 0)
        {
            var clue = state.Clues[state.CurrentTurn % state.Clues.Count];
            suggestions.Add($"Questionar diretamente sobre '{clue.Title}': {clue.Description}");
        }

        int nextStress = state.Stress + output.StressDelta;
        int nextProgress = state.CaseProgress + output.ProgressDelta;

        if (nextStress >= 68)
        {
            suggestions.Add($"Acalmar {state.NpcName.Split(' ')[0]} e prometer proteção para {bp.WeakPointLabel} se entregar o mandante");
        }
        else
        {
            suggestions.Add($"Tocar no ponto fraco ({bp.WeakPointLabel}) e oferecer proteção policial em troca da verdade");
        }

        if (nextProgress >= 60)
        {
            suggestions.Add($"Exigir o nome do verdadeiro culpado e onde escondeu {bp.HiddenObjectLabel}");
        }
        else
        {
            var altClue = state.Clues[(state.CurrentTurn + 1) % state.Clues.Count];
            suggestions.Add($"Apontar a contradição na pista '{altClue.Title}'");
        }

        return suggestions.Take(3).ToList();
    }

    private static List<DialogueOption> BuildDialogueTreeForBlueprint(NoirCaseBlueprint bp)
    {
        return ProceduralCaseGenerator.BuildDialogueTreeForBlueprint(bp);
    }

    public void Dispose()
    {
        KillAllLlamafileProcesses();
        _serverLock.Dispose();
        _httpClient.Dispose();
    }

    public void KillAllLlamafileProcesses()
    {
        _isServerVerifiedOnline = false;
        try
        {
            if (_serverProcess is { HasExited: false })
            {
                _serverProcess.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Ignora
        }

        try
        {
            foreach (var proc in Process.GetProcessesByName("llamafile"))
            {
                try
                {
                    proc.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Ignora
                }
            }
        }
        catch
        {
            // Ignora
        }
    }

    private sealed class TurnPlan
    {
        public int TrustDelta { get; init; }
        public int StressDelta { get; init; }
        public int ProgressDelta { get; init; }
        public EmotionalState NextEmotion { get; init; }
        public string GmNarration { get; init; } = string.Empty;
        public string FirstPersonCoreArgument { get; init; } = string.Empty;
        public string DirectSpeechFallback { get; init; } = string.Empty;
        public string? UnlockedClueTitle { get; init; }
        public string? UnlockedClueDescription { get; init; }
        public bool RequireExactFact { get; init; }
        public string? MustContainKeyword { get; init; }
        public bool IsFreeformConversational { get; init; }
    }
}
