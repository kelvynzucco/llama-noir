using System;
using System.Collections.Generic;
using System.Linq;
using LlamaNoir.Models;

namespace LlamaNoir.Services;

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
    /// Gera proceduralmente um caso Noir único e inédito para a sessão.
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
    /// Sintetizador procedural que monta dinamicamente casos infinitos combinando arquétipos,
    /// motivos, provas e segredos sem nunca repetir a mesma configuração.
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
                TitleSeed = "O Enigma do Cofre da Estação",
                Context = "O tesoureiro da linha férrea foi encontrado inconsciente e o malote de 150 mil da folha de pagamento desapareceu.",
                Objective = $"Faça {suspectName} confessar quem forçou o cofre e onde escondeu as chaves mestras e o dinheiro.",
                Role = "Despachante Ferroviário • Estação Central",
                Personality = "Pragmático e cansado, mas teme a ruína financeira de sua família.",
                Accomplice = "Inspetor Cordeiro",
                HiddenPlace = "dentro da caldeira da locomotiva nº 9",
                HiddenObj = "o malote com 150 mil",
                WeakPoint = "sua neta Maria",
                WeakKeywords = new[] { "neta", "maria", "família", "menina", "salvar", "hospital" },
                Clue1Title = "Chave Inglesa com Sangue Seco",
                Clue1Desc = $"Uma chave pesada pertencente à oficina de {suspectName} foi localizada atrás da bilheteria.",
                Clue1Excuse = "Aquela chave sumiu da minha caixa de ferramentas na última quarta-feira, qualquer foguista teve acesso.",
                Clue1Adm = "Sim... a chave era minha! Fui eu quem desferiu o golpe quando o tesoureiro tentou puxar o alarme!",
                Clue2Title = "Bilhete de Passagem Rasgado",
                Clue2Desc = "Um tíquete de fuga para o porto na madrugada com horário marcado para as 03h10.",
                Clue2Excuse = "Comprei a passagem apenas para visitar parentes no litoral durante minha folga regulamentar.",
                Clue2Adm = "A passagem era para mim sim! Eu ia fugir no cargueiro das três e dez assim que entregasse a grana!",
                Clue3Title = "Graxa Especial da Tranca",
                Clue3Desc = "Vestígios de lubrificante químico militar encontrados nas unhas e punhos da camisa do suspeito.",
                Clue3Excuse = "Limpei as engrenagens da catraca de embarque antes de encerrar o expediente da noite.",
                Clue3Adm = "A graxa é do segredo do cofre! Cordeiro me deu o código e me mandou forçar a maçaneta interna!",
                Clue4Title = "Relógio de Bolso Amassado",
                Clue4Desc = "O relógio do tesoureiro travado no horário exato do assalto, com fibras de uniforme de maquinista.",
                Clue4Excuse = "O tesoureiro vivia deixando coisas no vestiário geral dos maquinistas.",
                Clue4Adm = "Eu arranquei o relógio dele durante a luta para que ninguém soubesse o minuto exato da ação!"
            },
            new
            {
                TitleSeed = "Chantagem no Jornal da Noite",
                Context = "O editor-chefe investigativo foi morto por asfixia em sua sala com os arquivos da quadrilha revirados.",
                Objective = $"Faça {suspectName} confessar quem encomendou o roubo das pastas e onde escondeu os manuscritos.",
                Role = "Chefe de Redação • Diário Popular",
                Personality = "Culto, sarcástico e tenso, com histórico de dívidas de apostas clandestinas.",
                Accomplice = "Senador Fagundes",
                HiddenPlace = "no depósito de papel de bobina do subsolo",
                HiddenObj = "os manuscritos de denúncia",
                WeakPoint = "seu filho recém-nascido Lucas",
                WeakKeywords = new[] { "filho", "lucas", "bebê", "bebe", "berço", "família", "proteção" },
                Clue1Title = "Lauda Datilografada com Sangue",
                Clue1Desc = "Uma folha de texto da máquina pessoal do suspeito encontrada sob o corpo do editor.",
                Clue1Excuse = "Aquela lauda era apenas uma matéria policial arquivada que o editor pediu para revisar antes do fechamento.",
                Clue1Adm = "A lauda caiu da minha pasta quando ele me pegou vasculhando a gaveta dele!",
                Clue2Title = "Tinteiro de Prata Quebrado",
                Clue2Desc = "Fragmentos de cristal com marcas de impacto violento no chão do gabinete da vítima.",
                Clue2Excuse = "O tinteiro escorregou da escrivaninha na tarde anterior durante uma discussão sobre prazos de pauta.",
                Clue2Adm = "O tinteiro se quebrou quando o capanga de Fagundes prensou o editor contra a parede!",
                Clue3Title = "Chave da Sala de Máquinas",
                Clue3Desc = $"A chave do depósito onde as pastas sumiram foi encontrada no bolso do casaco de {suspectName}.",
                Clue3Excuse = "Tenho a cópia daquela chave porque guardo resmas de papel e formulários administrativos lá dentro.",
                Clue3Adm = "Fui eu quem destrancou o depósito às duas da madrugada para esconder as pastas originais!",
                Clue4Title = "Bloco de Notas Rascunhado",
                Clue4Desc = "Um bloco com anotações de quantias em dinheiro e as iniciais do senador envolvido no escândalo.",
                Clue4Excuse = "São apenas estimativas de tiragem e custos de gráfica para a edição de domingo.",
                Clue4Adm = "Eram os valores do suborno que Fagundes me prometeu para queimar a reportagem!"
            },
            new
            {
                TitleSeed = "O Assassinato no Cabaré Imperial",
                Context = "A estrela do cabaré foi encontrada sem vida em seu camarim após uma briga aos fundos às 02h20.",
                Objective = $"Faça {suspectName} confessar quem contratou o crime e onde escondeu as joias furtadas.",
                Role = "Maitre e Barman • Cabaré Imperial",
                Personality = "Frio, polido e observador, mas desesperado para não ser condenado à pena capital.",
                Accomplice = "Comendador Barreto",
                HiddenPlace = "dentro do barril de uísque falso nº 12",
                HiddenObj = "as joias de esmeralda",
                WeakPoint = "sua irmã mais nova Teresa",
                WeakKeywords = new[] { "irmã", "irma", "teresa", "família", "moça", "proteção", "convento" },
                Clue1Title = "Lenço Manchado de Cosmético",
                Clue1Desc = "Um lenço fino de linho com a fragrância exclusiva da vítima e marcas de dedos ensanguentados.",
                Clue1Excuse = "Entreguei o lenço à artista durante o intervalo do primeiro ato quando ela retocava a maquiagem.",
                Clue1Adm = "O lenço foi usado para abafar os gritos dela quando Barreto mandou cobrar o colar!",
                Clue2Title = "Anel com Brasão Nobre",
                Clue2Desc = "Um anel masculino de ouro pesado caído atrás da cortina de veludo do camarim.",
                Clue2Excuse = "Aquele anel foi esquecido por algum dos clientes abonados que frequentam as mesas VIP do mezanino.",
                Clue2Adm = "O anel é do próprio Comendador Barreto... caiu da mão dele durante a luta no camarim!",
                Clue3Title = "Taça com Resíduo Químico",
                Clue3Desc = "Uma taça de cristal com gotas de sedativo para cavalos misturado ao licor servido pelo bar.",
                Clue3Excuse = "Sirvo centenas de coquetéis todas as noites, qualquer garçom pode ter trocado as bandejas na cozinha.",
                Clue3Adm = "Fui eu quem colocou as gotas no licor a mando de Barreto para que ela não resistisse ao assalto!",
                Clue4Title = "Chave da Saída de Emergência",
                Clue4Desc = "A chave de ferro dos fundos do cabaré com fibras do vestido da vítima presa no ferrolho.",
                Clue4Excuse = "A porta dos fundos fica sempre aberta para os entregadores de bebidas e músicos.",
                Clue4Adm = "Eu destranquei a porta de emergência para facilitar a fuga rápida de Barreto com as joias!"
            },
            new
            {
                TitleSeed = "Emboscada no Clube Blue Note",
                Context = "O informante 'Rato' Soares foi morto com dois tiros às 02h00 no beco atrás do Clube de Jazz Blue Note.",
                Objective = $"Faça {suspectName} confessar quem atirou em Soares e onde escondeu o livro-caixa do sindicato.",
                Role = "Gerente Noturno • Clube Blue Note",
                Personality = "Irônico e vivido na noite, tenta manter a frieza, mas entra em pânico quando a segurança de sua família é ameaçada.",
                Accomplice = "Donato Braga",
                HiddenPlace = "dentro da caixa acústica do piano de cauda do clube",
                HiddenObj = "o livro-caixa do sindicato",
                WeakPoint = "sua irmã Lúcia",
                WeakKeywords = new[] { "irmã", "irma", "lúcia", "lucia", "família", "familia", "proteger", "proteção" },
                Clue1Title = "Chave da Porta dos Fundos",
                Clue1Desc = "Uma cópia recente da chave da porta dos fundos foi encontrada caída no beco onde o corpo foi achado.",
                Clue1Excuse = "Aquela cópia sumiu do quadro da gerência há duas semanas, qualquer garçom pode ter pegado.",
                Clue1Adm = "Fui eu quem destrancou os fundos às duas em ponto! Donato jurou matar minha irmã se eu não abrisse!",
                Clue2Title = "Isqueiro de Prata Gravado",
                Clue2Desc = $"Um isqueiro gravado com as iniciais de {suspectName} achado a três metros do cadáver no beco.",
                Clue2Excuse = "Perdi esse isqueiro no balcão do bar durante o show da meia-noite, diante de dezenas de clientes.",
                Clue2Adm = "O isqueiro caiu da minha mão quando ouvi os tiros no beco e saí correndo desesperado!",
                Clue3Title = "Bilhete com Ameaça de Morte",
                Clue3Desc = "Um bilhete amassado com caligrafia recortada de jornal encontrado no bolso do terno do suspeito.",
                Clue3Excuse = "Alguém colocou esse bilhete no bolso do meu casaco na chapelaria enquanto eu atendia as mesas.",
                Clue3Adm = "O bilhete veio de Donato Braga! Ele me mandou calar a boca ou Lúcia seria a próxima vítima!",
                Clue4Title = "Fita Magnética de Gravação",
                Clue4Desc = "Um carretel de gravação do escritório com trechos de discussão entre a gerência e os capangas de Donato.",
                Clue4Excuse = "Gravamos os ensaios musicais dos músicos de jazz, não há nada criminoso naquelas fitas.",
                Clue4Adm = "Eu gravei Donato me ameaçando no escritório para ter uma garantia caso a polícia me pegasse!"
            },
            new
            {
                TitleSeed = "O Veneno no Cálice da Ópera",
                Context = "O maestro da Ópera Municipal foi envenenado com estricnina durante o intervalo do segundo ato.",
                Objective = $"Faça {suspectName} confessar quem encomendou o veneno e onde escondeu a partitura original roubada.",
                Role = "Pianista e Arranjador • Ópera Municipal",
                Personality = "Metódico, vaidoso e ansioso, com rancor profissional reprimido e dívidas de jogatina.",
                Accomplice = "Crítico Otávio Munhoz",
                HiddenPlace = "no vão do forro do mezanino 3",
                HiddenObj = "a partitura original roubada",
                WeakPoint = "seu sobrinho doente Tomás",
                WeakKeywords = new[] { "sobrinho", "tomás", "tomas", "menino", "família", "hospital", "remédio" },
                Clue1Title = "Frasco de Estricnina Farmacêutica",
                Clue1Desc = "Um frasco de vidro âmbar com resíduos de pó branco tóxico encontrado na lixeira do camarim.",
                Clue1Excuse = "Uso esse produto na manutenção química das cordas e teclas de marfim dos pianos do teatro.",
                Clue1Adm = "O pó era estricnina pura... Otávio Munhoz me entregou o frasco no saguão e me mandou pingar na taça!",
                Clue2Title = "Partitura com Anotações Manuscritas",
                Clue2Desc = "A partitura do maestro com comentários sarcásticos e anotações técnicas feitas com a caligrafia do suspeito.",
                Clue2Excuse = "Estudávamos a mesma composição lírica juntos pela manhã, as anotações eram estritamente harmônicas.",
                Clue2Adm = "Eu odiava aquele homem! Ele roubou meus arranjos e Munhoz prometeu financiar minha ópera se eu o tirasse do caminho!",
                Clue3Title = "Taça de Cristal Manchada",
                Clue3Desc = "A taça usada pelo maestro no intervalo continha impressões parciais dos dedos do suspeito na haste.",
                Clue3Excuse = "Entreguei a taça de água a pedido dele no fosso da orquestra antes do início do primeiro ato.",
                Clue3Adm = "Sim, fui eu quem levou a taça ao camarim dele no intervalo... mas o veneno já estava misturado por Munhoz!",
                Clue4Title = "Recibo de Casa de Penhores",
                Clue4Desc = "Um recibo de penhor de joias de família para quitar débitos de jogo datado de dois dias antes do crime.",
                Clue4Excuse = "Apenas resgatei um relógio antigo que meu pai havia deixado com o penhorista.",
                Clue4Adm = "Eu estava falido e Munhoz comprou minhas dívidas na mesa de bacará em troca do veneno!"
            },
            new
            {
                TitleSeed = "O Golpe no Grande Hotel",
                Context = "O diplomata suíço foi dopado na suíte presidencial e uma maleta com microfilmagens secretas desapareceu.",
                Objective = $"Faça {suspectName} confessar quem encomendou as microfilmagens e onde escondeu os rolos de filme.",
                Role = "Concierge Noturno • Grande Hotel Central",
                Personality = "Polido, calculista e cínico, habituado aos segredos da alta sociedade, mas apavorado com a penitenciária.",
                Accomplice = "Espião Viktor Klaus",
                HiddenPlace = "no cofre de bagagens 14B da recepção",
                HiddenObj = "as microfilmagens da embaixada",
                WeakPoint = "sua noiva enfermeira Beatriz",
                WeakKeywords = new[] { "noiva", "beatriz", "noivado", "moça", "família", "casamento" },
                Clue1Title = "Chave Mestra das Suítes",
                Clue1Desc = "O cartão mestra de metal das suítes de luxo foi registrado destrancando a suíte 401 às 03h15.",
                Clue1Excuse = "Subi para levar toalhas limpas solicitadas pelo interfone da governança às três da manhã.",
                Clue1Adm = "Eu usei a chave mestra para abrir a suíte enquanto Klaus colocava o éter no lenço!",
                Clue2Title = "Rolo de Filme Revelado",
                Clue2Desc = "Fragmentos de película 16mm cortada encontrados na cesta de correspondências da portaria.",
                Clue2Excuse = "Eram apenas fotos promocionais do saguão do hotel para o catálogo da temporada de inverno.",
                Clue2Adm = "Eram as cópias dos tratados da embaixada... Klaus fotografou tudo em cinco minutos!",
                Clue3Title = "Crachá Registrado às 03h15",
                Clue3Desc = "O ponto eletrônico e a guarita registraram o crachá do suspeito no corredor presidencial.",
                Clue3Excuse = "Faço rondas periódicas em todos os andares para garantir a tranquilidade dos hóspedes.",
                Clue3Adm = "Eu fiquei vigiando o corredor enquanto Klaus revirava o cofre da suíte!",
                Clue4Title = "Envelope com Moeda Estrangeira",
                Clue4Desc = "Cinco mil dólares em notas limpas escondidos atrás do espelho do banheiro dos funcionários.",
                Clue4Excuse = "Aquele envelope foi esquecido por algum turista estrangeiro no hall e guardado para devolução.",
                Clue4Adm = "Foi a primeira parcela que Klaus me pagou para garantir o acesso livre ao elevador privativo!"
            },
            new
            {
                TitleSeed = "O Homicídio na Cocheira do Jóquei",
                Context = "O jóquei campeão foi encontrado morto a golpes de estribo na baia do puro-sangue antes do Grande Prêmio.",
                Objective = $"Faça {suspectName} confessar quem pagou pelo boicote do cavalo e onde escondeu a seringa dopante.",
                Role = "Veterinário do Hipódromo • Clube Hípico",
                Personality = "Nervoso, com dedos trêmulos e olhar evasivo, afundado em dívidas de apostas clandestinas.",
                Accomplice = "Apostador Tonho Valente",
                HiddenPlace = "dentro do cocho duplo da baia 7",
                HiddenObj = "a seringa com sedativo para cavalos",
                WeakPoint = "sua mãe viúva dona Laura",
                WeakKeywords = new[] { "mãe", "mae", "laura", "idosa", "família", "viúva" },
                Clue1Title = "Estribo Manchado de Sangue",
                Clue1Desc = "Um estribo de ferro pesado com vestígios de sangue da vítima localizado sob a palha da cocheira.",
                Clue1Excuse = "O estribo se soltou da sela durante o treino matinal e foi deixado no chão da baia pelos cavalariços.",
                Clue1Adm = "O jóquei me pegou com a seringa na mão na baia! Houve uma luta cega no escuro e o estribo atingiu a têmpora dele!",
                Clue2Title = "Cartela de Apostas de 100 Mil",
                Clue2Desc = "Um bilhete de aposta vultosa contra o cavalo favorito apreendido no armário do vestiário do suspeito.",
                Clue2Excuse = "Achei essa cartela caída na arquibancada após o encerramento do páreo da tarde anterior.",
                Clue2Adm = "Tonho Valente apostou 100 mil contra o cavalo e me prometeu metade se o animal não corresse!",
                Clue3Title = "Frasco de Sedativo Veterinário Vazio",
                Clue3Desc = "Um frasco de cloridrato de xilazina para cavalos com o lacre rompido na lixeira do consultório.",
                Clue3Excuse = "Uso esse sedativo regularmente em procedimentos cirúrgicos autorizados pela comissão de corridas.",
                Clue3Adm = "A dose daquele frasco foi parar na seringa que Valente me mandou aplicar antes da largada!",
                Clue4Title = "Luva de Borracha com Fibras",
                Clue4Desc = "Luvas cirúrgicas com fios do uniforme de corrida do jóquei presas no cano de esgoto da cocheira.",
                Clue4Excuse = "Descartei as luvas após fazer curativos de rotina nos tendões do animal na manhã de ontem.",
                Clue4Adm = "Eu tentei me livrar das luvas no cano quando vi que o jóquei não respirava mais!"
            },
            new
            {
                TitleSeed = "A Fraude do Antiquário",
                Context = "O avaliador do museu foi asfixiado na sobreloja do antiquário após descobrir que as joias da coroa eram réplicas de vidro.",
                Objective = $"Faça {suspectName} confessar onde estão as joias autênticas e quem financiou a cópia falsificada.",
                Role = "Restaurador de Relíquias • Antiquário Imperial",
                Personality = "Erudito, arrogante e meticuloso, mas incapaz de suportar a ameaça pública aos seus entes queridos.",
                Accomplice = "Barão Montenegro",
                HiddenPlace = "dentro do relógio de pêndulo carrilhão do século XVIII",
                HiddenObj = "as joias verdadeiras de safira",
                WeakPoint = "sua filha pintora Mariana",
                WeakKeywords = new[] { "filha", "mariana", "menina", "família", "ateliê", "proteção" },
                Clue1Title = "Lupa de Ourives Quebrada",
                Clue1Desc = "Fragmentos de lente graduada com impressões digitais ensanguentadas sobre o tapete persa da loja.",
                Clue1Excuse = "A lente escorregou da bancada durante o exame de rotina de moedas de prata na parte da tarde.",
                Clue1Adm = "A lupa quebrou quando o avaliador me chamou de falsário e tentou pegar o telefone para chamar a polícia!",
                Clue2Title = "Molde de Gesso com Resíduo de Resina",
                Clue2Desc = "Um molde usado para replicar o broche imperial com pó de vidro azul achado na pia da oficina.",
                Clue2Excuse = "Estava apenas restaurando uma moldura barroca decorativa para um cliente particular da alta sociedade.",
                Clue2Adm = "Eu moldei a réplica a mando de Montenegro! Ele trocou as safiras verdadeiras pela pasta de vidro!",
                Clue3Title = "Nota Promissória de 80 Mil Réis",
                Clue3Desc = "Um compromisso de pagamento assinado pelo Barão Montenegro no cofre de documentos do antiquário.",
                Clue3Excuse = "Trata-se de uma transação lícita referente à venda de tapeçarias flamengas do acervo da família dele.",
                Clue3Adm = "Os 80 mil eram o pagamento pela cópia perfeita! Montenegro já tinha comprador no exterior!",
                Clue4Title = "Retalho de Veludo Ensanguentado",
                Clue4Desc = "Um pedaço de tecido de forro de estojo com gotas de sangue oculto atrás da estante de tomos antigos.",
                Clue4Excuse = "Cortei o dedo manuseando uma lâmina de x-ato enquanto encadernava um catálogo antigo.",
                Clue4Adm = "O retalho serviu para sufocar o avaliador antes que ele conseguisse gritar por socorro na rua!"
            },
            new
            {
                TitleSeed = "A Sabotagem na Fundição São Jorge",
                Context = "A válvula principal da caldeira foi travada propositalmente, causando uma explosão que soterrou o líder grevista.",
                Objective = $"Faça {suspectName} confessar quem pagou pela sabotagem da caldeira e onde escondeu a ferramenta de bloqueio.",
                Role = "Mestre de Caldeira • Fundição São Jorge",
                Personality = "Rústico, taciturno e desconfiado da polícia, movido pelo medo das represálias da diretoria fabril.",
                Accomplice = "Capataz Silveira",
                HiddenPlace = "no tubo de descarte de escória sob a esteira 3",
                HiddenObj = "a chave de bloqueio adulterada",
                WeakPoint = "seu irmão caçula Pedro",
                WeakKeywords = new[] { "irmão", "irmao", "pedro", "família", "menino", "salvar" },
                Clue1Title = "Manômetro Rompido com Marcas de Martelo",
                Clue1Desc = "O marcador de pressão da caldeira 4 tinha ranhuras evidentes de golpe mecânico desferido por trás.",
                Clue1Excuse = "O manômetro estourou pela sobrepressão do vapor quente durante a troca de turno dos operários.",
                Clue1Adm = "Fui eu quem bateu no manômetro para que o vigia não visse a pressão subir antes da detonação!",
                Clue2Title = "Luva de Amianto Queimada",
                Clue2Desc = "Uma luva térmica com as iniciais do suspeito encontrada próxima ao registro de alimentação de gás.",
                Clue2Excuse = "Esqueci minhas luvas na bancada de trabalho ao encerrar o turno regular das 18 horas.",
                Clue2Adm = "Eu tive que usar a luva para fechar o registro de escape sem queimar minhas mãos!",
                Clue3Title = "Ficha de Ponto com Horário Falso",
                Clue3Desc = "O cartão de ponto marcava saída às 18h10, mas testemunhas relatam movimentação na caldeira às 22h.",
                Clue3Excuse = "Bati meu cartão e fui para a taverna da esquina beber com os outros metalúrgicos.",
                Clue3Adm = "Silveira bateu meu cartão no início da noite e me fez voltar pelo portão dos fundos às dez!",
                Clue4Title = "Maço de Notas Graxentas",
                Clue4Desc = "Dois mil cruzeiros manchados com óleo mineral de motor localizados dentro da botina do suspeito.",
                Clue4Excuse = "Eram minhas economias de meses de hora extra guardadas no armário para pagar a reforma de casa.",
                Clue4Adm = "Silveira me deu essa grana viva na hora... prometeu que ninguém ia morrer na explosão!"
            },
            new
            {
                TitleSeed = "A Chantagem na Farmácia Central",
                Context = "O juiz distrital faleceu após ingerir cápsulas manipuladas com dosagem letal de estricnina.",
                Objective = $"Faça {suspectName} confessar quem encomendou a adulteração do remédio e onde escondeu o caderno de dosagens.",
                Role = "Manipulador Farmacêutico • Botica Central",
                Personality = "Tímido, metódico e aterrorizado pela perspectiva da forca, vulnerável à compaixão pela sua família.",
                Accomplice = "Advogado Bastos",
                HiddenPlace = "no fundo falso do armário de controlados",
                HiddenObj = "o caderno de receitas adulteradas",
                WeakPoint = "sua filha pequena Clara",
                WeakKeywords = new[] { "filha", "clara", "criança", "crianca", "família", "berço" },
                Clue1Title = "Pilão de Porcelana com Resíduo Tóxico",
                Clue1Desc = "Um pilão de trituração química com pó de estricnina achado debaixo da pia do laboratório de manipulação.",
                Clue1Excuse = "Uso o pilão para moer compostos fitoterápicos analgésicos de venda comum no balcão.",
                Clue1Adm = "Eu triturei a estricnina naquele pilão... Bastos me garantiu que era apenas um sedativo para afastar o juiz do julgamento!",
                Clue2Title = "Receituário Rasgado com a Caligrafia do Juiz",
                Clue2Desc = "Restos de receita médica oficial recuperados do incinerador de papel do pátio interno.",
                Clue2Excuse = "Descartei formulários vencidos e receitas arquivadas da semana passada conforme a vigilância sanitária exige.",
                Clue2Adm = "Eu tentei queimar a receita original porque a dosagem que coloquei na cápsula era cinco vezes maior!",
                Clue3Title = "Frasco de Cianeto sem Lacre",
                Clue3Desc = "Um vidro de estricnina do estoque controlado com o selo fiscal violado e 10 gramas a menos no peso.",
                Clue3Excuse = "Houve evaporação química ou erro de medição do laboratório fornecedor da matéria-prima.",
                Clue3Adm = "As 10 gramas foram para as cápsulas que Bastos mandou entregar na residência do magistrado!",
                Clue4Title = "Livro de Fórmulas com Rasuras",
                Clue4Desc = "No livro oficial de controle da farmácia, a página da receita do juiz teve a dosagem apagada com ácido.",
                Clue4Excuse = "Derramei reagente sobre o livro por acidente enquanto limpava a bancada de vidro.",
                Clue4Adm = "Eu rasurei a quantidade no livro para que os fiscais da saúde não descobrissem a dose letal!"
            }
        };

        var innocentArchetypes = new[]
        {
            new
            {
                TitleSeed = "A Armação no Cartório Central",
                Context = "O cofre blindado de escrituras do Cartório foi saqueado e documentos de herança sumiram. Documentos comprometedores foram plantados na bolsa do suspeito.",
                Objective = $"Comprove a inocência de {suspectName}, desmascare a farsa forjada pelo Tabelião Prado e encontre os papéis originais.",
                Role = "Escrevente Notarial • Cartório Central",
                Personality = "Honesto, metódico e chocado com a calúnia pública contra sua reputação.",
                Accomplice = "Tabelião Prado",
                HiddenPlace = "na caixa postal 42 da agência central dos correios",
                HiddenObj = "as escrituras de herança autênticas",
                WeakPoint = "sua mãe idosa dona Cecília",
                WeakKeywords = new[] { "mãe", "mae", "cecília", "cecilia", "idosa", "família", "proteção" },
                Clue1Title = "Chave Cópia do Cofre Notarial",
                Clue1Desc = $"A chave da sala forte foi encontrada dentro da pasta de couro de {suspectName}.",
                Clue1Excuse = "Aquela chave foi plantada! Deixei minha pasta na recepção enquanto fui almoçar!",
                Clue1Adm = "O próprio Tabelião Prado pediu minha pasta ontem dizendo que ia guardar selos e plantou a chave!",
                Clue2Title = "Envelope com Carimbo Pessoal",
                Clue2Desc = "Um envelope timbrado usado no furto levava o carimbo numérico do suspeito.",
                Clue2Excuse = "Meu carimbo fica sobre a mesa ao alcance de qualquer visitante ou estagiário do cartório.",
                Clue2Adm = "Prado usou meu carimbo no escuro após as 18h para simular que eu tinha despachado a pasta!",
                Clue3Title = "Canhoto de Ponto Batido às 21h",
                Clue3Desc = "O cartão de ponto marcava presença do suspeito no prédio na hora exata do arrombamento.",
                Clue3Excuse = "Eu estava no culto da igreja às 21h diante de dezenas de testemunhas! Alguém bateu meu cartão!",
                Clue3Adm = "Foi o comparsa de Prado que bateu meu cartão na catraca para forjar meu álibi falso e me incriminar!",
                Clue4Title = "Chancela de Cera Falsificada",
                Clue4Desc = "Um selo de cera com imperfeições usado para lacrar gavetas que não correspondia ao molde oficial do suspeito.",
                Clue4Excuse = "Essa chancela nunca foi minha! Vejam os traços irregulares gravados nela!",
                Clue4Adm = "Prado mandou forjar essa chancela em um ourives da baixada para tentar simular minha assinatura nos lacres!"
            },
            new
            {
                TitleSeed = "O Complô no Laboratório Metalúrgico",
                Context = "A fórmula de uma blindagem experimental foi roubada e o cofre de patentes foi arrombado. Vestígios químicos foram deixados no armário do suspeito.",
                Objective = $"Comprove a inocência de {suspectName}, desarme a conspiração do Diretor Fontana e descubra onde a fórmula está escondida.",
                Role = "Assistente Químico • Fundição Industrial",
                Personality = "Trabalhador pacato, assustado com os métodos agressivos dos investigadores.",
                Accomplice = "Diretor Fontana",
                HiddenPlace = "dentro do cilindro de nitrogênio inativo da caldeira",
                HiddenObj = "o tubo de ensaio com a fórmula original",
                WeakPoint = "sua filha recém-nascida Clara",
                WeakKeywords = new[] { "filha", "clara", "bebê", "bebe", "família", "salvar", "berço" },
                Clue1Title = "Jaleco Salpicado de Reagente",
                Clue1Desc = "O jaleco de trabalho do suspeito continha gotas da solução fosforescente do cofre.",
                Clue1Excuse = "Meu armário foi arrombado durante a noite e meu jaleco foi trocado pelo de outro turno!",
                Clue1Adm = "Fontana pegou meu jaleco emprestado fingindo que o dele tinha rasgado na esteira!",
                Clue2Title = "Credencial Magnética Registrada",
                Clue2Desc = "O sistema eletrônico registrou o crachá do suspeito destravando a porta às 01h15.",
                Clue2Excuse = "Perdi meu crachá no refeitório ao meio-dia e avisei a chefia imediatamente!",
                Clue2Adm = "Fontana achou meu crachá e usou às pressas na madrugada para que as câmeras culpassem a mim!",
                Clue3Title = "Frasco de Ácido Corrosivo",
                Clue3Desc = "Um frasco de ácido usado para derreter a fechadura foi achado embaixo da bancada do suspeito.",
                Clue3Excuse = "Aquele ácido é de uso geral da manutenção pesada da fábrica, qualquer técnico tem acesso.",
                Clue3Adm = "O capanga de Fontana colocou aquele frasco ali enquanto eu estava prestando os primeiros socorros ao vigia!",
                Clue4Title = "Registro de Pressão Adulterado",
                Clue4Desc = "Uma folha de registro do painel da caldeira com a caligrafia do suspeito simulada de forma grosseira.",
                Clue4Excuse = "Não fui eu quem preencheu esses dados de pressão, a caneta usada nem é a padrão do meu setor!",
                Clue4Adm = "Fontana tentou falsificar minhas anotações na caldeira para fingir que eu sabotei a pressão propositalmente!"
            },
            new
            {
                TitleSeed = "A Armadilha na Casa de Câmbio",
                Context = "Cinquenta mil dólares em espécie sumiram do cofre blindado da agência de câmbio durante a noite. Chaves duplicadas e notas marcadas foram colocadas no armário do suspeito.",
                Objective = $"Comprove a inocência de {suspectName}, desmonte a farsa armada pelo Gerente Peixoto e encontre o dinheiro verdadeiro.",
                Role = "Caixa Bancário • Câmbio Atlântica",
                Personality = "Metódico, pontual e apavorado com a prisão injusta, pai de família dedicado.",
                Accomplice = "Gerente Peixoto",
                HiddenPlace = "no forro de gesso da sala da gerência",
                HiddenObj = "o malote com as cédulas verdadeiras de dólar",
                WeakPoint = "seu irmão acamado Gabriel",
                WeakKeywords = new[] { "irmão", "irmao", "gabriel", "acamado", "doente", "família", "hospital" },
                Clue1Title = "Falso Boleto de Depósito com Assinatura Falsificada",
                Clue1Desc = "Um comprovante de entrada com a rubrica do suspeito simulada de maneira grosseira.",
                Clue1Excuse = "Aquela assinatura não é minha! O traço da letra é trêmulo e a tinta nem é a da minha caneta!",
                Clue1Adm = "O próprio Gerente Peixoto recolheu meus carimbos ontem dizendo que ia conferir a auditoria!",
                Clue2Title = "Gaveta do Caixa Destrancada com Chave Michê",
                Clue2Desc = "A fechadura do guichê 2 tinha marcas de lima e estrias de arrombamento forçado.",
                Clue2Excuse = "Eu entreguei a chave mestra no cofre ao sair às 17h, como faço rigorosamente todos os dias.",
                Clue2Adm = "Peixoto ficou até depois das oito da noite sozinho na agência fingindo fechar o balanço!",
                Clue3Title = "Bilhete de Fuga Plantado no Vestiário",
                Clue3Desc = "Um bilhete de passagem de ônibus para a fronteira achado dentro da bolsa do suspeito.",
                Clue3Excuse = "Nunca comprei passagem para lugar nenhum, tenho um irmão doente que precisa de mim aqui!",
                Clue3Adm = "O comparsa de Peixoto colocou essa passagem na minha bolsa enquanto fui ao banheiro!",
                Clue4Title = "Fita de Máquina Somadora Adulterada",
                Clue4Desc = "A fita de papel da contabilidade com o total do caixa adulterado para simular o desfalque.",
                Clue4Excuse = "O total bateu certinho às cinco da tarde diante do fiscal de balcão!",
                Clue4Adm = "Peixoto passou a fita novamente à noite alterando os dígitos da coluna de crédito!"
            },
            new
            {
                TitleSeed = "A Calúnia no Depósito Portuário",
                Context = "Um carregamento de armamentos pesados da alfândega foi violado no porto e o selo de vistoria do suspeito foi encontrado rompido.",
                Objective = $"Comprove a inocência de {suspectName}, desmascare a corrupção do Despachante Nogueira e recupere o manifesto autêntico.",
                Role = "Conferente de Carga • Doca 14",
                Personality = "Trabalhador honrado, calejado pelo cais, mas teme perder o sustento de sua família.",
                Accomplice = "Despachante Nogueira",
                HiddenPlace = "dentro do contêiner marítimo falso nº 88",
                HiddenObj = "o manifesto de carga original sem adulteração",
                WeakPoint = "sua filha adolescente Elisa",
                WeakKeywords = new[] { "filha", "elisa", "menina", "escola", "família", "proteção" },
                Clue1Title = "Lacre de Contêiner Violado com Alicate do Suspeito",
                Clue1Desc = "Um lacre de aço quebrado com ranhuras coincidentes com o alicate da guarita do suspeito.",
                Clue1Excuse = "Aquele alicate fica preso num gancho na guarita, dezenas de estivadores usam para abrir caixotes.",
                Clue1Adm = "Nogueira pegou o alicate emprestado às 23h dizendo que precisava abrir uma caixa de café!",
                Clue2Title = "Manifesto de Importação com Carimbo Plantado",
                Clue2Desc = "A folha de liberação aduaneira timbrada com a chancela de conferência de carga.",
                Clue2Excuse = "Aquele carimbo foi furtado da minha prancheta durante o tumulto da atracação do navio.",
                Clue2Adm = "O carimbo foi batido na folha fria pelos capangas de Nogueira para culpar a conferência!",
                Clue3Title = "Chave do Galpão 5 Achada na Gaveta do Suspeito",
                Clue3Desc = "A chave de ferro do depósito secreto onde o arsenal foi descarregado.",
                Clue3Excuse = "Alguém abriu a gaveta da minha mesa no vestiário geral durante o intervalo da janta.",
                Clue3Adm = "Foi Nogueira quem plantou a chave ali enquanto os conferentes estavam no refeitório!",
                Clue4Title = "Prancheta de Vistoria com Letra Falsificada",
                Clue4Desc = "A anotação de 'carga inspecionada' grafada imitando a letra do suspeito.",
                Clue4Excuse = "Eu assino com caneta tinteiro azul escura, aquilo ali foi feito com lápis cópia comum!",
                Clue4Adm = "Nogueira tentou simular minhas anotações para fingir que eu dei o visto nas armas!"
            },
            new
            {
                TitleSeed = "O Flagrante Forjado na Tipografia",
                Context = "Panfletos clandestinos atacando o governo foram encontrados na prensa e placas de linotipo com calúnia foram atribuídas ao suspeito.",
                Objective = $"Comprove a inocência de {suspectName}, revele a armadilha armada pelo Proprietário Valadares e ache as matrizes originais.",
                Role = "Impressor Gráfico • Gráfica Progresso",
                Personality = "Pacífico, artesão apaixonado pelas letras, desolado com a acusação política caluniosa.",
                Accomplice = "Proprietário Valadares",
                HiddenPlace = "embaixo do assoalho da oficina de encadernação",
                HiddenObj = "as matrizes de impressão originais do panfleto",
                WeakPoint = "sua mãe costureira dona Helena",
                WeakKeywords = new[] { "mãe", "mae", "helena", "idosa", "família", "costureira" },
                Clue1Title = "Folha de Prova com Tinta nas Mãos do Suspeito",
                Clue1Desc = "Resíduos de tinta gráfica preta nas pontas dos dedos e uma folha teste do manifesto no chão.",
                Clue1Excuse = "Trabalhei doze horas imprimindo livros escolares, minhas mãos ficam sujas de tinta preta todo dia!",
                Clue1Adm = "Valadares me mandou limpar a prensa de madrugada para que meus dedos ficassem manchados daquela tinta!",
                Clue2Title = "Tipos Móveis de Chumbo Encontrados na Mochila",
                Clue2Desc = "Letras de chumbo que formavam o título subversivo achadas dentro da mochila do suspeito.",
                Clue2Excuse = "Minha mochila ficou pendurada atrás da porta do vestiário aberta o dia inteiro.",
                Clue2Adm = "O próprio Valadares colocou os tipos de chumbo ali antes de ligar para a polícia política!",
                Clue3Title = "Ordem de Serviço Datilografada no Nome do Suspeito",
                Clue3Desc = "Um pedido de tiragem irregular assinado em nome do impressor.",
                Clue3Excuse = "Eu não tenho autoridade administrativa para abrir ordem de serviço, só o dono pode autorizar!",
                Clue3Adm = "Valadares usou a máquina de escrever do balcão para forjar o pedido em meu nome!",
                Clue4Title = "Chave da Prensa de Tipografia Duplicada",
                Clue4Desc = "A chave de partida do motor elétrico da máquina rotativa encontrada no bolso do casaco.",
                Clue4Excuse = "O casaco era da empresa e fica no armário compartilhado da manutenção pesada.",
                Clue4Adm = "Valadares me deu aquele casaco de presente ontem à tarde... já estava com a chave no bolso!"
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
                SecretTruth = $"{suspectName} é completamente inocente e foi vítima de uma armadilha calculada por {arch.Accomplice}. A prova definitiva ({arch.HiddenObj}) está {arch.HiddenPlace}.",
                OpeningGmNarration = $"A luz solitária ilumina o rosto pálido de {suspectName}. Seus olhos expressam o terror genuíno de um cidadão honesto arrastado para um pesadelo.",
                OpeningNpcDialogue = $"\"Detetive, pelo amor de Deus, eu sou inocente! Eu nunca roubei nada na minha vida! Estão armando contra mim para proteger peixe grande desta cidade!\"",
                AlibiSpeech = "Eu estava longe dali cumprindo minhas obrigações e tenho testemunhas sérias que podem comprovar minha conduta ilibada!",
                EmpathyEarlySpeech = "Obrigado por me dar a chance de falar sem me condenar antes da hora, detetive... você é a primeira pessoa nesta delegacia que parece querer a verdade.",
                EarlyWhoWhereSpeech = $"Quem armou essa cilada é influente demais... o {arch.Accomplice} sabia cada passo da minha rotina e tinha as chaves necessárias!",
                WeakPointLabel = arch.WeakPoint,
                WeakPointKeywords = arch.WeakKeywords,
                WeakPointReactionSpeech = $"Deixem {arch.WeakPoint} fora desse pesadelo! Eu conto o que sei: foi {arch.Accomplice} quem armou tudo para salvar a própria pele!",
                ConfessionSpeech = $"A farsa foi montada por {arch.Accomplice}! Ele roubou tudo e me incriminou deliberadamente! {arch.HiddenObj} está escondido {arch.HiddenPlace}!",
                ExonerationSpeech = $"A verdade finalmente prevaleceu! Deus abençoe seu senso de justiça, detetive! {arch.Accomplice} vai responder perante a lei e {arch.HiddenObj} está {arch.HiddenPlace}!",
                KeyAccompliceWord = arch.Accomplice,
                KeyConfessionWord = arch.HiddenPlace.Split(' ').Last(),
                HiddenObjectLabel = arch.HiddenObj,
                BonusClueTitle = $"Armação de {arch.Accomplice}",
                BonusClueDescription = $"{suspectName} demonstrou que {arch.Accomplice} forjou as evidências e o usou como bode expiatório.",
                BonusClueKeywords = new[] { arch.Accomplice.ToLowerInvariant(), "armação", "armacao", "farsa", "bode" },
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
                SecretTruth = $"{arch.Accomplice} forçou {suspectName} a participar do crime. O objeto ({arch.HiddenObj}) está escondido {arch.HiddenPlace}.",
                OpeningGmNarration = $"A lâmpada amarelada oscila sobre a mesa metálica. {suspectName} puxa o ar com esforço, os olhos inquietos varrendo as sombras da sala de interrogatório.",
                OpeningNpcDialogue = $"\"Eu já prestei meu depoimento aos guardas, detetive. Cumpri meu dever profissional e não tenho nada a ver com essa desgraça. Se querem um culpado, procurem nas ruas da máfia.\"",
                AlibiSpeech = "Eu estava no meu posto de trabalho cumprindo as tarefas normais até o encerramento do expediente, sem qualquer desvio de conduta.",
                EmpathyEarlySpeech = "Ninguém quer ver desgraça acontecer, detetive... mas há forças nesta cidade com as quais um trabalhador comum não pode bater de frente.",
                EarlyWhoWhereSpeech = $"Se eu pronunciar o nome de quem realmente mandou fazer isso sem uma escolta na minha porta, minha vida não vale dez tostões amanhã.",
                WeakPointLabel = arch.WeakPoint,
                WeakPointKeywords = arch.WeakKeywords,
                WeakPointReactionSpeech = $"Deixem {arch.WeakPoint} em paz! Eu conto: foi o {arch.Accomplice} quem armou tudo e me colocou entre a cruz e a espada!",
                ConfessionSpeech = $"Eu confesso tudo! O {arch.Accomplice} foi o mandante e me obrigou a participar. {arch.HiddenObj} está escondido {arch.HiddenPlace}!",
                KeyAccompliceWord = arch.Accomplice,
                KeyConfessionWord = arch.HiddenPlace.Split(' ').Last(),
                HiddenObjectLabel = arch.HiddenObj,
                BonusClueTitle = $"Mandado de {arch.Accomplice}",
                BonusClueDescription = $"{suspectName} admitiu que {arch.Accomplice} ordenou o crime e coagiu sua cooperação.",
                BonusClueKeywords = new[] { arch.Accomplice.ToLowerInvariant(), "ordem", "mandante", "coação" },
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