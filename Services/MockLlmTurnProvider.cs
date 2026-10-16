using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LlamaInterrogation.Models;

namespace LlamaInterrogation.Services;

/// <summary>
/// Simulador local caso o llamafile esteja desligado.
/// </summary>
public class MockLlmTurnProvider : ILlmTurnProvider
{
    private static readonly Random Rng = new();
    public string ProviderName => "Simulador Local (Fallback)";

    private readonly HashSet<string> _usedCaseTitles = new(StringComparer.OrdinalIgnoreCase);

    public async Task<LlmGeneratedCase> GenerateNewCaseAsync(
        string? customTheme = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(250, cancellationToken);
        var (bp, isProcedural) = ProceduralCaseGenerator.GetUniqueBlueprint(_usedCaseTitles);
        int caseNumber = Rng.Next(101, 999);
        return ProceduralCaseGenerator.BuildCaseFromBlueprint(bp, caseNumber);
    }

    public async Task<LlmTurnOutput> ProcessPlayerTurnAsync(
        InterrogationState state,
        string playerInput,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(350, cancellationToken);

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

        string lower = playerInput.ToLowerInvariant();

        if (lower.Contains("filha") || lower.Contains("clínica") || lower.Contains("clinica") ||
            lower.Contains("dívida") || lower.Contains("divida") || lower.Contains("acordo") || lower.Contains("prote"))
        {
            return new LlmTurnOutput
            {
                GmNarration = $"{state.NpcName} hesita, sentindo o peso das suas palavras tocar exatamente no ponto fraco que tentava esconder.",
                NpcDialogue = "\"Se eu abrir o bico aqui, você garante mesmo proteção?! Tudo bem... eu não agi por ambição, fui pressionado por gente muito maior que eu!\"",
                NpcEmotion = "Cooperativo",
                TrustDelta = +20,
                StressDelta = +5,
                ProgressDelta = +35,
                NewClueTitle = "Mandante Oculto Confirmado",
                NewClueDescription = "O suspeito admitiu que recebeu ordens diretas de um superior em troca de dinheiro.",
                SuggestedPrompts = new List<string>
                {
                    "Exigir a localização exata do objeto/prova principal agora mesmo",
                    "Perguntar o nome do mandante para fechar o acordo",
                    "Pressionar pelos detalhes finais para assinar a confissão"
                }
            };
        }

        if (lower.Contains("bater") || lower.Contains("mesa") || lower.Contains("grit") || lower.Contains("cadeia"))
        {
            return new LlmTurnOutput
            {
                GmNarration = $"O impacto na mesa faz {state.NpcName} franzir o cenho com hostilidade, cruzando os braços.",
                NpcDialogue = "\"Gritar não vai te dar respostas, detetive! Se continuar com esse showzinho barato, vou esperar meu advogado calado!\"",
                NpcEmotion = "Hostil",
                TrustDelta = -15,
                StressDelta = +25,
                ProgressDelta = +5,
                SuggestedPrompts = new List<string>
                {
                    "Mudar de tática e apresentar uma das pistas do dossiê com frieza",
                    "Oferecer uma saída negociada caso coopere agora",
                    "Apontar a contradição nos registros de horário"
                }
            };
        }

        return new LlmTurnOutput
        {
            GmNarration = $"{state.NpcName} observa sua reação com cautela, medindo o quanto você realmente sabe sobre o caso.",
            NpcDialogue = $"\"Sobre '{Truncate(playerInput, 45)}'... você está chutando no escuro, detetive. Mostre-me provas reais ou me deixe ir.\"",
            NpcEmotion = "Defensivo",
            TrustDelta = +5,
            StressDelta = +10,
            ProgressDelta = +20,
            SuggestedPrompts = new List<string>
            {
                "Confrontar diretamente com as pistas do dossiê",
                "Propor um acordo de proteção se entregar o esconderijo",
                "Fazer uma pergunta inesperada para desestabilizar o suspeito"
            }
        };
    }

    private static string Truncate(string value, int maxChars) =>
        value.Length <= maxChars ? value : value[..maxChars] + "...";
}
