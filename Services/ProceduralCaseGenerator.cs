using System;
using System.Collections.Generic;
using System.Linq;
using LlamaInterrogation.Models;

namespace LlamaInterrogation.Services;

public class ClueBlueprint
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Keywords { get; init; } = Array.Empty<string>();
    public string ExcuseSpeech { get; init; } = string.Empty;
    public string AdmissionSpeech { get; init; } = string.Empty;
}

public class NoirCaseBlueprint
{
    public string Title { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;
    public string Objective { get; init; } = string.Empty;
    public string NpcName { get; init; } = string.Empty;
    public string NpcRole { get; init; } = string.Empty;
    public string NpcPersonality { get; init; } = string.Empty;
    public string SecretTruth { get; init; } = string.Empty;
    public string OpeningGmNarration { get; init; } = string.Empty;
    public string OpeningNpcDialogue { get; init; } = string.Empty;
    public string AlibiSpeech { get; init; } = string.Empty;
    public string EmpathyEarlySpeech { get; init; } = string.Empty;
    public string EarlyWhoWhereSpeech { get; init; } = string.Empty;
    public string WeakPointLabel { get; init; } = string.Empty;
    public string[] WeakPointKeywords { get; init; } = Array.Empty<string>();
    public string WeakPointReactionSpeech { get; init; } = string.Empty;
    public string ConfessionSpeech { get; init; } = string.Empty;
    public string KeyAccompliceWord { get; init; } = string.Empty;
    public string KeyConfessionWord { get; init; } = string.Empty;
    public string HiddenObjectLabel { get; init; } = string.Empty;
    public string BonusClueTitle { get; init; } = string.Empty;
    public string BonusClueDescription { get; init; } = string.Empty;
    public string[] BonusClueKeywords { get; init; } = Array.Empty<string>();
    public bool IsGuilty { get; init; } = true;
    public string ExonerationSpeech { get; init; } = string.Empty;
    public ClueBlueprint[] Clues { get; init; } = Array.Empty<ClueBlueprint>();
}

/// <summary>
/// Gerador Procedural que fornece casos Noir originais e inéditos, garantindo que o jogador
/// nunca jogue o mesmo caso repetidamente em uma sessão.
/// </summary>
public static class ProceduralCaseGenerator
{
    private static readonly Random Rng = new();

    private static readonly string[] FirstNames = {
        "Vicente", "Heitor", "Helena", "Dario", "César", "Arthur", "Raul", "Bruno", "Verônica",
        "Rodolfo", "Tiago", "Carmen", "Valdemar", "Samuel", "Cláudio", "Tonho", "Horácio", "Marcelo",
        "Dante", "Otávio", "Gabriel", "Álvaro", "Nestor", "Renata", "Elisa", "Sílvia", "Teresa",
        "Maurício", "Gustavo", "Rogério"
    };

    private static readonly string[] LastNames = {
        "Moretti", "Vasconcelos", "Santos", "Fontana", "Galvão", "Montenegro", "Camargo", "Alencar",
        "Prado", "Mendes", "Valério", "Valadares", "Cruz", "Nogueira", "Ramos", "Silva", "Figueira",
        "Brandão", "Peixoto", "Silveira", "Fontes", "Soares", "Barone", "Linhares", "Ferraz",
        "Salieri", "Castro", "Guimarães", "Bastos", "Albuquerque"
    };

    private static readonly string[] SkinTones = { "clara", "media", "morena", "escura" };
    private static readonly string[] HairColors = { "grisalho", "preto", "castanho", "loiro", "ruivo" };
    private static readonly string[] HairStyles = { "curto", "longo", "calvo", "topete" };
    private static readonly string[] OutfitStyles = { "jaleco", "terno", "jaqueta", "uniforme", "vinho" };


    /// <summary>
    /// Gera proceduralmente um caso Noir Ãºnico e inÃ©dito para a sessÃ£o.
    /// </summary>
    public static (NoirCaseBlueprint Blueprint, bool IsProcedurallyGenerated) GetUniqueBlueprint(HashSet<string>? usedTitles = null)
    {
        usedTitles ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var synthetic = GenerateInfiniteProceduralBlueprint(usedTitles);
        usedTitles.Add(synthetic.Title);
        return (synthetic, true);
    }

    /// <summary>
    /// Constrói um objeto LlmGeneratedCase a partir de um NoirCaseBlueprint.
    /// </summary>
    public static LlmGeneratedCase BuildCaseFromBlueprint(NoirCaseBlueprint bp, int caseNumber)
    {
        string skin = SkinTones[Rng.Next(SkinTones.Length)];
        string hairCol = HairColors[Rng.Next(HairColors.Length)];
        string hairSty = HairStyles[Rng.Next(HairStyles.Length)];
        string outfit = OutfitStyles[Rng.Next(OutfitStyles.Length)];
        bool glasses = Rng.NextDouble() > 0.45;
        bool beard = Rng.NextDouble() > 0.55;

        var generated = new LlmGeneratedCase
        {
            CaseTitle = $"CASO #{caseNumber:000}: {bp.Title}",
            CaseObjective = $"CONTEXTO: {bp.Context}\nOBJETIVO: {bp.Objective}",
            NpcName = bp.NpcName,
            NpcRole = bp.NpcRole,
            NpcPersonality = bp.NpcPersonality,
            SecretTruth = bp.SecretTruth,
            IsGuilty = bp.IsGuilty,
            OpeningGmNarration = bp.OpeningGmNarration,
            OpeningNpcDialogue = bp.OpeningNpcDialogue,
            Appearance = new NpcAppearance
            {
                SkinTone = skin,
                HairColor = hairCol,
                HairStyle = hairSty,
                OutfitStyle = outfit,
                HasGlasses = glasses,
                HasBeard = beard
            },
            InitialClues = bp.Clues.Select((c, i) => new ClueItem
            {
                Id = $"clue_{i + 1}",
                Title = c.Title,
                Description = c.Description,
                IsNew = false
            }).ToList()
        };

        generated.DialogueTree = BuildDialogueTreeForBlueprint(bp);
        generated.InitialSuggestions = new List<string>
        {
            $"Confrontar o álibi inicial de {bp.NpcName.Split(' ')[0]}",
            $"Apresentar a evidência '{bp.Clues[0].Title}' na mesa",
            $"Tocar no ponto fraco ({bp.WeakPointLabel}) oferecendo proteção"
        };

        return generated;
    }

    /// <summary>
    /// Constrói uma árvore completa e ramificada de 14 opções de diálogo em formato de falas do detetive.
    /// </summary>
    public static List<DialogueOption> BuildDialogueTreeForBlueprint(NoirCaseBlueprint bp)
    {
        var list = new List<DialogueOption>();
        int idCounter = 1;

        if (bp.IsGuilty)
        {
            // ==================== SUSPEITO CULPADO ====================
            // 1. Álibi Inicial (Confronto básico: pouco progresso, não vence o jogo sozinho)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_alibi",
                Category = "Confronto",
                Tag = "[CONFRONTO • ÁLIBI]",
                PlayerDialogue = "Você afirma que encerrou seu dia normalmente e foi para casa. Nossos relatórios mostram atividade no local logo depois. Como explica isso?",
                IntentSummary = "Confrontar a fragilidade do álibi inicial",
                GmNarration = $"{bp.NpcName} ajeita a postura na cadeira e desvia o olhar, mantendo um tom ensaiado para tentar manter a compostura.",
                NpcDialogue = $"\"{bp.AlibiSpeech}\"",
                NpcEmotion = EmotionalState.Defensivo,
                TrustDelta = -5,
                StressDelta = 10,
                ProgressDelta = 5,
                SynergisticKeywords = new List<string> { "02h00", "02:14", "02h15", "02h10", "vigia", "carro", "placa", "isqueiro", "chave", "cais", "álibi", "alibi" }
            });

            // 2. Provas materiais iniciais (Fase 1 e Fase 2 para cada pista)
            for (int i = 0; i < bp.Clues.Length; i++)
            {
                var clue = bp.Clues[i];
                string stage1Id = $"opt_{idCounter++}_clue_{i + 1}";

                list.Add(new DialogueOption
                {
                    Id = stage1Id,
                    Category = "Confronto",
                    Tag = $"[CONFRONTO • {clue.Title.ToUpperInvariant()}]",
                    PlayerDialogue = $"A perícia recolheu uma prova contundente: {clue.Title} ({clue.Description}). Como você justifica isso?",
                    IntentSummary = $"Apresentar prova material: {clue.Title}",
                    GmNarration = $"{bp.NpcName} engole em seco e seus olhos fitam a evidência posta sobre a mesa com visível desconforto.",
                    NpcDialogue = $"\"{clue.ExcuseSpeech}\"",
                    NpcEmotion = EmotionalState.Defensivo,
                    TrustDelta = -5,
                    StressDelta = 15,
                    ProgressDelta = 5,
                    RelatedClueTitle = clue.Title,
                    SynergisticKeywords = clue.Keywords.ToList()
                });

                list.Add(new DialogueOption
                {
                    Id = $"opt_{idCounter++}_clue_push_{i + 1}",
                    Category = "Pressao",
                    Tag = $"[PRESSÃO • {clue.Title.ToUpperInvariant()}]",
                    PlayerDialogue = $"Essa história sobre '{clue.Title}' não convence ninguém, {bp.NpcName}! Os fatos provam o contrário. Fale a verdade!",
                    IntentSummary = $"Pressionar contradição de {clue.Title}",
                    GmNarration = $"{bp.NpcName} recua contra o encosto da cadeira, a respiração pesada sob a pressão da evidência.",
                    NpcDialogue = $"\"{clue.AdmissionSpeech}\"",
                    NpcEmotion = EmotionalState.Nervoso,
                    TrustDelta = -5,
                    StressDelta = 20,
                    ProgressDelta = 10,
                    PrerequisiteOptionId = stage1Id,
                    RelatedClueTitle = clue.Title,
                    SynergisticKeywords = clue.Keywords.ToList()
                });
            }

            // 3. Abordagem empática de cooperação (Restaura confiança, reduz estresse, pouco progresso investigativo sozinho)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_empathy",
                Category = "Empatia",
                Tag = "[EMPATIA • COOPERAÇÃO]",
                PlayerDialogue = $"Olhe para mim, {bp.NpcName}. Eu vejo que você não é um criminoso de sangue frio. Conte o que realmente houve e eu garanto que você não afundará sozinho.",
                IntentSummary = "Oferecer apoio e reduzir resistência inicial",
                GmNarration = $"{bp.NpcName} solta o ar com força e seus ombros relaxam levemente, ponderando a oferta sincera do detetive.",
                NpcDialogue = $"\"{bp.EmpathyEarlySpeech}\"",
                NpcEmotion = EmotionalState.Cooperativo,
                TrustDelta = 20,
                StressDelta = -15,
                ProgressDelta = 5,
                SynergisticKeywords = new List<string> { "acordo", "proteção", "protecao", "filha", "irmã", "família", "mãe", "esposa" }
            });

            // 4. Investigação do medo de represálias
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_danger",
                Category = "Empatia",
                Tag = "[INVESTIGAÇÃO • PERIGO]",
                PlayerDialogue = "Você está aterrorizado com alguém lá fora, não está? Quem está te coagindo a assumir essa culpa?",
                IntentSummary = "Questionar as ameaças externas que o calam",
                GmNarration = $"{bp.NpcName} olha discretamente para a porta e para os cantos da sala, temendo estar sendo vigiado.",
                NpcDialogue = $"\"{bp.EarlyWhoWhereSpeech}\"",
                NpcEmotion = EmotionalState.Nervoso,
                TrustDelta = 15,
                StressDelta = 5,
                ProgressDelta = 5,
                SynergisticKeywords = new List<string> { "ameaça", "ameaca", "máfia", "agiotas", "perigo", "mandante" }
            });

            // 5. Ponto Fraco - Abordagem Empática de Proteção (Requer progresso mínimo investigativo >= 30%)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_weakpoint_help",
                Category = "Empatia",
                Tag = $"[EMPATIA • {bp.WeakPointLabel.ToUpperInvariant()}]",
                PlayerDialogue = $"Nós sabemos que a sua maior preocupação é {bp.WeakPointLabel}. Se você falar a verdade agora, nós colocamos proteção policial imediata.",
                IntentSummary = $"Oferecer proteção para {bp.WeakPointLabel}",
                GmNarration = $"A máscara de frieza cai por terra. {bp.NpcName} se inclina para frente com os olhos marejados de pavor e alívio.",
                NpcDialogue = $"\"{bp.WeakPointReactionSpeech}\"",
                NpcEmotion = EmotionalState.Cooperativo,
                TrustDelta = 25,
                StressDelta = -10,
                ProgressDelta = 20,
                RequiredMinProgress = 30,
                NewClueTitle = bp.BonusClueTitle,
                NewClueDescription = bp.BonusClueDescription,
                RelatedClueTitle = bp.BonusClueTitle,
                SynergisticKeywords = bp.WeakPointKeywords.ToList()
            });

            // 6. Ponto Fraco - Abordagem de Pressão Policial (Alto risco: dispara estresse e derruba confiança)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_weakpoint_threat",
                Category = "Pressao",
                Tag = $"[PRESSÃO • {bp.WeakPointLabel.ToUpperInvariant()}]",
                PlayerDialogue = $"Você vai mesmo deixar {bp.WeakPointLabel} sofrer as consequências por algo que você causou? É essa a sua escolha?",
                IntentSummary = $"Pressionar a responsabilidade sobre {bp.WeakPointLabel}",
                GmNarration = $"{bp.NpcName} cerra os punhos e treme de indignação e desespero, o eletrocardiograma no monitor sobe bruscamente.",
                NpcDialogue = $"\"{bp.WeakPointReactionSpeech}\"",
                NpcEmotion = EmotionalState.Acuado,
                TrustDelta = -20,
                StressDelta = 35,
                ProgressDelta = 10,
                RequiredMinProgress = 30,
                NewClueTitle = bp.BonusClueTitle,
                NewClueDescription = bp.BonusClueDescription,
                RelatedClueTitle = bp.BonusClueTitle,
                SynergisticKeywords = bp.WeakPointKeywords.ToList()
            });

            // 7. Pressão Agressiva / Pena Máxima (ANTI-SPAM: 0% de progresso! Apenas aumenta estresse e quebra confiança)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_threat_prison",
                Category = "Pressao",
                Tag = "[PRESSÃO • PENA MÁXIMA]",
                PlayerDialogue = "Você está diante de uma acusação gravíssima! São trinta anos na prisão se não abrir a boca agora!",
                IntentSummary = "Ameaçar com pena máxima de prisão",
                GmNarration = $"O ar foge dos pulmões de {bp.NpcName}. A agressividade destrói qualquer relação de confiança e gera revolta e pânico.",
                NpcDialogue = "\"Você está me sufocando, detetive! Eu tenho direitos! Se continuar gritando comigo, não digo mais nenhuma palavra sem advogado!\"",
                NpcEmotion = EmotionalState.Acuado,
                TrustDelta = -20,
                StressDelta = 30,
                ProgressDelta = 0
            });

            // 8. Confrontar Mandante Principal (Requer progresso intermediário >= 50%)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_accomplice",
                Category = "Revelacao",
                Tag = $"[REVELAÇÃO • {bp.KeyAccompliceWord.ToUpperInvariant()}]",
                PlayerDialogue = $"Nós já descobrimos o elo: foi {bp.KeyAccompliceWord} quem orquestrou tudo! Confirme isso oficialmente e tire esse peso das suas costas!",
                IntentSummary = $"Confrontar o mandante {bp.KeyAccompliceWord}",
                GmNarration = $"{bp.NpcName} respira fundo, engolindo o terror pela primeira vez para incriminar o verdadeiro mandante.",
                NpcDialogue = $"\"Foi ele sim... {bp.KeyAccompliceWord} orquestrou cada detalhe! Ele prometeu que me protegeria, mas me usou como bucha de canhão!\"",
                NpcEmotion = EmotionalState.Cooperativo,
                TrustDelta = 15,
                StressDelta = -10,
                ProgressDelta = 25,
                RequiredMinProgress = 50,
                SynergisticKeywords = new List<string> { bp.KeyAccompliceWord, "mandante", "suborno", "ordem", "chefe" }
            });

            // 9. Confissão Final e Localização da Prova Oculta (Requer progresso avançado >= 70%)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_final_truth",
                Category = "Final",
                Tag = $"[CONFISSÃO FINAL • {bp.HiddenObjectLabel.ToUpperInvariant()}]",
                PlayerDialogue = $"Acabou o jogo, {bp.NpcName}. Diga exatamente onde está {bp.HiddenObjectLabel} e feche seu depoimento agora com a polícia!",
                IntentSummary = $"Exigir localização de {bp.HiddenObjectLabel} e confissão final",
                GmNarration = $"{bp.NpcName} abaixa a cabeça sobre a mesa, os ombros tremendo de alívio e exaustão moral, entregando a verdade.",
                NpcDialogue = $"\"{bp.ConfessionSpeech}\"",
                NpcEmotion = EmotionalState.Cooperativo,
                TrustDelta = 20,
                StressDelta = -15,
                ProgressDelta = 30,
                RequiredMinProgress = 70,
                NewClueTitle = $"Confissão: {bp.KeyConfessionWord}",
                NewClueDescription = bp.ConfessionSpeech,
                SynergisticKeywords = new List<string> { bp.KeyConfessionWord, bp.HiddenObjectLabel, "prova", "escondido", "verdade" }
            });
        }
        else
        {
            // ==================== SUSPEITO INOCENTE (BODE EXPIATÓRIO) ====================
            // 1. Álibi Inicial (Inocente assustado com a detenção)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_alibi_innocent",
                Category = "Confronto",
                Tag = "[CONFRONTO • ÁLIBI]",
                PlayerDialogue = "Você foi detido nas imediações do crime. O que você estava fazendo no local naquele momento?",
                IntentSummary = "Questionar presença no local do crime",
                GmNarration = $"{bp.NpcName} arregala os olhos, as mãos trêmulas sobre a mesa demonstrando choque genuíno com a suspeita.",
                NpcDialogue = $"\"{bp.AlibiSpeech}\"",
                NpcEmotion = EmotionalState.Nervoso,
                TrustDelta = -5,
                StressDelta = 15,
                ProgressDelta = 5,
                SynergisticKeywords = new List<string> { "local", "hora", "álibi", "alibi", "presença", "noite" }
            });

            // 2. Apresentar evidências para testar contradições (Fase 1 e Fase 2)
            for (int i = 0; i < bp.Clues.Length; i++)
            {
                var clue = bp.Clues[i];
                string stage1Id = $"opt_{idCounter++}_innocent_clue_{i + 1}";

                list.Add(new DialogueOption
                {
                    Id = stage1Id,
                    Category = "Confronto",
                    Tag = $"[INVESTIGAÇÃO • {clue.Title.ToUpperInvariant()}]",
                    PlayerDialogue = $"Encontramos '{clue.Title}' ({clue.Description}). Se você jura que não foi você, como isso foi parar lá?",
                    IntentSummary = $"Apresentar prova que o incrimina: {clue.Title}",
                    GmNarration = $"{bp.NpcName} examina a prova perplexo, apontando incoerências com voz aflita.",
                    NpcDialogue = $"\"{clue.ExcuseSpeech}\"",
                    NpcEmotion = EmotionalState.Nervoso,
                    TrustDelta = 5,
                    StressDelta = 10,
                    ProgressDelta = 5,
                    RelatedClueTitle = clue.Title,
                    SynergisticKeywords = clue.Keywords.ToList()
                });

                list.Add(new DialogueOption
                {
                    Id = $"opt_{idCounter++}_innocent_push_{i + 1}",
                    Category = "Empatia",
                    Tag = $"[DESVENDAR FARSA • {clue.Title.ToUpperInvariant()}]",
                    PlayerDialogue = $"Pense com calma: quem tinha acesso às suas coisas ou motivos para plantar '{clue.Title}' para te incriminar?",
                    IntentSummary = $"Investigar como {clue.Title} foi forjada",
                    GmNarration = $"{bp.NpcName} arregala os olhos ao perceber a armação e conecta os fatos com urgência.",
                    NpcDialogue = $"\"{clue.AdmissionSpeech}\"",
                    NpcEmotion = EmotionalState.Cooperativo,
                    TrustDelta = 15,
                    StressDelta = -10,
                    ProgressDelta = 15,
                    PrerequisiteOptionId = stage1Id,
                    RelatedClueTitle = clue.Title,
                    SynergisticKeywords = clue.Keywords.ToList()
                });
            }

            // 3. Acolhimento Empático (Crucial para inocentes!)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_innocent_empathy",
                Category = "Empatia",
                Tag = "[EMPATIA • ACOLHIMENTO]",
                PlayerDialogue = $"Respire fundo, {bp.NpcName}. Eu não estou aqui para fabricar um culpado, estou aqui para descobrir a verdade. Se você é inocente, me ajude a provar.",
                IntentSummary = "Transmitir segurança e pedir colaboração sincera",
                GmNarration = $"O alívio invade o rosto pálido de {bp.NpcName}. A tensão corporal cede imediatamente diante da postura justa do investigador.",
                NpcDialogue = $"\"{bp.EmpathyEarlySpeech}\"",
                NpcEmotion = EmotionalState.Cooperativo,
                TrustDelta = 25,
                StressDelta = -20,
                ProgressDelta = 10,
                SynergisticKeywords = new List<string> { "inocente", "verdade", "ajuda", "proteção", "justiça" }
            });

            // 4. Investigar Quem Armou a Cilada
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_innocent_plot",
                Category = "Empatia",
                Tag = "[INVESTIGAÇÃO • ARMAÇÃO]",
                PlayerDialogue = "Alguém orquestrou tudo para que a polícia chegasse direto em você. Quem é essa pessoa?",
                IntentSummary = "Identificar o mandante da armação",
                GmNarration = $"{bp.NpcName} se inclina para frente e sussurra com convicção e receio.",
                NpcDialogue = $"\"{bp.EarlyWhoWhereSpeech}\"",
                NpcEmotion = EmotionalState.Cooperativo,
                TrustDelta = 15,
                StressDelta = -5,
                ProgressDelta = 10,
                SynergisticKeywords = new List<string> { "armação", "armacao", "cilada", "mandante", "inimigo" }
            });

            // 5. Ponto Fraco do Inocente (Família / Desespero - Requer progresso >= 30%)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_innocent_weakpoint",
                Category = "Empatia",
                Tag = $"[EMPATIA • {bp.WeakPointLabel.ToUpperInvariant()}]",
                PlayerDialogue = $"Eu sei o quanto você se importa com {bp.WeakPointLabel}. Não deixe quem te incriminou sair impune. Fale tudo!",
                IntentSummary = $"Motivar revelação através de {bp.WeakPointLabel}",
                GmNarration = $"Lágrimas brotam nos olhos de {bp.NpcName}. A determinação em salvar {bp.WeakPointLabel} supera o terror da cadeia.",
                NpcDialogue = $"\"{bp.WeakPointReactionSpeech}\"",
                NpcEmotion = EmotionalState.Cooperativo,
                TrustDelta = 25,
                StressDelta = -15,
                ProgressDelta = 20,
                RequiredMinProgress = 30,
                NewClueTitle = bp.BonusClueTitle,
                NewClueDescription = bp.BonusClueDescription,
                RelatedClueTitle = bp.BonusClueTitle,
                SynergisticKeywords = bp.WeakPointKeywords.ToList()
            });

            // 6. Pressão Cruel / Coação Indevida (PERIGOSÍSSIMO: +40% Estresse, -25% Confiança! Risco de DerrotaCoacaoInocente)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_innocent_threat",
                Category = "Pressao",
                Tag = "[PRESSÃO • ACUSAÇÃO CRUEL]",
                PlayerDialogue = "Pare com esse teatro de vítima! As evidências estão contra você e você vai mofar numa cela para sempre!",
                IntentSummary = "Coagir e ameaçar violentamente o suspeito",
                GmNarration = $"{bp.NpcName} entra em colapso de desespero, hiperventilando com as mãos na cabeça diante da injustiça da polícia.",
                NpcDialogue = "\"Não! Pelo amor de Deus, eu não fiz isso! Vocês estão me torturando psicologicamente! Eu não aguento mais essa pressão!\"",
                NpcEmotion = EmotionalState.Acuado,
                TrustDelta = -25,
                StressDelta = 40,
                ProgressDelta = 0
            });

            // 7. Confrontar Evidência Contra o Verdadeiro Culpado (Requer progresso intermediário >= 50%)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_innocent_true_culprit",
                Category = "Revelacao",
                Tag = $"[REVELAÇÃO • {bp.KeyAccompliceWord.ToUpperInvariant()}]",
                PlayerDialogue = $"Nós cruzamos as pontas: {bp.KeyAccompliceWord} foi quem planejou essa farsa para escapar impune! Você confirma essa ligação?",
                IntentSummary = $"Desmascarar a armação de {bp.KeyAccompliceWord}",
                GmNarration = $"{bp.NpcName} respira fundo com firmeza renovada, confirmando o ardil do verdadeiro assassino.",
                NpcDialogue = $"\"Foi ele sim! {bp.KeyAccompliceWord} sabia de cada detalhe da minha rotina e me colocou no centro disso tudo para desviar a atenção de vocês!\"",
                NpcEmotion = EmotionalState.Cooperativo,
                TrustDelta = 20,
                StressDelta = -15,
                ProgressDelta = 25,
                RequiredMinProgress = 50,
                SynergisticKeywords = new List<string> { bp.KeyAccompliceWord, "armação", "farsa", "verdadeiro", "mandante" }
            });

            // 8. Exoneração Completa e Denúncia Formal (Requer progresso >= 70%)
            list.Add(new DialogueOption
            {
                Id = $"opt_{idCounter++}_innocent_final",
                Category = "Final",
                Tag = $"[EXONERAÇÃO • {bp.HiddenObjectLabel.ToUpperInvariant()}]",
                PlayerDialogue = $"Suas declarações batem com a perícia. Você está inocentado, {bp.NpcName}! Onde está {bp.HiddenObjectLabel} para prendermos o verdadeiro culpado?",
                IntentSummary = $"Concluir a inocência e recolher a prova chave contra o mandante",
                GmNarration = $"{bp.NpcName} solta o choro contido há horas, entregando a localização da prova que sela o caso.",
                NpcDialogue = $"\"{(string.IsNullOrEmpty(bp.ExonerationSpeech) ? bp.ConfessionSpeech : bp.ExonerationSpeech)}\"",
                NpcEmotion = EmotionalState.Cooperativo,
                TrustDelta = 30,
                StressDelta = -25,
                ProgressDelta = 30,
                RequiredMinProgress = 70,
                NewClueTitle = $"Prova Final: {bp.KeyConfessionWord}",
                NewClueDescription = bp.ConfessionSpeech,
                SynergisticKeywords = new List<string> { bp.KeyConfessionWord, bp.HiddenObjectLabel, "inocente", "prova", "justiça" }
            });
        }

        return list;
    }
    /// <summary>
    /// Sintetizador procedural que monta dinamicamente casos infinitos combinando arquÃ©tipos,
    /// motivos, provas e segredos sem nunca repetir a mesma configuraÃ§Ã£o.
    /// </summary>
    private static NoirCaseBlueprint GenerateInfiniteProceduralBlueprint(HashSet<string> usedTitles)
    {
        string firstName = FirstNames[Rng.Next(FirstNames.Length)];
        string lastName = LastNames[Rng.Next(LastNames.Length)];
        string suspectName = $"{firstName} {lastName}";

        var guiltyArchetypes = new[]
        {
            new
            {
                TitleSeed = "O Enigma do Cofre da EstaÃ§Ã£o",
                Context = "O tesoureiro da linha fÃ©rrea foi encontrado inconsciente e o malote de 150 mil da folha de pagamento desapareceu.",
                Objective = $"FaÃ§a {suspectName} confessar quem forÃ§ou o cofre e onde escondeu as chaves mestras e o dinheiro.",
                Role = "Despachante FerroviÃ¡rio â€¢ EstaÃ§Ã£o Central",
                Personality = "PragmÃ¡tico e cansado, mas teme a ruÃ­na financeira de sua famÃ­lia.",
                Accomplice = "Inspetor Cordeiro",
                HiddenPlace = "dentro da caldeira da locomotiva nÂº 9",
                HiddenObj = "o malote com 150 mil",
                WeakPoint = "sua neta Maria",
                WeakKeywords = new[] { "neta", "maria", "famÃ­lia", "menina", "salvar", "hospital" },
                Clue1Title = "Chave Inglesa com Sangue Seco",
                Clue1Desc = $"Uma chave pesada pertencente Ã  oficina de {suspectName} foi localizada atrÃ¡s da bilheteria.",
                Clue1Excuse = "Aquela chave sumiu da minha caixa de ferramentas na Ãºltima quarta-feira, qualquer foguista teve acesso.",
                Clue1Adm = "Sim... a chave era minha! Fui eu quem desferiu o golpe quando o tesoureiro tentou puxar o alarme!",
                Clue2Title = "Bilhete de Passagem Rasgado",
                Clue2Desc = "Um tÃ­quete de fuga para o porto na madrugada com horÃ¡rio marcado para as 03h10.",
                Clue2Excuse = "Comprei a passagem apenas para visitar parentes no litoral durante minha folga regulamentar.",
                Clue2Adm = "A passagem era para mim sim! Eu ia fugir no cargueiro das trÃªs e dez assim que entregasse a grana!",
                Clue3Title = "Graxa Especial da Tranca",
                Clue3Desc = "VestÃ­gios de lubrificante quÃ­mico militar encontrados nas unhas e punhos da camisa do suspeito.",
                Clue3Excuse = "Limpei as engrenagens da catraca de embarque antes de encerrar o expediente da noite.",
                Clue3Adm = "A graxa Ã© do segredo do cofre! Cordeiro me deu o cÃ³digo e me mandou forÃ§ar a maÃ§aneta interna!",
                Clue4Title = "RelÃ³gio de Bolso Amassado",
                Clue4Desc = "O relÃ³gio do tesoureiro travado no horÃ¡rio exato do assalto, com fibras de uniforme de maquinista.",
                Clue4Excuse = "O tesoureiro vivia deixando coisas no vestiÃ¡rio geral dos maquinistas.",
                Clue4Adm = "Eu arranquei o relÃ³gio dele durante a luta para que ninguÃ©m soubesse o minuto exato da aÃ§Ã£o!"
            },
            new
            {
                TitleSeed = "Chantagem no Jornal da Noite",
                Context = "O editor-chefe investigativo foi morto por asfixia em sua sala com os arquivos da quadrilha revirados.",
                Objective = $"FaÃ§a {suspectName} confessar quem encomendou o roubo das pastas e onde escondeu os manuscritos.",
                Role = "Chefe de RedaÃ§Ã£o â€¢ DiÃ¡rio Popular",
                Personality = "Culto, sarcÃ¡stico e tenso, com histÃ³rico de dÃ­vidas de apostas clandestinas.",
                Accomplice = "Senador Fagundes",
                HiddenPlace = "no depÃ³sito de papel de bobina do subsolo",
                HiddenObj = "os manuscritos de denÃºncia",
                WeakPoint = "seu filho recÃ©m-nascido Lucas",
                WeakKeywords = new[] { "filho", "lucas", "bebÃª", "bebe", "berÃ§o", "famÃ­lia", "proteÃ§Ã£o" },
                Clue1Title = "Lauda Datilografada com Sangue",
                Clue1Desc = "Uma folha de texto da mÃ¡quina pessoal do suspeito encontrada sob o corpo do editor.",
                Clue1Excuse = "Aquela lauda era apenas uma matÃ©ria policial arquivada que o editor pediu para revisar antes do fechamento.",
                Clue1Adm = "A lauda caiu da minha pasta quando ele me pegou vasculhando a gaveta dele!",
                Clue2Title = "Tinteiro de Prata Quebrado",
                Clue2Desc = "Fragmentos de cristal com marcas de impacto violento no chÃ£o do gabinete da vÃ­tima.",
                Clue2Excuse = "O tinteiro escorregou da escrivaninha na tarde anterior durante uma discussÃ£o sobre prazos de pauta.",
                Clue2Adm = "O tinteiro se quebrou quando o capanga de Fagundes prensou o editor contra a parede!",
                Clue3Title = "Chave da Sala de MÃ¡quinas",
                Clue3Desc = $"A chave do depÃ³sito onde as pastas sumiram foi encontrada no bolso do casaco de {suspectName}.",
                Clue3Excuse = "Tenho a cÃ³pia daquela chave porque guardo resmas de papel e formulÃ¡rios administrativos lÃ¡ dentro.",
                Clue3Adm = "Fui eu quem destrancou o depÃ³sito Ã s duas da madrugada para esconder as pastas originais!",
                Clue4Title = "Bloco de Notas Rascunhado",
                Clue4Desc = "Um bloco com anotaÃ§Ãµes de quantias em dinheiro e as iniciais do senador envolvido no escÃ¢ndalo.",
                Clue4Excuse = "SÃ£o apenas estimativas de tiragem e custos de grÃ¡fica para a ediÃ§Ã£o de domingo.",
                Clue4Adm = "Eram os valores do suborno que Fagundes me prometeu para queimar a reportagem!"
            },
            new
            {
                TitleSeed = "O Assassinato no CabarÃ© Imperial",
                Context = "A estrela do cabarÃ© foi encontrada sem vida em seu camarim apÃ³s uma briga aos fundos Ã s 02h20.",
                Objective = $"FaÃ§a {suspectName} confessar quem contratou o crime e onde escondeu as joias furtadas.",
                Role = "Maitre e Barman â€¢ CabarÃ© Imperial",
                Personality = "Frio, polido e observador, mas desesperado para nÃ£o ser condenado Ã  pena capital.",
                Accomplice = "Comendador Barreto",
                HiddenPlace = "dentro do barril de uÃ­sque falso nÂº 12",
                HiddenObj = "as joias de esmeralda",
                WeakPoint = "sua irmÃ£ mais nova Teresa",
                WeakKeywords = new[] { "irmÃ£", "irma", "teresa", "famÃ­lia", "moÃ§a", "proteÃ§Ã£o", "convento" },
                Clue1Title = "LenÃ§o Manchado de CosmÃ©tico",
                Clue1Desc = "Um lenÃ§o fino de linho com a fragrÃ¢ncia exclusiva da vÃ­tima e marcas de dedos ensanguentados.",
                Clue1Excuse = "Entreguei o lenÃ§o Ã  artista durante o intervalo do primeiro ato quando ela retocava a maquiagem.",
                Clue1Adm = "O lenÃ§o foi usado para abafar os gritos dela quando Barreto mandou cobrar o colar!",
                Clue2Title = "Anel com BrasÃ£o Nobre",
                Clue2Desc = "Um anel masculino de ouro pesado caÃ­do atrÃ¡s da cortina de veludo do camarim.",
                Clue2Excuse = "Aquele anel foi esquecido por algum dos clientes abonados que frequentam as mesas VIP do mezanino.",
                Clue2Adm = "O anel Ã© do prÃ³prio Comendador Barreto... caiu da mÃ£o dele durante a luta no camarim!",
                Clue3Title = "TaÃ§a com ResÃ­duo QuÃ­mico",
                Clue3Desc = "Uma taÃ§a de cristal com gotas de sedativo para cavalos misturado ao licor servido pelo bar.",
                Clue3Excuse = "Sirvo centenas de coquetÃ©is todas as noites, qualquer garÃ§om pode ter trocado as bandejas na cozinha.",
                Clue3Adm = "Fui eu quem colocou as gotas no licor a mando de Barreto para que ela nÃ£o resistisse ao assalto!",
                Clue4Title = "Chave da SaÃ­da de EmergÃªncia",
                Clue4Desc = "A chave de ferro dos fundos do cabarÃ© com fibras do vestido da vÃ­tima presa no ferrolho.",
                Clue4Excuse = "A porta dos fundos fica sempre aberta para os entregadores de bebidas e mÃºsicos.",
                Clue4Adm = "Eu destranquei a porta de emergÃªncia para facilitar a fuga rÃ¡pida de Barreto com as joias!"
            },
            new
            {
                TitleSeed = "Emboscada no Clube Blue Note",
                Context = "O informante 'Rato' Soares foi morto com dois tiros Ã s 02h00 no beco atrÃ¡s do Clube de Jazz Blue Note.",
                Objective = $"FaÃ§a {suspectName} confessar quem atirou em Soares e onde escondeu o livro-caixa do sindicato.",
                Role = "Gerente Noturno â€¢ Clube Blue Note",
                Personality = "IrÃ´nico e vivido na noite, tenta manter a frieza, mas entra em pÃ¢nico quando a seguranÃ§a de sua famÃ­lia Ã© ameaÃ§ada.",
                Accomplice = "Donato Braga",
                HiddenPlace = "dentro da caixa acÃºstica do piano de cauda do clube",
                HiddenObj = "o livro-caixa do sindicato",
                WeakPoint = "sua irmÃ£ LÃºcia",
                WeakKeywords = new[] { "irmÃ£", "irma", "lÃºcia", "lucia", "famÃ­lia", "familia", "proteger", "proteÃ§Ã£o" },
                Clue1Title = "Chave da Porta dos Fundos",
                Clue1Desc = "Uma cÃ³pia recente da chave da porta dos fundos foi encontrada caÃ­da no beco onde o corpo foi achado.",
                Clue1Excuse = "Aquela cÃ³pia sumiu do quadro da gerÃªncia hÃ¡ duas semanas, qualquer garÃ§om pode ter pegado.",
                Clue1Adm = "Fui eu quem destrancou os fundos Ã s duas em ponto! Donato jurou matar minha irmÃ£ se eu nÃ£o abrisse!",
                Clue2Title = "Isqueiro de Prata Gravado",
                Clue2Desc = $"Um isqueiro gravado com as iniciais de {suspectName} achado a trÃªs metros do cadÃ¡ver no beco.",
                Clue2Excuse = "Perdi esse isqueiro no balcÃ£o do bar durante o show da meia-noite, diante de dezenas de clientes.",
                Clue2Adm = "O isqueiro caiu da minha mÃ£o quando ouvi os tiros no beco e saÃ­ correndo desesperado!",
                Clue3Title = "Bilhete com AmeaÃ§a de Morte",
                Clue3Desc = "Um bilhete amassado com caligrafia recortada de jornal encontrado no bolso do terno do suspeito.",
                Clue3Excuse = "AlguÃ©m colocou esse bilhete no bolso do meu casaco na chapelaria enquanto eu atendia as mesas.",
                Clue3Adm = "O bilhete veio de Donato Braga! Ele me mandou calar a boca ou LÃºcia seria a prÃ³xima vÃ­tima!",
                Clue4Title = "Fita MagnÃ©tica de GravaÃ§Ã£o",
                Clue4Desc = "Um carretel de gravaÃ§Ã£o do escritÃ³rio com trechos de discussÃ£o entre a gerÃªncia e os capangas de Donato.",
                Clue4Excuse = "Gravamos os ensaios musicais dos mÃºsicos de jazz, nÃ£o hÃ¡ nada criminoso naquelas fitas.",
                Clue4Adm = "Eu gravei Donato me ameaÃ§ando no escritÃ³rio para ter uma garantia caso a polÃ­cia me pegasse!"
            },
            new
            {
                TitleSeed = "O Veneno no CÃ¡lice da Ã“pera",
                Context = "O maestro da Ã“pera Municipal foi envenenado com estricnina durante o intervalo do segundo ato.",
                Objective = $"FaÃ§a {suspectName} confessar quem encomendou o veneno e onde escondeu a partitura original roubada.",
                Role = "Pianista e Arranjador â€¢ Ã“pera Municipal",
                Personality = "MetÃ³dico, vaidoso e ansioso, com rancor profissional reprimido e dÃ­vidas de jogatina.",
                Accomplice = "CrÃ­tico OtÃ¡vio Munhoz",
                HiddenPlace = "no vÃ£o do forro do mezanino 3",
                HiddenObj = "a partitura original roubada",
                WeakPoint = "seu sobrinho doente TomÃ¡s",
                WeakKeywords = new[] { "sobrinho", "tomÃ¡s", "tomas", "menino", "famÃ­lia", "hospital", "remÃ©dio" },
                Clue1Title = "Frasco de Estricnina FarmacÃªutica",
                Clue1Desc = "Um frasco de vidro Ã¢mbar com resÃ­duos de pÃ³ branco tÃ³xico encontrado na lixeira do camarim.",
                Clue1Excuse = "Uso esse produto na manutenÃ§Ã£o quÃ­mica das cordas e teclas de marfim dos pianos do teatro.",
                Clue1Adm = "O pÃ³ era estricnina pura... OtÃ¡vio Munhoz me entregou o frasco no saguÃ£o e me mandou pingar na taÃ§a!",
                Clue2Title = "Partitura com AnotaÃ§Ãµes Manuscritas",
                Clue2Desc = "A partitura do maestro com comentÃ¡rios sarcÃ¡sticos e anotaÃ§Ãµes tÃ©cnicas feitas com a caligrafia do suspeito.",
                Clue2Excuse = "EstudÃ¡vamos a mesma composiÃ§Ã£o lÃ­rica juntos pela manhÃ£, as anotaÃ§Ãµes eram estritamente harmÃ´nicas.",
                Clue2Adm = "Eu odiava aquele homem! Ele roubou meus arranjos e Munhoz prometeu financiar minha Ã³pera se eu o tirasse do caminho!",
                Clue3Title = "TaÃ§a de Cristal Manchada",
                Clue3Desc = "A taÃ§a usada pelo maestro no intervalo continha impressÃµes parciais dos dedos do suspeito na haste.",
                Clue3Excuse = "Entreguei a taÃ§a de Ã¡gua a pedido dele no fosso da orquestra antes do inÃ­cio do primeiro ato.",
                Clue3Adm = "Sim, fui eu quem levou a taÃ§a ao camarim dele no intervalo... mas o veneno jÃ¡ estava misturado por Munhoz!",
                Clue4Title = "Recibo de Casa de Penhores",
                Clue4Desc = "Um recibo de penhor de joias de famÃ­lia para quitar dÃ©bitos de jogo datado de dois dias antes do crime.",
                Clue4Excuse = "Apenas resgatei um relÃ³gio antigo que meu pai havia deixado com o penhorista.",
                Clue4Adm = "Eu estava falido e Munhoz comprou minhas dÃ­vidas na mesa de bacarÃ¡ em troca do veneno!"
            },
            new
            {
                TitleSeed = "O Golpe no Grande Hotel",
                Context = "O diplomata suÃ­Ã§o foi dopado na suÃ­te presidencial e uma maleta com microfilmagens secretas desapareceu.",
                Objective = $"FaÃ§a {suspectName} confessar quem encomendou as microfilmagens e onde escondeu os rolos de filme.",
                Role = "Concierge Noturno â€¢ Grande Hotel Central",
                Personality = "Polido, calculista e cÃ­nico, habituado aos segredos da alta sociedade, mas apavorado com a penitenciÃ¡ria.",
                Accomplice = "EspiÃ£o Viktor Klaus",
                HiddenPlace = "no cofre de bagagens 14B da recepÃ§Ã£o",
                HiddenObj = "as microfilmagens da embaixada",
                WeakPoint = "sua noiva enfermeira Beatriz",
                WeakKeywords = new[] { "noiva", "beatriz", "noivado", "moÃ§a", "famÃ­lia", "casamento" },
                Clue1Title = "Chave Mestra das SuÃ­tes",
                Clue1Desc = "O cartÃ£o mestra de metal das suÃ­tes de luxo foi registrado destrancando a suÃ­te 401 Ã s 03h15.",
                Clue1Excuse = "Subi para levar toalhas limpas solicitadas pelo interfone da governanÃ§a Ã s trÃªs da manhÃ£.",
                Clue1Adm = "Eu usei a chave mestra para abrir a suÃ­te enquanto Klaus colocava o Ã©ter no lenÃ§o!",
                Clue2Title = "Rolo de Filme Revelado",
                Clue2Desc = "Fragmentos de pelÃ­cula 16mm cortada encontrados na cesta de correspondÃªncias da portaria.",
                Clue2Excuse = "Eram apenas fotos promocionais do saguÃ£o do hotel para o catÃ¡logo da temporada de inverno.",
                Clue2Adm = "Eram as cÃ³pias dos tratados da embaixada... Klaus fotografou tudo em cinco minutos!",
                Clue3Title = "CrachÃ¡ Registrado Ã s 03h15",
                Clue3Desc = "O ponto eletrÃ´nico e a guarita registraram o crachÃ¡ do suspeito no corredor presidencial.",
                Clue3Excuse = "FaÃ§o rondas periÃ³dicas em todos os andares para garantir a tranquilidade dos hÃ³spedes.",
                Clue3Adm = "Eu fiquei vigiando o corredor enquanto Klaus revirava o cofre da suÃ­te!",
                Clue4Title = "Envelope com Moeda Estrangeira",
                Clue4Desc = "Cinco mil dÃ³lares em notas limpas escondidos atrÃ¡s do espelho do banheiro dos funcionÃ¡rios.",
                Clue4Excuse = "Aquele envelope foi esquecido por algum turista estrangeiro no hall e guardado para devoluÃ§Ã£o.",
                Clue4Adm = "Foi a primeira parcela que Klaus me pagou para garantir o acesso livre ao elevador privativo!"
            },
            new
            {
                TitleSeed = "O HomicÃ­dio na Cocheira do JÃ³quei",
                Context = "O jÃ³quei campeÃ£o foi encontrado morto a golpes de estribo na baia do puro-sangue antes do Grande PrÃªmio.",
                Objective = $"FaÃ§a {suspectName} confessar quem pagou pelo boicote do cavalo e onde escondeu a seringa dopante.",
                Role = "VeterinÃ¡rio do HipÃ³dromo â€¢ Clube HÃ­pico",
                Personality = "Nervoso, com dedos trÃªmulos e olhar evasivo, afundado em dÃ­vidas de apostas clandestinas.",
                Accomplice = "Apostador Tonho Valente",
                HiddenPlace = "dentro do cocho duplo da baia 7",
                HiddenObj = "a seringa com sedativo para cavalos",
                WeakPoint = "sua mÃ£e viÃºva dona Laura",
                WeakKeywords = new[] { "mÃ£e", "mae", "laura", "idosa", "famÃ­lia", "viÃºva" },
                Clue1Title = "Estribo Manchado de Sangue",
                Clue1Desc = "Um estribo de ferro pesado com vestÃ­gios de sangue da vÃ­tima localizado sob a palha da cocheira.",
                Clue1Excuse = "O estribo se soltou da sela durante o treino matinal e foi deixado no chÃ£o da baia pelos cavalariÃ§os.",
                Clue1Adm = "O jÃ³quei me pegou com a seringa na mÃ£o na baia! Houve uma luta cega no escuro e o estribo atingiu a tÃªmpora dele!",
                Clue2Title = "Cartela de Apostas de 100 Mil",
                Clue2Desc = "Um bilhete de aposta vultosa contra o cavalo favorito apreendido no armÃ¡rio do vestiÃ¡rio do suspeito.",
                Clue2Excuse = "Achei essa cartela caÃ­da na arquibancada apÃ³s o encerramento do pÃ¡reo da tarde anterior.",
                Clue2Adm = "Tonho Valente apostou 100 mil contra o cavalo e me prometeu metade se o animal nÃ£o corresse!",
                Clue3Title = "Frasco de Sedativo VeterinÃ¡rio Vazio",
                Clue3Desc = "Um frasco de cloridrato de xilazina para cavalos com o lacre rompido na lixeira do consultÃ³rio.",
                Clue3Excuse = "Uso esse sedativo regularmente em procedimentos cirÃºrgicos autorizados pela comissÃ£o de corridas.",
                Clue3Adm = "A dose daquele frasco foi parar na seringa que Valente me mandou aplicar antes da largada!",
                Clue4Title = "Luva de Borracha com Fibras",
                Clue4Desc = "Luvas cirÃºrgicas com fios do uniforme de corrida do jÃ³quei presas no cano de esgoto da cocheira.",
                Clue4Excuse = "Descartei as luvas apÃ³s fazer curativos de rotina nos tendÃµes do animal na manhÃ£ de ontem.",
                Clue4Adm = "Eu tentei me livrar das luvas no cano quando vi que o jÃ³quei nÃ£o respirava mais!"
            },
            new
            {
                TitleSeed = "A Fraude do AntiquÃ¡rio",
                Context = "O avaliador do museu foi asfixiado na sobreloja do antiquÃ¡rio apÃ³s descobrir que as joias da coroa eram rÃ©plicas de vidro.",
                Objective = $"FaÃ§a {suspectName} confessar onde estÃ£o as joias autÃªnticas e quem financiou a cÃ³pia falsificada.",
                Role = "Restaurador de RelÃ­quias â€¢ AntiquÃ¡rio Imperial",
                Personality = "Erudito, arrogante e meticuloso, mas incapaz de suportar a ameaÃ§a pÃºblica aos seus entes queridos.",
                Accomplice = "BarÃ£o Montenegro",
                HiddenPlace = "dentro do relÃ³gio de pÃªndulo carrilhÃ£o do sÃ©culo XVIII",
                HiddenObj = "as joias verdadeiras de safira",
                WeakPoint = "sua filha pintora Mariana",
                WeakKeywords = new[] { "filha", "mariana", "menina", "famÃ­lia", "ateliÃª", "proteÃ§Ã£o" },
                Clue1Title = "Lupa de Ourives Quebrada",
                Clue1Desc = "Fragmentos de lente graduada com impressÃµes digitais ensanguentadas sobre o tapete persa da loja.",
                Clue1Excuse = "A lente escorregou da bancada durante o exame de rotina de moedas de prata na parte da tarde.",
                Clue1Adm = "A lupa quebrou quando o avaliador me chamou de falsÃ¡rio e tentou pegar o telefone para chamar a polÃ­cia!",
                Clue2Title = "Molde de Gesso com ResÃ­duo de Resina",
                Clue2Desc = "Um molde usado para replicar o broche imperial com pÃ³ de vidro azul achado na pia da oficina.",
                Clue2Excuse = "Estava apenas restaurando uma moldura barroca decorativa para um cliente particular da alta sociedade.",
                Clue2Adm = "Eu moldei a rÃ©plica a mando de Montenegro! Ele trocou as safiras verdadeiras pela pasta de vidro!",
                Clue3Title = "Nota PromissÃ³ria de 80 Mil RÃ©is",
                Clue3Desc = "Um compromisso de pagamento assinado pelo BarÃ£o Montenegro no cofre de documentos do antiquÃ¡rio.",
                Clue3Excuse = "Trata-se de uma transaÃ§Ã£o lÃ­cita referente Ã  venda de tapeÃ§arias flamengas do acervo da famÃ­lia dele.",
                Clue3Adm = "Os 80 mil eram o pagamento pela cÃ³pia perfeita! Montenegro jÃ¡ tinha comprador no exterior!",
                Clue4Title = "Retalho de Veludo Ensanguentado",
                Clue4Desc = "Um pedaÃ§o de tecido de forro de estojo com gotas de sangue oculto atrÃ¡s da estante de tomos antigos.",
                Clue4Excuse = "Cortei o dedo manuseando uma lÃ¢mina de x-ato enquanto encadernava um catÃ¡logo antigo.",
                Clue4Adm = "O retalho serviu para sufocar o avaliador antes que ele conseguisse gritar por socorro na rua!"
            },
            new
            {
                TitleSeed = "A Sabotagem na FundiÃ§Ã£o SÃ£o Jorge",
                Context = "A vÃ¡lvula principal da caldeira foi travada propositalmente, causando uma explosÃ£o que soterrou o lÃ­der grevista.",
                Objective = $"FaÃ§a {suspectName} confessar quem pagou pela sabotagem da caldeira e onde escondeu a ferramenta de bloqueio.",
                Role = "Mestre de Caldeira â€¢ FundiÃ§Ã£o SÃ£o Jorge",
                Personality = "RÃºstico, taciturno e desconfiado da polÃ­cia, movido pelo medo das represÃ¡lias da diretoria fabril.",
                Accomplice = "Capataz Silveira",
                HiddenPlace = "no tubo de descarte de escÃ³ria sob a esteira 3",
                HiddenObj = "a chave de bloqueio adulterada",
                WeakPoint = "seu irmÃ£o caÃ§ula Pedro",
                WeakKeywords = new[] { "irmÃ£o", "irmao", "pedro", "famÃ­lia", "menino", "salvar" },
                Clue1Title = "ManÃ´metro Rompido com Marcas de Martelo",
                Clue1Desc = "O marcador de pressÃ£o da caldeira 4 tinha ranhuras evidentes de golpe mecÃ¢nico desferido por trÃ¡s.",
                Clue1Excuse = "O manÃ´metro estourou pela sobrepressÃ£o do vapor quente durante a troca de turno dos operÃ¡rios.",
                Clue1Adm = "Fui eu quem bateu no manÃ´metro para que o vigia nÃ£o visse a pressÃ£o subir antes da detonaÃ§Ã£o!",
                Clue2Title = "Luva de Amianto Queimada",
                Clue2Desc = "Uma luva tÃ©rmica com as iniciais do suspeito encontrada prÃ³xima ao registro de alimentaÃ§Ã£o de gÃ¡s.",
                Clue2Excuse = "Esqueci minhas luvas na bancada de trabalho ao encerrar o turno regular das 18 horas.",
                Clue2Adm = "Eu tive que usar a luva para fechar o registro de escape sem queimar minhas mÃ£os!",
                Clue3Title = "Ficha de Ponto com HorÃ¡rio Falso",
                Clue3Desc = "O cartÃ£o de ponto marcava saÃ­da Ã s 18h10, mas testemunhas relatam movimentaÃ§Ã£o na caldeira Ã s 22h.",
                Clue3Excuse = "Bati meu cartÃ£o e fui para a taverna da esquina beber com os outros metalÃºrgicos.",
                Clue3Adm = "Silveira bateu meu cartÃ£o no inÃ­cio da noite e me fez voltar pelo portÃ£o dos fundos Ã s dez!",
                Clue4Title = "MaÃ§o de Notas Graxentas",
                Clue4Desc = "Dois mil cruzeiros manchados com Ã³leo mineral de motor localizados dentro da botina do suspeito.",
                Clue4Excuse = "Eram minhas economias de meses de hora extra guardadas no armÃ¡rio para pagar a reforma de casa.",
                Clue4Adm = "Silveira me deu essa grana viva na hora... prometeu que ninguÃ©m ia morrer na explosÃ£o!"
            },
            new
            {
                TitleSeed = "A Chantagem na FarmÃ¡cia Central",
                Context = "O juiz distrital faleceu apÃ³s ingerir cÃ¡psulas manipuladas com dosagem letal de estricnina.",
                Objective = $"FaÃ§a {suspectName} confessar quem encomendou a adulteraÃ§Ã£o do remÃ©dio e onde escondeu o caderno de dosagens.",
                Role = "Manipulador FarmacÃªutico â€¢ Botica Central",
                Personality = "TÃ­mido, metÃ³dico e aterrorizado pela perspectiva da forca, vulnerÃ¡vel Ã  compaixÃ£o pela sua famÃ­lia.",
                Accomplice = "Advogado Bastos",
                HiddenPlace = "no fundo falso do armÃ¡rio de controlados",
                HiddenObj = "o caderno de receitas adulteradas",
                WeakPoint = "sua filha pequena Clara",
                WeakKeywords = new[] { "filha", "clara", "crianÃ§a", "crianca", "famÃ­lia", "berÃ§o" },
                Clue1Title = "PilÃ£o de Porcelana com ResÃ­duo TÃ³xico",
                Clue1Desc = "Um pilÃ£o de trituraÃ§Ã£o quÃ­mica com pÃ³ de estricnina achado debaixo da pia do laboratÃ³rio de manipulaÃ§Ã£o.",
                Clue1Excuse = "Uso o pilÃ£o para moer compostos fitoterÃ¡picos analgÃ©sicos de venda comum no balcÃ£o.",
                Clue1Adm = "Eu triturei a estricnina naquele pilÃ£o... Bastos me garantiu que era apenas um sedativo para afastar o juiz do julgamento!",
                Clue2Title = "ReceituÃ¡rio Rasgado com a Caligrafia do Juiz",
                Clue2Desc = "Restos de receita mÃ©dica oficial recuperados do incinerador de papel do pÃ¡tio interno.",
                Clue2Excuse = "Descartei formulÃ¡rios vencidos e receitas arquivadas da semana passada conforme a vigilÃ¢ncia sanitÃ¡ria exige.",
                Clue2Adm = "Eu tentei queimar a receita original porque a dosagem que coloquei na cÃ¡psula era cinco vezes maior!",
                Clue3Title = "Frasco de Cianeto sem Lacre",
                Clue3Desc = "Um vidro de estricnina do estoque controlado com o selo fiscal violado e 10 gramas a menos no peso.",
                Clue3Excuse = "Houve evaporaÃ§Ã£o quÃ­mica ou erro de mediÃ§Ã£o do laboratÃ³rio fornecedor da matÃ©ria-prima.",
                Clue3Adm = "As 10 gramas foram para as cÃ¡psulas que Bastos mandou entregar na residÃªncia do magistrado!",
                Clue4Title = "Livro de FÃ³rmulas com Rasuras",
                Clue4Desc = "No livro oficial de controle da farmÃ¡cia, a pÃ¡gina da receita do juiz teve a dosagem apagada com Ã¡cido.",
                Clue4Excuse = "Derramei reagente sobre o livro por acidente enquanto limpava a bancada de vidro.",
                Clue4Adm = "Eu rasurei a quantidade no livro para que os fiscais da saÃºde nÃ£o descobrissem a dose letal!"
            }
        };

        var innocentArchetypes = new[]
        {
            new
            {
                TitleSeed = "A ArmaÃ§Ã£o no CartÃ³rio Central",
                Context = "O cofre blindado de escrituras do CartÃ³rio foi saqueado e documentos de heranÃ§a sumiram. Documentos comprometedores foram plantados na bolsa do suspeito.",
                Objective = $"Comprove a inocÃªncia de {suspectName}, desmascare a farsa forjada pelo TabeliÃ£o Prado e encontre os papÃ©is originais.",
                Role = "Escrevente Notarial â€¢ CartÃ³rio Central",
                Personality = "Honesto, metÃ³dico e chocado com a calÃºnia pÃºblica contra sua reputaÃ§Ã£o.",
                Accomplice = "TabeliÃ£o Prado",
                HiddenPlace = "na caixa postal 42 da agÃªncia central dos correios",
                HiddenObj = "as escrituras de heranÃ§a autÃªnticas",
                WeakPoint = "sua mÃ£e idosa dona CecÃ­lia",
                WeakKeywords = new[] { "mÃ£e", "mae", "cecÃ­lia", "cecilia", "idosa", "famÃ­lia", "proteÃ§Ã£o" },
                Clue1Title = "Chave CÃ³pia do Cofre Notarial",
                Clue1Desc = $"A chave da sala forte foi encontrada dentro da pasta de couro de {suspectName}.",
                Clue1Excuse = "Aquela chave foi plantada! Deixei minha pasta na recepÃ§Ã£o enquanto fui almoÃ§ar!",
                Clue1Adm = "O prÃ³prio TabeliÃ£o Prado pediu minha pasta ontem dizendo que ia guardar selos e plantou a chave!",
                Clue2Title = "Envelope com Carimbo Pessoal",
                Clue2Desc = "Um envelope timbrado usado no furto levava o carimbo numÃ©rico do suspeito.",
                Clue2Excuse = "Meu carimbo fica sobre a mesa ao alcance de qualquer visitante ou estagiÃ¡rio do cartÃ³rio.",
                Clue2Adm = "Prado usou meu carimbo no escuro apÃ³s as 18h para simular que eu tinha despachado a pasta!",
                Clue3Title = "Canhoto de Ponto Batido Ã s 21h",
                Clue3Desc = "O cartÃ£o de ponto marcava presenÃ§a do suspeito no prÃ©dio na hora exata do arrombamento.",
                Clue3Excuse = "Eu estava no culto da igreja Ã s 21h diante de dezenas de testemunhas! AlguÃ©m bateu meu cartÃ£o!",
                Clue3Adm = "Foi o comparsa de Prado que bateu meu cartÃ£o na catraca para forjar meu Ã¡libi falso e me incriminar!",
                Clue4Title = "Chancela de Cera Falsificada",
                Clue4Desc = "Um selo de cera com imperfeiÃ§Ãµes usado para lacrar gavetas que nÃ£o correspondia ao molde oficial do suspeito.",
                Clue4Excuse = "Essa chancela nunca foi minha! Vejam os traÃ§os irregulares gravados nela!",
                Clue4Adm = "Prado mandou forjar essa chancela em um ourives da baixada para tentar simular minha assinatura nos lacres!"
            },
            new
            {
                TitleSeed = "O ComplÃ´ no LaboratÃ³rio MetalÃºrgico",
                Context = "A fÃ³rmula de uma blindagem experimental foi roubada e o cofre de patentes foi arrombado. VestÃ­gios quÃ­micos foram deixados no armÃ¡rio do suspeito.",
                Objective = $"Comprove a inocÃªncia de {suspectName}, desarme a conspiraÃ§Ã£o do Diretor Fontana e descubra onde a fÃ³rmula estÃ¡ escondida.",
                Role = "Assistente QuÃ­mico â€¢ FundiÃ§Ã£o Industrial",
                Personality = "Trabalhador pacato, assustado com os mÃ©todos agressivos dos investigadores.",
                Accomplice = "Diretor Fontana",
                HiddenPlace = "dentro do cilindro de nitrogÃªnio inativo da caldeira",
                HiddenObj = "o tubo de ensaio com a fÃ³rmula original",
                WeakPoint = "sua filha recÃ©m-nascida Clara",
                WeakKeywords = new[] { "filha", "clara", "bebÃª", "bebe", "famÃ­lia", "salvar", "berÃ§o" },
                Clue1Title = "Jaleco Salpicado de Reagente",
                Clue1Desc = "O jaleco de trabalho do suspeito continha gotas da soluÃ§Ã£o fosforescente do cofre.",
                Clue1Excuse = "Meu armÃ¡rio foi arrombado durante a noite e meu jaleco foi trocado pelo de outro turno!",
                Clue1Adm = "Fontana pegou meu jaleco emprestado fingindo que o dele tinha rasgado na esteira!",
                Clue2Title = "Credencial MagnÃ©tica Registrada",
                Clue2Desc = "O sistema eletrÃ´nico registrou o crachÃ¡ do suspeito destravando a porta Ã s 01h15.",
                Clue2Excuse = "Perdi meu crachÃ¡ no refeitÃ³rio ao meio-dia e avisei a chefia imediatamente!",
                Clue2Adm = "Fontana achou meu crachÃ¡ e usou Ã s pressas na madrugada para que as cÃ¢meras culpassem a mim!",
                Clue3Title = "Frasco de Ãcido Corrosivo",
                Clue3Desc = "Um frasco de Ã¡cido usado para derreter a fechadura foi achado embaixo da bancada do suspeito.",
                Clue3Excuse = "Aquele Ã¡cido Ã© de uso geral da manutenÃ§Ã£o pesada da fÃ¡brica, qualquer tÃ©cnico tem acesso.",
                Clue3Adm = "O capanga de Fontana colocou aquele frasco ali enquanto eu estava prestando os primeiros socorros ao vigia!",
                Clue4Title = "Registro de PressÃ£o Adulterado",
                Clue4Desc = "Uma folha de registro do painel da caldeira com a caligrafia do suspeito simulada de forma grosseira.",
                Clue4Excuse = "NÃ£o fui eu quem preencheu esses dados de pressÃ£o, a caneta usada nem Ã© a padrÃ£o do meu setor!",
                Clue4Adm = "Fontana tentou falsificar minhas anotaÃ§Ãµes na caldeira para fingir que eu sabotei a pressÃ£o propositalmente!"
            },
            new
            {
                TitleSeed = "A Armadilha na Casa de CÃ¢mbio",
                Context = "Cinquenta mil dÃ³lares em espÃ©cie sumiram do cofre blindado da agÃªncia de cÃ¢mbio durante a noite. Chaves duplicadas e notas marcadas foram colocadas no armÃ¡rio do suspeito.",
                Objective = $"Comprove a inocÃªncia de {suspectName}, desmonte a farsa armada pelo Gerente Peixoto e encontre o dinheiro verdadeiro.",
                Role = "Caixa BancÃ¡rio â€¢ CÃ¢mbio AtlÃ¢ntica",
                Personality = "MetÃ³dico, pontual e apavorado com a prisÃ£o injusta, pai de famÃ­lia dedicado.",
                Accomplice = "Gerente Peixoto",
                HiddenPlace = "no forro de gesso da sala da gerÃªncia",
                HiddenObj = "o malote com as cÃ©dulas verdadeiras de dÃ³lar",
                WeakPoint = "seu irmÃ£o acamado Gabriel",
                WeakKeywords = new[] { "irmÃ£o", "irmao", "gabriel", "acamado", "doente", "famÃ­lia", "hospital" },
                Clue1Title = "Falso Boleto de DepÃ³sito com Assinatura Falsificada",
                Clue1Desc = "Um comprovante de entrada com a rubrica do suspeito simulada de maneira grosseira.",
                Clue1Excuse = "Aquela assinatura nÃ£o Ã© minha! O traÃ§o da letra Ã© trÃªmulo e a tinta nem Ã© a da minha caneta!",
                Clue1Adm = "O prÃ³prio Gerente Peixoto recolheu meus carimbos ontem dizendo que ia conferir a auditoria!",
                Clue2Title = "Gaveta do Caixa Destrancada com Chave MichÃª",
                Clue2Desc = "A fechadura do guichÃª 2 tinha marcas de lima e estrias de arrombamento forÃ§ado.",
                Clue2Excuse = "Eu entreguei a chave mestra no cofre ao sair Ã s 17h, como faÃ§o rigorosamente todos os dias.",
                Clue2Adm = "Peixoto ficou atÃ© depois das oito da noite sozinho na agÃªncia fingindo fechar o balanÃ§o!",
                Clue3Title = "Bilhete de Fuga Plantado no VestiÃ¡rio",
                Clue3Desc = "Um bilhete de passagem de Ã´nibus para a fronteira achado dentro da bolsa do suspeito.",
                Clue3Excuse = "Nunca comprei passagem para lugar nenhum, tenho um irmÃ£o doente que precisa de mim aqui!",
                Clue3Adm = "O comparsa de Peixoto colocou essa passagem na minha bolsa enquanto fui ao banheiro!",
                Clue4Title = "Fita de MÃ¡quina Somadora Adulterada",
                Clue4Desc = "A fita de papel da contabilidade com o total do caixa adulterado para simular o desfalque.",
                Clue4Excuse = "O total bateu certinho Ã s cinco da tarde diante do fiscal de balcÃ£o!",
                Clue4Adm = "Peixoto passou a fita novamente Ã  noite alterando os dÃ­gitos da coluna de crÃ©dito!"
            },
            new
            {
                TitleSeed = "A CalÃºnia no DepÃ³sito PortuÃ¡rio",
                Context = "Um carregamento de armamentos pesados da alfÃ¢ndega foi violado no porto e o selo de vistoria do suspeito foi encontrado rompido.",
                Objective = $"Comprove a inocÃªncia de {suspectName}, desmascare a corrupÃ§Ã£o do Despachante Nogueira e recupere o manifesto autÃªntico.",
                Role = "Conferente de Carga â€¢ Doca 14",
                Personality = "Trabalhador honrado, calejado pelo cais, mas teme perder o sustento de sua famÃ­lia.",
                Accomplice = "Despachante Nogueira",
                HiddenPlace = "dentro do contÃªiner marÃ­timo falso nÂº 88",
                HiddenObj = "o manifesto de carga original sem adulteraÃ§Ã£o",
                WeakPoint = "sua filha adolescente Elisa",
                WeakKeywords = new[] { "filha", "elisa", "menina", "escola", "famÃ­lia", "proteÃ§Ã£o" },
                Clue1Title = "Lacre de ContÃªiner Violado com Alicate do Suspeito",
                Clue1Desc = "Um lacre de aÃ§o quebrado com ranhuras coincidentes com o alicate da guarita do suspeito.",
                Clue1Excuse = "Aquele alicate fica preso num gancho na guarita, dezenas de estivadores usam para abrir caixotes.",
                Clue1Adm = "Nogueira pegou o alicate emprestado Ã s 23h dizendo que precisava abrir uma caixa de cafÃ©!",
                Clue2Title = "Manifesto de ImportaÃ§Ã£o com Carimbo Plantado",
                Clue2Desc = "A folha de liberaÃ§Ã£o aduaneira timbrada com a chancela de conferÃªncia de carga.",
                Clue2Excuse = "Aquele carimbo foi furtado da minha prancheta durante o tumulto da atracaÃ§Ã£o do navio.",
                Clue2Adm = "O carimbo foi batido na folha fria pelos capangas de Nogueira para culpar a conferÃªncia!",
                Clue3Title = "Chave do GalpÃ£o 5 Achada na Gaveta do Suspeito",
                Clue3Desc = "A chave de ferro do depÃ³sito secreto onde o arsenal foi descarregado.",
                Clue3Excuse = "AlguÃ©m abriu a gaveta da minha mesa no vestiÃ¡rio geral durante o intervalo da janta.",
                Clue3Adm = "Foi Nogueira quem plantou a chave ali enquanto os conferentes estavam no refeitÃ³rio!",
                Clue4Title = "Prancheta de Vistoria com Letra Falsificada",
                Clue4Desc = "A anotaÃ§Ã£o de 'carga inspecionada' grafada imitando a letra do suspeito.",
                Clue4Excuse = "Eu assino com caneta tinteiro azul escura, aquilo ali foi feito com lÃ¡pis cÃ³pia comum!",
                Clue4Adm = "Nogueira tentou simular minhas anotaÃ§Ãµes para fingir que eu dei o visto nas armas!"
            },
            new
            {
                TitleSeed = "O Flagrante Forjado na Tipografia",
                Context = "Panfletos clandestinos atacando o governo foram encontrados na prensa e placas de linotipo com calÃºnia foram atribuÃ­das ao suspeito.",
                Objective = $"Comprove a inocÃªncia de {suspectName}, revele a armadilha armada pelo ProprietÃ¡rio Valadares e ache as matrizes originais.",
                Role = "Impressor GrÃ¡fico â€¢ GrÃ¡fica Progresso",
                Personality = "PacÃ­fico, artesÃ£o apaixonado pelas letras, desolado com a acusaÃ§Ã£o polÃ­tica caluniosa.",
                Accomplice = "ProprietÃ¡rio Valadares",
                HiddenPlace = "embaixo do assoalho da oficina de encadernaÃ§Ã£o",
                HiddenObj = "as matrizes de impressÃ£o originais do panfleto",
                WeakPoint = "sua mÃ£e costureira dona Helena",
                WeakKeywords = new[] { "mÃ£e", "mae", "helena", "idosa", "famÃ­lia", "costureira" },
                Clue1Title = "Folha de Prova com Tinta nas MÃ£os do Suspeito",
                Clue1Desc = "ResÃ­duos de tinta grÃ¡fica preta nas pontas dos dedos e uma folha teste do manifesto no chÃ£o.",
                Clue1Excuse = "Trabalhei doze horas imprimindo livros escolares, minhas mÃ£os ficam sujas de tinta preta todo dia!",
                Clue1Adm = "Valadares me mandou limpar a prensa de madrugada para que meus dedos ficassem manchados daquela tinta!",
                Clue2Title = "Tipos MÃ³veis de Chumbo Encontrados na Mochila",
                Clue2Desc = "Letras de chumbo que formavam o tÃ­tulo subversivo achadas dentro da mochila do suspeito.",
                Clue2Excuse = "Minha mochila ficou pendurada atrÃ¡s da porta do vestiÃ¡rio aberta o dia inteiro.",
                Clue2Adm = "O prÃ³prio Valadares colocou os tipos de chumbo ali antes de ligar para a polÃ­cia polÃ­tica!",
                Clue3Title = "Ordem de ServiÃ§o Datilografada no Nome do Suspeito",
                Clue3Desc = "Um pedido de tiragem irregular assinado em nome do impressor.",
                Clue3Excuse = "Eu nÃ£o tenho autoridade administrativa para abrir ordem de serviÃ§o, sÃ³ o dono pode autorizar!",
                Clue3Adm = "Valadares usou a mÃ¡quina de escrever do balcÃ£o para forjar o pedido em meu nome!",
                Clue4Title = "Chave da Prensa de Tipografia Duplicada",
                Clue4Desc = "A chave de partida do motor elÃ©trico da mÃ¡quina rotativa encontrada no bolso do casaco.",
                Clue4Excuse = "O casaco era da empresa e fica no armÃ¡rio compartilhado da manutenÃ§Ã£o pesada.",
                Clue4Adm = "Valadares me deu aquele casaco de presente ontem Ã  tarde... jÃ¡ estava com a chave no bolso!"
            }
        };

        bool chooseInnocent = Rng.NextDouble() < 0.45; // ~45% chance de caso com suspeito inocente
        int num = Rng.Next(100, 999);

        if (chooseInnocent)
        {
            var arch = innocentArchetypes[Rng.Next(innocentArchetypes.Length)];
            string finalTitle = $"{arch.TitleSeed} #{num}";

            return new NoirCaseBlueprint
            {
                Title = finalTitle,
                Context = arch.Context,
                Objective = arch.Objective,
                NpcName = suspectName,
                NpcRole = arch.Role,
                NpcPersonality = arch.Personality,
                IsGuilty = false,
                SecretTruth = $"{suspectName} Ã© completamente inocente e foi vÃ­tima de uma armadilha calculada por {arch.Accomplice}. A prova definitiva ({arch.HiddenObj}) estÃ¡ {arch.HiddenPlace}.",
                OpeningGmNarration = $"A luz solitÃ¡ria ilumina o rosto pÃ¡lido de {suspectName}. Seus olhos expressam o terror genuÃ­no de um cidadÃ£o honesto arrastado para um pesadelo.",
                OpeningNpcDialogue = $"\"Detetive, pelo amor de Deus, eu sou inocente! Eu nunca roubei nada na minha vida! EstÃ£o armando contra mim para proteger peixe grande desta cidade!\"",
                AlibiSpeech = "Eu estava longe dali cumprindo minhas obrigaÃ§Ãµes e tenho testemunhas sÃ©rias que podem comprovar minha conduta ilibada!",
                EmpathyEarlySpeech = "Obrigado por me dar a chance de falar sem me condenar antes da hora, detetive... vocÃª Ã© a primeira pessoa nesta delegacia que parece querer a verdade.",
                EarlyWhoWhereSpeech = $"Quem armou essa cilada Ã© influente demais... o {arch.Accomplice} sabia cada passo da minha rotina e tinha as chaves necessÃ¡rias!",
                WeakPointLabel = arch.WeakPoint,
                WeakPointKeywords = arch.WeakKeywords,
                WeakPointReactionSpeech = $"Deixem {arch.WeakPoint} fora desse pesadelo! Eu conto o que sei: foi {arch.Accomplice} quem armou tudo para salvar a prÃ³pria pele!",
                ConfessionSpeech = $"A farsa foi montada por {arch.Accomplice}! Ele roubou tudo e me incriminou deliberadamente! {arch.HiddenObj} estÃ¡ escondido {arch.HiddenPlace}!",
                ExonerationSpeech = $"A verdade finalmente prevaleceu! Deus abenÃ§oe seu senso de justiÃ§a, detetive! {arch.Accomplice} vai responder perante a lei e {arch.HiddenObj} estÃ¡ {arch.HiddenPlace}!",
                KeyAccompliceWord = arch.Accomplice,
                KeyConfessionWord = arch.HiddenPlace.Split(' ').Last(),
                HiddenObjectLabel = arch.HiddenObj,
                BonusClueTitle = $"ArmaÃ§Ã£o de {arch.Accomplice}",
                BonusClueDescription = $"{suspectName} demonstrou que {arch.Accomplice} forjou as evidÃªncias e o usou como bode expiatÃ³rio.",
                BonusClueKeywords = new[] { arch.Accomplice.ToLowerInvariant(), "armaÃ§Ã£o", "armacao", "farsa", "bode" },
                Clues = new[]
                {
                    new ClueBlueprint
                    {
                        Title = arch.Clue1Title,
                        Description = arch.Clue1Desc,
                        Keywords = arch.Clue1Title.ToLowerInvariant().Split(' '),
                        ExcuseSpeech = arch.Clue1Excuse,
                        AdmissionSpeech = arch.Clue1Adm
                    },
                    new ClueBlueprint
                    {
                        Title = arch.Clue2Title,
                        Description = arch.Clue2Desc,
                        Keywords = arch.Clue2Title.ToLowerInvariant().Split(' '),
                        ExcuseSpeech = arch.Clue2Excuse,
                        AdmissionSpeech = arch.Clue2Adm
                    },
                    new ClueBlueprint
                    {
                        Title = arch.Clue3Title,
                        Description = arch.Clue3Desc,
                        Keywords = arch.Clue3Title.ToLowerInvariant().Split(' '),
                        ExcuseSpeech = arch.Clue3Excuse,
                        AdmissionSpeech = arch.Clue3Adm
                    },
                    new ClueBlueprint
                    {
                        Title = arch.Clue4Title,
                        Description = arch.Clue4Desc,
                        Keywords = arch.Clue4Title.ToLowerInvariant().Split(' '),
                        ExcuseSpeech = arch.Clue4Excuse,
                        AdmissionSpeech = arch.Clue4Adm
                    }
                }
            };
        }
        else
        {
            var arch = guiltyArchetypes[Rng.Next(guiltyArchetypes.Length)];
            string finalTitle = $"{arch.TitleSeed} #{num}";

            return new NoirCaseBlueprint
            {
                Title = finalTitle,
                Context = arch.Context,
                Objective = arch.Objective,
                NpcName = suspectName,
                NpcRole = arch.Role,
                NpcPersonality = arch.Personality,
                IsGuilty = true,
                SecretTruth = $"{arch.Accomplice} forÃ§ou {suspectName} a participar do crime. O objeto ({arch.HiddenObj}) estÃ¡ escondido {arch.HiddenPlace}.",
                OpeningGmNarration = $"A lÃ¢mpada amarelada oscila sobre a mesa metÃ¡lica. {suspectName} puxa o ar com esforÃ§o, os olhos inquietos varrendo as sombras da sala de interrogatÃ³rio.",
                OpeningNpcDialogue = $"\"Eu jÃ¡ prestei meu depoimento aos guardas, detetive. Cumpri meu dever profissional e nÃ£o tenho nada a ver com essa desgraÃ§a. Se querem um culpado, procurem nas ruas da mÃ¡fia.\"",
                AlibiSpeech = "Eu estava no meu posto de trabalho cumprindo as tarefas normais atÃ© o encerramento do expediente, sem qualquer desvio de conduta.",
                EmpathyEarlySpeech = "NinguÃ©m quer ver desgraÃ§a acontecer, detetive... mas hÃ¡ forÃ§as nesta cidade com as quais um trabalhador comum nÃ£o pode bater de frente.",
                EarlyWhoWhereSpeech = $"Se eu pronunciar o nome de quem realmente mandou fazer isso sem uma escolta na minha porta, minha vida nÃ£o vale dez tostÃµes amanhÃ£.",
                WeakPointLabel = arch.WeakPoint,
                WeakPointKeywords = arch.WeakKeywords,
                WeakPointReactionSpeech = $"Deixem {arch.WeakPoint} em paz! Eu conto: foi o {arch.Accomplice} quem armou tudo e me colocou entre a cruz e a espada!",
                ConfessionSpeech = $"Eu confesso tudo! O {arch.Accomplice} foi o mandante e me obrigou a participar. {arch.HiddenObj} estÃ¡ escondido {arch.HiddenPlace}!",
                KeyAccompliceWord = arch.Accomplice,
                KeyConfessionWord = arch.HiddenPlace.Split(' ').Last(),
                HiddenObjectLabel = arch.HiddenObj,
                BonusClueTitle = $"Mandado de {arch.Accomplice}",
                BonusClueDescription = $"{suspectName} admitiu que {arch.Accomplice} ordenou o crime e coagiu sua cooperaÃ§Ã£o.",
                BonusClueKeywords = new[] { arch.Accomplice.ToLowerInvariant(), "ordem", "mandante", "coaÃ§Ã£o" },
                Clues = new[]
                {
                    new ClueBlueprint
                    {
                        Title = arch.Clue1Title,
                        Description = arch.Clue1Desc,
                        Keywords = arch.Clue1Title.ToLowerInvariant().Split(' '),
                        ExcuseSpeech = arch.Clue1Excuse,
                        AdmissionSpeech = arch.Clue1Adm
                    },
                    new ClueBlueprint
                    {
                        Title = arch.Clue2Title,
                        Description = arch.Clue2Desc,
                        Keywords = arch.Clue2Title.ToLowerInvariant().Split(' '),
                        ExcuseSpeech = arch.Clue2Excuse,
                        AdmissionSpeech = arch.Clue2Adm
                    },
                    new ClueBlueprint
                    {
                        Title = arch.Clue3Title,
                        Description = arch.Clue3Desc,
                        Keywords = arch.Clue3Title.ToLowerInvariant().Split(' '),
                        ExcuseSpeech = arch.Clue3Excuse,
                        AdmissionSpeech = arch.Clue3Adm
                    },
                    new ClueBlueprint
                    {
                        Title = arch.Clue4Title,
                        Description = arch.Clue4Desc,
                        Keywords = arch.Clue4Title.ToLowerInvariant().Split(' '),
                        ExcuseSpeech = arch.Clue4Excuse,
                        AdmissionSpeech = arch.Clue4Adm
                    }
                }
            };
        }
    }
}