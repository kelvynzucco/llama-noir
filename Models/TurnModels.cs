using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LlamaInterrogation.Models;

public enum EmotionalState
{
    Calmo,
    Defensivo,
    Nervoso,
    Hostil,
    Cooperativo,
    Acuado
}

public enum GameOutcome
{
    EmAndamento,
    VitoriaConfissao,
    VitoriaInocencia,
    DerrotaCoacaoInocente,
    DerrotaSilencioTotal,
    DerrotaTempoEsgotado
}

public class ClueItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsNew { get; set; }
}

public enum LogEntryType
{
    SystemBriefing,
    PlayerAction,
    GmNarration,
    NpcDialogue,
    StateFeedback
}

public class ChatLogEntry
{
    public LogEntryType Type { get; set; }
    public string Speaker { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Turn { get; set; }
}

/// <summary>
/// Perfil visual gerado pelo LLM para customizar o retrato procedural do suspeito a cada novo caso.
/// </summary>
public class NpcAppearance
{
    [JsonPropertyName("skin_tone")]
    public string SkinTone { get; set; } = "media"; // clara, media, morena, escura

    [JsonPropertyName("hair_color")]
    public string HairColor { get; set; } = "grisalho"; // grisalho, preto, castanho, loiro, ruivo

    [JsonPropertyName("hair_style")]
    public string HairStyle { get; set; } = "curto"; // curto, longo, calvo, topete

    [JsonPropertyName("outfit_style")]
    public string OutfitStyle { get; set; } = "jaleco"; // jaleco, terno, jaqueta, uniforme, vinho

    [JsonPropertyName("has_glasses")]
    public bool HasGlasses { get; set; } = true;

    [JsonPropertyName("has_beard")]
    public bool HasBeard { get; set; } = false;
}

public enum ClueSynergyType
{
    None,
    Positive,
    Negative
}

/// <summary>
/// Representa uma opção concreta de fala/diálogo do Detetive com toda sua ramificação de consequências.
/// </summary>
public class DialogueOption
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = "Confronto"; // Confronto, Empatia, Pressao, Revelacao, Final

    [JsonPropertyName("tag")]
    public string Tag { get; set; } = "[CONFRONTO]";

    [JsonPropertyName("player_dialogue")]
    public string PlayerDialogue { get; set; } = string.Empty;

    [JsonPropertyName("intent_summary")]
    public string IntentSummary { get; set; } = string.Empty;

    [JsonPropertyName("gm_narration")]
    public string GmNarration { get; set; } = string.Empty;

    [JsonPropertyName("npc_dialogue")]
    public string NpcDialogue { get; set; } = string.Empty;

    [JsonPropertyName("npc_emotion")]
    public EmotionalState NpcEmotion { get; set; } = EmotionalState.Defensivo;

    [JsonPropertyName("trust_delta")]
    public int TrustDelta { get; set; }

    [JsonPropertyName("stress_delta")]
    public int StressDelta { get; set; }

    [JsonPropertyName("progress_delta")]
    public int ProgressDelta { get; set; }

    [JsonPropertyName("new_clue_title")]
    public string? NewClueTitle { get; set; }

    [JsonPropertyName("new_clue_description")]
    public string? NewClueDescription { get; set; }

    [JsonPropertyName("required_min_progress")]
    public int RequiredMinProgress { get; set; } = 0;

    [JsonPropertyName("required_clue_title")]
    public string? RequiredClueTitle { get; set; }

    [JsonPropertyName("related_clue_title")]
    public string? RelatedClueTitle { get; set; }

    [JsonPropertyName("synergistic_keywords")]
    public List<string> SynergisticKeywords { get; set; } = new();

    [JsonPropertyName("prerequisite_option_id")]
    public string? PrerequisiteOptionId { get; set; }

    [JsonIgnore]
    public bool IsUsed { get; set; } = false;

    public bool IsClueCompatible(ClueItem? clue)
    {
        if (clue == null || string.IsNullOrWhiteSpace(clue.Title))
            return false;

        if (!string.IsNullOrEmpty(RelatedClueTitle) &&
            (RelatedClueTitle.Contains(clue.Title, StringComparison.OrdinalIgnoreCase) ||
             clue.Title.Contains(RelatedClueTitle, StringComparison.OrdinalIgnoreCase)))
            return true;

        if (!string.IsNullOrEmpty(RequiredClueTitle) &&
            (RequiredClueTitle.Contains(clue.Title, StringComparison.OrdinalIgnoreCase) ||
             clue.Title.Contains(RequiredClueTitle, StringComparison.OrdinalIgnoreCase)))
            return true;

        if (!string.IsNullOrEmpty(Tag) && Tag.Contains(clue.Title, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.IsNullOrEmpty(PlayerDialogue) && PlayerDialogue.Contains(clue.Title, StringComparison.OrdinalIgnoreCase))
            return true;

        if (SynergisticKeywords is { Count: > 0 })
        {
            foreach (var kw in SynergisticKeywords)
            {
                if (clue.Title.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    clue.Description.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        // Casamento por palavras-chave significativas do título da pista (ex.: "Espora", "Sangue", "Disjuntor")
        var clueWords = clue.Title.Split(new[] { ' ', '-', '•', ':', ',', '\'', '"' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in clueWords)
        {
            if (word.Length >= 4 &&
                !word.Equals("para", StringComparison.OrdinalIgnoreCase) &&
                !word.Equals("com", StringComparison.OrdinalIgnoreCase) &&
                !word.Equals("pela", StringComparison.OrdinalIgnoreCase) &&
                !word.Equals("pelo", StringComparison.OrdinalIgnoreCase))
            {
                if (Tag.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    PlayerDialogue.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        string[] commonKeywords = {
            "isqueiro", "carro", "sedã", "seda", "02:14", "02h10", "02h15", "vigia", "placa",
            "envelope", "dinheiro", "sanatório", "sanatorio", "clara", "irmã", "irma", "lucia", "lúcia",
            "adega", "chave", "garrafa", "bordeaux", "cianeto", "veneno", "cinzas", "lareira",
            "crachá", "cracha", "núcleo", "nucleo", "câmeras", "cameras", "piano", "livro-caixa",
            "dívida", "divida", "cassino", "riviera", "espora", "seringa", "disjuntor", "blecaute",
            "baile", "mármore", "estátua", "estatua", "bota", "jóquei", "joquei", "hipódromo", "hipodromo"
        };

        foreach (var kw in commonKeywords)
        {
            bool inOption = Tag.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                            PlayerDialogue.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                            IntentSummary.Contains(kw, StringComparison.OrdinalIgnoreCase);

            bool inClue = clue.Title.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                          clue.Description.Contains(kw, StringComparison.OrdinalIgnoreCase);

            if (inOption && inClue)
                return true;
        }

        return false;
    }
}

/// <summary>
/// Estrutura JSON retornada pelo Llamafile ao criar um Novo Caso proceduralmente.
/// </summary>
public class LlmGeneratedCase
{
    [JsonPropertyName("case_title")]
    public string CaseTitle { get; set; } = string.Empty;

    [JsonPropertyName("case_objective")]
    public string CaseObjective { get; set; } = string.Empty;

    [JsonPropertyName("npc_name")]
    public string NpcName { get; set; } = string.Empty;

    [JsonPropertyName("npc_role")]
    public string NpcRole { get; set; } = string.Empty;

    [JsonPropertyName("npc_personality")]
    public string NpcPersonality { get; set; } = string.Empty;

    [JsonPropertyName("secret_truth")]
    public string SecretTruth { get; set; } = string.Empty;

    [JsonPropertyName("appearance")]
    public NpcAppearance Appearance { get; set; } = new();

    [JsonPropertyName("initial_clues")]
    public List<ClueItem> InitialClues { get; set; } = new();

    [JsonPropertyName("opening_gm_narration")]
    public string OpeningGmNarration { get; set; } = string.Empty;

    [JsonPropertyName("opening_npc_dialogue")]
    public string OpeningNpcDialogue { get; set; } = string.Empty;

    [JsonPropertyName("initial_suggestions")]
    public List<string> InitialSuggestions { get; set; } = new();

    [JsonPropertyName("is_guilty")]
    public bool IsGuilty { get; set; } = true;

    [JsonPropertyName("dialogue_tree")]
    public List<DialogueOption> DialogueTree { get; set; } = new();
}

/// <summary>
/// Representa um bloco completo de um Turno para exibição visual estruturada e orgânica.
/// </summary>
public class TurnRecord
{
    public int TurnNumber { get; set; }
    public bool IsOpeningScene { get; set; }
    public string? PlayerAction { get; set; }
    public string GmNarration { get; set; } = string.Empty;
    public string NpcDialogue { get; set; } = string.Empty;
    public EmotionalState NpcEmotion { get; set; } = EmotionalState.Calmo;
    public int TrustDelta { get; set; }
    public int StressDelta { get; set; }
    public int ProgressDelta { get; set; }
    public string? UnlockedClueTitle { get; set; }
    public string? AttachedClueTitle { get; set; }
    public ClueSynergyType ClueSynergy { get; set; } = ClueSynergyType.None;
    public string? SystemConclusionTitle { get; set; }
    public string? SystemConclusionText { get; set; }
    public bool IsVictoryConclusion { get; set; }
}

/// <summary>
/// Estrutura JSON que o Llamafile (atuando como Mestre do Jogo + NPC) retorna a cada turno.
/// </summary>
public class LlmTurnOutput
{
    [JsonPropertyName("gm_narration")]
    public string GmNarration { get; set; } = string.Empty;

    [JsonPropertyName("npc_dialogue")]
    public string NpcDialogue { get; set; } = string.Empty;

    [JsonPropertyName("npc_emotion")]
    public string NpcEmotion { get; set; } = "Calmo";

    [JsonPropertyName("trust_delta")]
    public int TrustDelta { get; set; }

    [JsonPropertyName("stress_delta")]
    public int StressDelta { get; set; }

    [JsonPropertyName("progress_delta")]
    public int ProgressDelta { get; set; }

    [JsonPropertyName("new_clue_title")]
    public string? NewClueTitle { get; set; }

    [JsonPropertyName("new_clue_description")]
    public string? NewClueDescription { get; set; }

    [JsonPropertyName("attached_clue_title")]
    public string? AttachedClueTitle { get; set; }

    [JsonPropertyName("clue_synergy")]
    public ClueSynergyType ClueSynergy { get; set; } = ClueSynergyType.None;

    [JsonPropertyName("suggested_prompts")]
    public List<string> SuggestedPrompts { get; set; } = new();
}

public class InterrogationState
{
    public int CurrentTurn { get; set; } = 1;
    public int MaxTurns { get; set; } = 10;

    public string NpcName { get; set; } = "Dr. Viktor Vance";
    public string NpcRole { get; set; } = "Engenheiro-Chefe de Segurança • Laboratório Aegis";
    public string NpcPersonality { get; set; } = "Calculista, orgulhoso da carreira, mas desesperado para salvar a filha doente.";
    public string CaseTitle { get; set; } = "CASO #409: O Roubo do Núcleo Quântico";
    public string CaseObjective { get; set; } = "OBJETIVO: Descubra onde está o Núcleo Quântico antes do Turno 10 sem deixar a Pressão atingir 100%.";
    public string SecretTruth { get; set; } = "Viktor usou seu crachá às 02:14 para destravar a primeira porta porque o Diretor Marcus Vane pagou 50 mil créditos para o tratamento de Clara (filha de Viktor). O Núcleo está escondido no Contêiner Criogênico 04 do subsolo.";

    public NpcAppearance Appearance { get; set; } = new();

    // Atributos de 0 a 100
    public int Trust { get; set; } = 35;
    public int Stress { get; set; } = 25;
    public int CaseProgress { get; set; } = 10;

    public EmotionalState Emotion { get; set; } = EmotionalState.Calmo;
    public GameOutcome Outcome { get; set; } = GameOutcome.EmAndamento;
    public bool IsGuilty { get; set; } = true;

    public List<ClueItem> Clues { get; set; } = new();
    public List<ChatLogEntry> History { get; set; } = new();
    public List<TurnRecord> TurnRecords { get; set; } = new();
    public List<string> QuickSuggestions { get; set; } = new();
    public List<DialogueOption> DialogueTree { get; set; } = new();

    /// <summary>
    /// Retorna até maxCount opções de diálogo disponíveis para o turno atual,
    /// equilibrando opções de Confronto, Empatia, Pressão e Revelação/Confissão.
    /// </summary>
    public List<DialogueOption> GetActiveDialogueOptions(int maxCount = 4)
    {
        var available = DialogueTree.Where(opt =>
            !opt.IsUsed &&
            CaseProgress >= opt.RequiredMinProgress &&
            (string.IsNullOrEmpty(opt.RequiredClueTitle) || Clues.Any(c => c.Title.Equals(opt.RequiredClueTitle, StringComparison.OrdinalIgnoreCase))) &&
            (string.IsNullOrEmpty(opt.PrerequisiteOptionId) || DialogueTree.Any(prev => prev.Id == opt.PrerequisiteOptionId && prev.IsUsed))
        ).ToList();

        if (available.Count == 0)
        {
            available = DialogueTree.Where(opt => !opt.IsUsed).ToList();
            if (available.Count == 0 && DialogueTree.Count > 0)
            {
                // Se tudo foi usado e o caso ainda não fechou, reutiliza opções não-finais
                available = DialogueTree.Where(o => o.Category != "Final").ToList();
            }
        }

        var selected = new List<DialogueOption>();

        // Prioridade 1: Se progresso >= 50, destaca opção de Revelação/Confissão se disponível
        if (CaseProgress >= 50)
        {
            var climaxOpt = available.FirstOrDefault(o => o.Category is "Revelacao" or "Final");
            if (climaxOpt != null)
            {
                selected.Add(climaxOpt);
                available.Remove(climaxOpt);
            }
        }

        // Tenta pegar 1 de Confronto / Fatos
        var confrontOpt = available.FirstOrDefault(o => o.Category == "Confronto");
        if (confrontOpt != null)
        {
            selected.Add(confrontOpt);
            available.Remove(confrontOpt);
        }

        // Tenta pegar 1 de Empatia / Acordo
        var empathyOpt = available.FirstOrDefault(o => o.Category == "Empatia");
        if (empathyOpt != null)
        {
            selected.Add(empathyOpt);
            available.Remove(empathyOpt);
        }

        // Tenta pegar 1 de Pressão / Intimidação
        var pressureOpt = available.FirstOrDefault(o => o.Category == "Pressao");
        if (pressureOpt != null)
        {
            selected.Add(pressureOpt);
            available.Remove(pressureOpt);
        }

        // Completa com opções restantes
        foreach (var remaining in available)
        {
            if (selected.Count >= maxCount) break;
            if (!selected.Contains(remaining))
                selected.Add(remaining);
        }

        return selected;
    }

    public void ApplyGeneratedCase(LlmGeneratedCase generated)
    {
        CurrentTurn = 1;
        MaxTurns = 10;
        IsGuilty = generated.IsGuilty;
        Trust = 30;
        Stress = generated.IsGuilty ? 20 : 35;
        CaseProgress = 0;
        Emotion = EmotionalState.Calmo;
        Outcome = GameOutcome.EmAndamento;

        CaseTitle = !string.IsNullOrWhiteSpace(generated.CaseTitle) ? generated.CaseTitle : "CASO PROCEDURAL #001";
        CaseObjective = !string.IsNullOrWhiteSpace(generated.CaseObjective)
            ? generated.CaseObjective
            : (generated.IsGuilty
                ? "OBJETIVO: Obtenha a confissão completa antes do Turno 10 sem deixar a Pressão chegar a 100%."
                : "OBJETIVO: Prove a inocência do suspeito, desmascare a farsa e descubra quem realmente cometeu o crime antes do Turno 10.");
        NpcName = !string.IsNullOrWhiteSpace(generated.NpcName) ? generated.NpcName : "Suspeito Desconhecido";
        NpcRole = !string.IsNullOrWhiteSpace(generated.NpcRole) ? generated.NpcRole : "Suspeito sob Custódia";
        NpcPersonality = !string.IsNullOrWhiteSpace(generated.NpcPersonality) ? generated.NpcPersonality : "Defensivo e cauteloso";
        SecretTruth = !string.IsNullOrWhiteSpace(generated.SecretTruth) ? generated.SecretTruth : "O suspeito esconde uma prova crítica.";
        Appearance = generated.Appearance ?? new NpcAppearance();

        Clues.Clear();
        if (generated.InitialClues is { Count: > 0 })
        {
            int idx = 1;
            foreach (var c in generated.InitialClues)
            {
                Clues.Add(new ClueItem
                {
                    Id = $"clue_{idx++}",
                    Title = c.Title,
                    Description = c.Description,
                    IsNew = false
                });
            }
        }

        string openingGm = !string.IsNullOrWhiteSpace(generated.OpeningGmNarration)
            ? generated.OpeningGmNarration
            : $"{NpcName} aguarda sentado à mesa da sala de interrogatório, observando cada movimento seu.";

        string openingNpc = !string.IsNullOrWhiteSpace(generated.OpeningNpcDialogue)
            ? generated.OpeningNpcDialogue
            : "\"Eu já disse que não tenho nada a ver com isso, detetive. Seja rápido.\"";

        History.Clear();
        History.Add(new ChatLogEntry
        {
            Type = LogEntryType.GmNarration,
            Speaker = "OBSERVAÇÃO FORENSE",
            Text = openingGm,
            Turn = 0
        });
        History.Add(new ChatLogEntry
        {
            Type = LogEntryType.NpcDialogue,
            Speaker = NpcName,
            Text = openingNpc,
            Turn = 0
        });

        TurnRecords.Clear();
        TurnRecords.Add(new TurnRecord
        {
            TurnNumber = 0,
            IsOpeningScene = true,
            GmNarration = openingGm,
            NpcDialogue = openingNpc,
            NpcEmotion = EmotionalState.Calmo
        });

        DialogueTree = generated.DialogueTree?.Select(d => new DialogueOption
        {
            Id = d.Id,
            Category = d.Category,
            Tag = d.Tag,
            PlayerDialogue = d.PlayerDialogue,
            IntentSummary = d.IntentSummary,
            GmNarration = d.GmNarration,
            NpcDialogue = d.NpcDialogue,
            NpcEmotion = d.NpcEmotion,
            TrustDelta = d.TrustDelta,
            StressDelta = d.StressDelta,
            ProgressDelta = d.ProgressDelta,
            NewClueTitle = d.NewClueTitle,
            NewClueDescription = d.NewClueDescription,
            RequiredMinProgress = d.RequiredMinProgress,
            RequiredClueTitle = d.RequiredClueTitle,
            RelatedClueTitle = d.RelatedClueTitle,
            SynergisticKeywords = new List<string>(d.SynergisticKeywords ?? new()),
            PrerequisiteOptionId = d.PrerequisiteOptionId,
            IsUsed = false
        }).ToList() ?? new List<DialogueOption>();

        QuickSuggestions = DialogueTree.Take(3).Select(d => d.PlayerDialogue).ToList();
    }

    public void ResetCase()
    {
        ApplyGeneratedCase(new LlmGeneratedCase
        {
            CaseTitle = "CASO #409: O Roubo do Núcleo Quântico",
            CaseObjective = "OBJETIVO: Descubra onde está o Núcleo Quântico antes do Turno 10 sem deixar a Pressão atingir 100%.",
            NpcName = "Dr. Viktor Vance",
            NpcRole = "Engenheiro-Chefe de Segurança • Laboratório Aegis",
            NpcPersonality = "Intelectual, orgulhoso, tenta manter a calma técnica, mas é vulnerável quando o assunto é sua filha Clara.",
            SecretTruth = "Viktor usou seu crachá às 02:14 para destravar a primeira porta porque o Diretor Marcus Vane pagou 50 mil créditos para o tratamento de Clara (filha de Viktor). O Núcleo está escondido no Contêiner Criogênico 04 do subsolo.",
            Appearance = new NpcAppearance
            {
                SkinTone = "clara",
                HairColor = "grisalho",
                HairStyle = "curto",
                OutfitStyle = "jaleco",
                HasGlasses = true,
                HasBeard = false
            },
            InitialClues = new List<ClueItem>
            {
                new() { Title = "Crachá usado às 02:14", Description = "O crachá do Dr. Vance destravou o cofre principal às 02:14 da madrugada." },
                new() { Title = "Câmeras em Loop", Description = "As câmeras do setor B foram colocadas em loop usando um terminal interno." },
                new() { Title = "Transferência Suspeita", Description = "Uma conta ligada à clínica onde a filha de Vance está internada recebeu 50 mil créditos ontem." }
            },
            OpeningGmNarration = "A luz fria da sala de interrogatório reflete nos óculos de Viktor Vance. Ele mantém as mãos entrelaçadas sobre a mesa de metal, tentando parecer no controle, mas seus dedos batem levemente no próprio pulso.",
            OpeningNpcDialogue = "\"Olha, detetive... eu já disse aos guardas. Eu saí do laboratório às 21h. Se meu crachá foi registrado às duas da manhã, o sistema de vocês falhou ou alguém clonou minha credencial. Vamos acabar logo com isso.\"",
            DialogueTree = new List<DialogueOption>
            {
                new()
                {
                    Id = "vance_badge",
                    Category = "Confronto",
                    Tag = "[CONFRONTO • CRACHÁ]",
                    PlayerDialogue = "Dr. Vance, o registro eletrônico é infalível: seu crachá destravou o cofre às 02:14. Como explica estar lá nesse horário?",
                    GmNarration = "Vance ajeita os óculos nervosamente e pigarreia antes de responder, evitando contato visual.",
                    NpcDialogue = "\"Alguém clonou meu código magnético ou furtou meu crachá enquanto eu estava no refeitório! Eu saí às nove!\"",
                    NpcEmotion = EmotionalState.Defensivo,
                    TrustDelta = -5,
                    StressDelta = 15,
                    ProgressDelta = 15
                },
                new()
                {
                    Id = "vance_clara",
                    Category = "Empatia",
                    Tag = "[EMPATIA • FAMÍLIA]",
                    PlayerDialogue = "Viktor, nós sabemos sobre o tratamento experimental da Clara na clínica. Você fez isso para salvar sua filha?",
                    GmNarration = "O olhar arrogante de Vance se desfaz em choque e desespero genuíno. Seus olhos marejam.",
                    NpcDialogue = "\"Não coloquem a Clara nisso! Ela é só uma criança... eu não tinha como pagar o tratamento! Me garantam que a clínica continuará recebendo o suporte!\"",
                    NpcEmotion = EmotionalState.Cooperativo,
                    TrustDelta = 25,
                    StressDelta = -10,
                    ProgressDelta = 30,
                    NewClueTitle = "Coação por Tratamento Médico",
                    NewClueDescription = "Viktor agiu motivado pelo desespero financeiro da doença de sua filha Clara."
                },
                new()
                {
                    Id = "vance_cameras",
                    Category = "Pressao",
                    Tag = "[PRESSÃO • CÂMERAS]",
                    PlayerDialogue = "As câmeras foram colocadas em loop a partir do seu próprio terminal! Você planejou cada detalhe desse roubo!",
                    GmNarration = "Vance bate com a palma na mesa e eleva a voz, o monitor de batimentos cardíacos sobe abruptamente.",
                    NpcDialogue = "\"Eu sou o engenheiro-chefe! Qualquer técnico com acesso nível 3 poderia ter invadido meu terminal durante a madrugada!\"",
                    NpcEmotion = EmotionalState.Hostil,
                    TrustDelta = -10,
                    StressDelta = 25,
                    ProgressDelta = 15
                },
                new()
                {
                    Id = "vance_confess",
                    Category = "Revelacao",
                    Tag = "[REVELAÇÃO • CONFISSÃO]",
                    PlayerDialogue = "Entregue o Marcus Vane e me diga onde está o Núcleo Quântico antes que seja tarde demais para a sua filha.",
                    GmNarration = "Viktor apoia a testa nas mãos e desmonta completamente a postura de superioridade.",
                    NpcDialogue = "\"Tudo bem... foi o Diretor Marcus Vane! Ele pagou os 50 mil créditos e eu guardei o Núcleo no Contêiner Criogênico 04 do subsolo!\"",
                    NpcEmotion = EmotionalState.Cooperativo,
                    TrustDelta = 30,
                    StressDelta = -15,
                    ProgressDelta = 45,
                    RequiredMinProgress = 40,
                    NewClueTitle = "Núcleo no Contêiner 04",
                    NewClueDescription = "O Núcleo Quântico roubado foi escondido no Contêiner Criogênico 04 do subsolo."
                }
            }
        });
    }
}
