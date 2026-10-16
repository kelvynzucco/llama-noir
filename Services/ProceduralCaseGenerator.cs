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
    /// Pool de Casos Noir Artesanais e Detalhados
    /// </summary>
    public static readonly NoirCaseBlueprint[] BaseBlueprints =
    {
        // 1. Emboscada no Beco Blue Note
        new()
        {
            Title = "Emboscada no Beco Blue Note",
            Context = "O informante 'Rato' Soares foi morto com dois tiros às 02h00 no beco atrás do Clube de Jazz Blue Note.",
            Objective = "Faça Vicente Moretti confessar quem atirou em Soares e onde escondeu o livro-caixa do sindicato.",
            NpcName = "Vicente Moretti",
            NpcRole = "Gerente Noturno • Clube de Jazz Blue Note",
            NpcPersonality = "Irônico e vivido na noite, tenta manter a frieza, mas entra em pânico quando a segurança de sua irmã Lúcia é ameaçada.",
            SecretTruth = "O mafioso Donato Braga ameaçou matar Lúcia (irmã de Vicente) para obrigá-lo a destrancar a porta dos fundos às 02h00. Donato Braga matou Rato Soares, e Vicente escondeu o livro-caixa dentro da caixa acústica do piano de cauda do clube.",
            OpeningGmNarration = "A fumaça do cigarro sobe lenta sob a lâmpada fria da delegacia. Vicente Moretti ajeita o colarinho do terno risca-de-giz e encara você com um meio sorriso cansado.",
            OpeningNpcDialogue = "\"Já falei tudo pros seus guardas na entrada, detetive. Fechei o Blue Note à meia-noite em ponto, passei o cadeado e fui pra minha cama. O que acontece naquele beco escuro depois que as luzes apagam não é conta minha.\"",
            AlibiSpeech = "Eu sou apenas o gerente do Clube Blue Note há dez anos, detetive. Fechei o bar à meia-noite em ponto e fui direto para casa dormir.",
            EmpathyEarlySpeech = "Agradeço o tom civilizado, detetive... a verdade é que o Rato Soares andava chantageando gente perigosa demais naquele clube, gente com quem ninguém quer cruzar.",
            EarlyWhoWhereSpeech = "Você acha que se eu soubesse quem apagou o Rato Soares eu estaria vivo sentado aqui? Se eu abrir a boca sem garantias, não chego vivo até amanhã.",
            WeakPointLabel = "sua irmã Lúcia",
            WeakPointKeywords = new[] { "irmã", "irma", "lúcia", "lucia", "família", "familia", "proteger", "proteção", "protecao", "viatura" },
            WeakPointReactionSpeech = "Deixe a minha irmã Lúcia fora disso, pelo amor de Deus! Se você colocar uma viatura na porta dela agora, eu falo: foi o chefão Donato Braga quem me obrigou a abrir a porta dos fundos às duas da manhã!",
            ConfessionSpeech = "Tudo bem, eu confesso tudo! Donato Braga jurou matar minha irmã Lúcia se eu não destrancasse os fundos às duas da manhã. Foi o próprio Donato Braga quem deu os dois tiros no Rato Soares, e eu escondi o livro-caixa dentro do piano de cauda do clube!",
            KeyAccompliceWord = "Donato Braga",
            KeyConfessionWord = "piano",
            HiddenObjectLabel = "o livro-caixa",
            BonusClueTitle = "Ameaça de Donato Braga",
            BonusClueDescription = "Moretti admitiu que o chefão Donato Braga coagiu sua irmã Lúcia para ter acesso ao beco às 02h00.",
            BonusClueKeywords = new[] { "donato", "braga", "ameaça", "ameaca", "piano" },
            Clues = new[]
            {
                new ClueBlueprint
                {
                    Title = "Isqueiro de Prata no Beco",
                    Description = "O isqueiro de prata gravado de Moretti foi encontrado no chão ao lado do corpo de Soares.",
                    Keywords = new[] { "isqueiro", "prata", "chão", "chao", "corpo" },
                    ExcuseSpeech = "Esse isqueiro de prata era meu sim, mas sumiu do balcão do bar ontem à tarde! Qualquer freguês pode ter furtado de mim e deixado cair no beco.",
                    AdmissionSpeech = "Está bem, o isqueiro caiu do meu bolso às duas da manhã! Eu estava lá no beco quando o Rato Soares morreu, mas não fui eu quem puxou o gatilho!"
                },
                new ClueBlueprint
                {
                    Title = "Carro Visto às 02h10",
                    Description = "O saxofonista da banda viu o sedã preto de Moretti estacionado no beco às 02h10 da madrugada.",
                    Keywords = new[] { "carro", "sedã", "seda", "saxofonista", "02h10", "duas e dez", "estacionado" },
                    ExcuseSpeech = "A bateria do meu sedã arriou antes da meia-noite, detetive. Deixei o carro trancado lá no beco e voltei para casa de táxi.",
                    AdmissionSpeech = "O saxofonista falou a verdade... meu carro estava lá às duas e dez porque me forçaram a ficar no clube esperando o serviço sujo terminar!"
                },
                new ClueBlueprint
                {
                    Title = "Envelope com 20 Mil Dólares",
                    Description = "A perícia achou 20 mil dólares em notas novas dentro da gaveta trancada do escritório de Moretti.",
                    Keywords = new[] { "envelope", "20 mil", "vinte mil", "dólares", "dolares", "dinheiro", "gaveta", "escritório" },
                    ExcuseSpeech = "Esses vinte mil dólares no envelope são apenas o faturamento das bebidas da semana no Blue Note, ainda não tive tempo de depositar no banco.",
                    AdmissionSpeech = "Aquele envelope de vinte mil dólares foi deixado na minha mesa pelo capanga de Donato Braga para comprar o meu silêncio sobre o livro-caixa!"
                },
                new ClueBlueprint
                {
                    Title = "Sapato com Barro do Beco",
                    Description = "O calçado italiano de Moretti continha terra fresca e fuligem idênticas às marcas achadas junto ao cadáver de Soares.",
                    Keywords = new[] { "sapato", "calçado", "calcado", "barro", "lama", "beco", "pegadas", "fuligem" },
                    ExcuseSpeech = "Pisei naquela poça de barro quando saí para conferir as lixeiras antes de fechar o bar à meia-noite.",
                    AdmissionSpeech = "Eu voltei ao beco às duas da manhã sim! As marcas de barro são minhas... Donato me forçou a conferir se o serviço estava feito!"
                }
            }
        },

        // 2. Queima de Arquivo no Cais 14
        new()
        {
            Title = "Queima de Arquivo no Cais 14",
            Context = "O despachante aduaneiro Artur Prado foi achado morto no Cais 14 e os manifestos de contrabando sumiram.",
            Objective = "Faça Dr. Heitor Vasconcelos confessar quem ordenou a morte no cais e onde escondeu os manifestos originais.",
            NpcName = "Dr. Heitor Vasconcelos",
            NpcRole = "Advogado do Sindicato Portuário • Cais 14",
            NpcPersonality = "Intelectual, formal e arrogante, mas desmorona quando confrontado sobre o tratamento médico de sua filha Clara.",
            SecretTruth = "O armador corrupto Marcus Vane pagou 50 mil dólares pelo sanatório de Clara (filha de Heitor) para que Heitor atraísse Artur Prado ao Cais 14 às 02h15. Os capangas de Vane mataram Prado, e Heitor escondeu os manifestos originais no cofre da Capela São Jorge.",
            OpeningGmNarration = "Dr. Heitor Vasconcelos limpa os óculos com um lenço de seda na sala fria da delegacia, mantendo as mãos cruzadas sobre a mesa metálica com calculada superioridade jurídica.",
            OpeningNpcDialogue = "\"Olhe para o relógio, detetive. Eu saí do escritório portuário às nove da noite. Se o infeliz do Artur Prado apareceu boiando no Cais 14 de madrugada, sugiro procurar entre os estivadores e não na minha banca de advocacia.\"",
            AlibiSpeech = "Eu sou o consultor jurídico do Sindicato Portuário, detetive. Encerrei meu expediente às 21h e passei a madrugada inteira em casa revisando contratos.",
            EmpathyEarlySpeech = "Poucos policiais nesta cidade conversam sem gritar, detetive. Artur Prado descobriu desvios pesados demais no porto... eu tentei avisá-lo para não mexer naquele vespeiro.",
            EarlyWhoWhereSpeech = "Os homens que controlam o Cais 14 compram juízes e delegados no atacado. Se eu disser um único nome aqui sem garantia de vida, minha família paga o preço.",
            WeakPointLabel = "sua filha Clara no sanatório",
            WeakPointKeywords = new[] { "filha", "clara", "sanatório", "sanatorio", "hospital", "clínica", "clinica", "tratamento", "médico", "medico" },
            WeakPointReactionSpeech = "A minha filha Clara depende daquele tratamento pulmonar para viver! Eu não tinha cinquenta mil dólares... foi o armador Marcus Vane quem pagou o sanatório em troca da chave do Cais 14!",
            ConfessionSpeech = "Eu confesso, detetive! Marcus Vane pagou a cirurgia da minha filha Clara e me obrigou a chamar Artur Prado ao Cais 14 às duas e quinze. Foram os homens de Marcus Vane que mataram o Prado, e eu escondi os manifestos originais no cofre da Capela São Jorge!",
            KeyAccompliceWord = "Marcus Vane",
            KeyConfessionWord = "Capela São Jorge",
            HiddenObjectLabel = "os manifestos originais",
            BonusClueTitle = "Suborno de Marcus Vane",
            BonusClueDescription = "Heitor admitiu que o armador Marcus Vane quitou a dívida médica de Clara para atrair Artur Prado ao cais.",
            BonusClueKeywords = new[] { "marcus", "vane", "armador", "capela", "são jorge" },
            Clues = new[]
            {
                new ClueBlueprint
                {
                    Title = "Placa Anotada às 02h15",
                    Description = "O vigia noturno anotou a placa do automóvel do Dr. Heitor saindo do Cais 14 às 02h15 da madrugada.",
                    Keywords = new[] { "placa", "vigia", "02h15", "duas e quinze", "automóvel", "automovel", "carro", "saindo" },
                    ExcuseSpeech = "Aquele vigia do porto vive bêbado de gim, detetive! Meu automóvel ficou na garagem a noite toda, ele deve ter confundido a placa na neblina.",
                    AdmissionSpeech = "Maldito vigia... sim, era o meu carro saindo do Cais 14 às duas e quinze! Eu estava lá quando emboscaram o Artur Prado, mas fui coagido!"
                },
                new ClueBlueprint
                {
                    Title = "Cinzas de Cópias Falsas",
                    Description = "Na lareira do escritório de Heitor foram achadas folhas queimadas, mas a perícia provou serem apenas cópias sem valor.",
                    Keywords = new[] { "cinzas", "lareira", "cópias", "copias", "queimadas", "fogo", "papel" },
                    ExcuseSpeech = "Queimei apenas rascunhos velhos de processos arquivados na lareira do meu escritório, é uma prática perfeitamente legal.",
                    AdmissionSpeech = "Eu queimei cópias falsas na lareira para enganar os capangas! Guardei os manifestos originais em outro lugar como seguro de vida!"
                },
                new ClueBlueprint
                {
                    Title = "Recibo de 50 Mil no Sanatório",
                    Description = "O Sanatório Santa Helena recebeu 50 mil dólares em espécie ontem para quitar a internação de Clara Vasconcelos.",
                    Keywords = new[] { "recibo", "50 mil", "cinquenta mil", "sanatório", "sanatorio", "internação", "internacao", "espécie" },
                    ExcuseSpeech = "Quitei as despesas médicas da minha filha Clara com economias pessoais que eu guardava em casa há anos, não devo satisfação ao fisco aqui.",
                    AdmissionSpeech = "Eu jamais teria cinquenta mil dólares em espécie! Foi o próprio Marcus Vane quem mandou entregar essa mala de dinheiro no sanatório da minha filha!"
                },
                new ClueBlueprint
                {
                    Title = "Cópia da Chave do Portão Sul",
                    Description = "Uma cópia recém-feita da chave do portão de serviço do Cais 14 foi encontrada no chaveiro particular de Heitor.",
                    Keywords = new[] { "chave", "portão", "portao", "sul", "serviço", "servico", "cais", "cadeado" },
                    ExcuseSpeech = "Como consultor jurídico do sindicato, tenho chave de todas as docas para vistorias legais.",
                    AdmissionSpeech = "Fui eu quem destrancou o portão sul para os capangas de Vane entrarem no cais sem serem vistos pelas câmeras da guarita!"
                }
            }
        },

        // 3. Veneno na Adega Saint-Germain
        new()
        {
            Title = "Veneno na Adega Saint-Germain",
            Context = "O crítico enólogo Pierre Dupont foi envenenado com cianeto após degustar uma garrafa rara na adega subterrânea.",
            Objective = "Faça a sommelier Helena Santos confessar quem encomendou o veneno e onde está a garrafa com o cianeto residual.",
            NpcName = "Helena Santos",
            NpcRole = "Sommelier-Chefe • Adega Saint-Germain",
            NpcPersonality = "Refinada, fria e atenta a detalhes, tenta se esquivar usando conhecimento técnico, mas teme a ruína da vinícola de sua família.",
            SecretTruth = "O conde rival chantageou Helena com notas promissórias falsas para adulterar o vinho de Dupont. Ela escondeu o frasco de cianeto e o saca-rolhas envenenado sob o tonel número 4 da adega subterrânea.",
            OpeningGmNarration = "Helena Santos recosta-se na cadeira com postura impecável, os braços cruzados sobre o casaco de lã. O cheiro sutil de vinho envelhecido e amêndoas amargas ainda a acompanha.",
            OpeningNpcDialogue = "\"Pierre Dupont tinha dezenas de desafetos entre os produtores que ele arruinou com suas críticas venenosas, detetive. Minha adega serve duzentos clientes por noite. Não sou responsável pelo que colocaram na taça dele.\"",
            AlibiSpeech = "Eu estive supervisionando o serviço do salão principal durante todo o jantar. Apenas os garçons de praça desceram à adega durante a degustação.",
            EmpathyEarlySpeech = "Dupont era um homem impiedoso que destruía famílias inteiras com uma canetada... muitos aqui respiraram aliviados quando ele caiu.",
            EarlyWhoWhereSpeech = "Quem encomendou a morte de Dupont é alguém com títulos e dinheiro suficiente para comprar qualquer adega deste país. Se eu disser o nome, minha família é apagada.",
            WeakPointLabel = "sua dívida de morte no Cassino Riviera",
            WeakPointKeywords = new[] { "cassino", "riviera", "dívida", "divida", "promissória", "promissoria", "agiotas", "vinícola", "vinicola" },
            WeakPointReactionSpeech = "Eles iam queimar a vinícola que meu avô construiu com as próprias mãos! Eu devia trezentos mil no Riviera... o Conde prometeu rasgar as promissórias se Dupont não saísse vivo daquela mesa!",
            ConfessionSpeech = "Fui eu quem pegou a chave da adega privativa, detetive! O Conde me forçou a pingar o veneno na taça de Bordeaux do Dupont. O frasco de cianeto e o conta-gotas ainda estão escondidos sob o tonel número 4 da adega!",
            KeyAccompliceWord = "Conde Saint-Germain",
            KeyConfessionWord = "tonel número 4",
            HiddenObjectLabel = "o frasco de cianeto",
            BonusClueTitle = "Notas do Cassino Riviera",
            BonusClueDescription = "Helena admitiu que dívidas de jogo com o Conde Saint-Germain a forçaram a envenenar a taça de Dupont.",
            BonusClueKeywords = new[] { "conde", "riviera", "tonel", "cianeto", "bordeaux" },
            Clues = new[]
            {
                new ClueBlueprint
                {
                    Title = "Frasco de Amêndoas Amargas",
                    Description = "No cesto de panos da sommelier foi encontrado um frasco vazio com odor característico de cianeto de potássio.",
                    Keywords = new[] { "frasco", "amêndoas", "amendoas", "cianeto", "veneno", "pano" },
                    ExcuseSpeech = "Aquele frasquinho no cesto era apenas essência culinária concentrada que o chef de confeitaria me pediu para descartar.",
                    AdmissionSpeech = "Sim... era o cianeto! Eu tentei lavar o frasco com vinagre e joguei nos panos velhos para que ninguém encontrasse!"
                },
                new ClueBlueprint
                {
                    Title = "Chave Privativa da Adega",
                    Description = "A chave mestra com acesso aos tonéis de guarda numerados estava no bolso do avental de Helena.",
                    Keywords = new[] { "chave", "mestra", "adega", "tonel", "avental", "privativa" },
                    ExcuseSpeech = "Como sommelier-chefe, carrego as chaves da adega no bolso o tempo todo. Isso é parte da minha rotina diária e não uma prova de crime.",
                    AdmissionSpeech = "Fui eu quem destrancou a adega às dez e meia... o homem do Conde me entregou a substância lá dentro na penumbra!"
                },
                new ClueBlueprint
                {
                    Title = "Taça de Bordeaux Adulterada",
                    Description = "A análise química da taça reservada revelou concentração letal de veneno colocada antes da abertura do rótulo.",
                    Keywords = new[] { "taça", "taca", "bordeaux", "veneno", "química", "adulterada", "vinho" },
                    ExcuseSpeech = "Dupont exigiu abrir a própria garrafa na mesa, qualquer pessoa que passou perto do aparador dele poderia ter tocado naquela taça.",
                    AdmissionSpeech = "Fui eu quem passou o veneno na borda interna da taça antes de levá-la ao Dupont... eu não tive escolha!"
                },
                new ClueBlueprint
                {
                    Title = "Conta-gotas com Traços Químicos",
                    Description = "Um conta-gotas de vidro com resíduos de ácido cianídrico foi localizado escondido atrás da prateleira de licores.",
                    Keywords = new[] { "conta-gotas", "vidro", "ácido", "cianídrico", "resíduos", "licores", "químico" },
                    ExcuseSpeech = "Usamos conta-gotas para dosar essências de carvalho em vinhos de teste, qualquer enólogo faz isso.",
                    AdmissionSpeech = "O conta-gotas era meu sim! Usei para pingar exatamente quatro gotas no fundo da taça sem deixar espumar!"
                }
            }
        },

        // 4. A Cilada no Museu Imperial (SUSPEITO INOCENTE / BODE EXPIATÓRIO)
        new()
        {
            Title = "A Cilada no Museu Imperial",
            Context = "Durante o blecaute no Baile de Gala, o valioso colar Rossi desapareceu. Dario Fontana foi encontrado caído no subsolo com as chaves mestras e ferramentas ao lado da vitrine quebrada.",
            Objective = "Comprove a inocência de Dario Fontana, desfaça a armação forjada pelo Curador Lorenzo Vitti e recupere a joia.",
            NpcName = "Dario Fontana",
            NpcRole = "Guarda Noturno • Museu Imperial",
            NpcPersonality = "Veterano humilde, honesto e apavorado por ver sua honra destruída e sua neta Laurinha desamparada.",
            IsGuilty = false,
            SecretTruth = "Dario Fontana é totalmente inocente. O curador corrupto Lorenzo Vitti forjou o roubo para receber dois milhões da seguradora. Vitti bateu na cabeça de Dario no escuro, plantou as ferramentas dele na vitrine e escondeu o colar Rossi dentro da estátua de mármore do salão norte.",
            OpeningGmNarration = "Dario Fontana treme visivelmente sobre a mesa de interrogatório. Seus olhos marejados encaram você com desespero genuíno e mãos machucadas.",
            OpeningNpcDialogue = "\"Eu trabalho há vinte anos como guarda noturno sem nunca ter levado uma advertência, detetive! Quando as luzes apagaram, alguém me golpeou por trás no porão! Quando acordei, já estavam me acusando de ladrão!\"",
            AlibiSpeech = "Eu estava fazendo minha ronda normal quando o disjuntor estourou às 23h40. Desci para checar a chave geral e levei uma pancada violenta na nuca no escuro!",
            EmpathyEarlySpeech = "Obrigado por me ouvir sem gritar comigo, detetive... a diretoria do museu estava à beira da falência e Lorenzo Vitti vivia dizendo que um seguro milionário salvaria tudo.",
            EarlyWhoWhereSpeech = "Lorenzo Vitti ficou no museu até tarde trancado na sala da curadoria... ele é a única pessoa com chave cópia do meu armário de ferramentas!",
            WeakPointLabel = "sua neta doente Laurinha",
            WeakPointKeywords = new[] { "neta", "laurinha", "laura", "família", "menina", "salvar", "remédios", "proteção" },
            WeakPointReactionSpeech = "Pela vida da minha netinha Laurinha, eu conto tudo o que vi! Pouco antes da luz apagar, vi Lorenzo Vitti saindo da curadoria com uma sacola preta de veludo!",
            ConfessionSpeech = "Lorenzo Vitti armou tudo para fraudar o seguro! Ele escondeu o colar Rossi dentro do pedestal oco da estátua de mármore do salão norte antes de me golpear!",
            ExonerationSpeech = "A justiça divina existe! Muito obrigado por acreditar na verdade, detetive! O verdadeiro criminoso é Lorenzo Vitti e o colar Rossi está na estátua de mármore!",
            KeyAccompliceWord = "Lorenzo Vitti",
            KeyConfessionWord = "estátua de mármore",
            HiddenObjectLabel = "o colar de diamantes Rossi",
            BonusClueTitle = "Armação de Lorenzo Vitti",
            BonusClueDescription = "Dario revelou que Lorenzo Vitti forjou o apagão e plantou as ferramentas para incriminá-lo perante a polícia.",
            BonusClueKeywords = new[] { "lorenzo", "vitti", "armação", "estátua", "estatua", "mármore", "seguro" },
            Clues = new[]
            {
                new ClueBlueprint
                {
                    Title = "Luva de Couro no Disjuntor",
                    Description = "Uma luva de trabalho com o nome de Dario foi encontrada presa na alavanca do disjuntor do subsolo.",
                    Keywords = new[] { "luva", "graxa", "disjuntor", "alavanca", "subsolo", "luz", "blecaute" },
                    ExcuseSpeech = "Vitti pegou meu par de luvas emprestado ontem à tarde dizendo que precisava consertar uma torneira na curadoria!",
                    AdmissionSpeech = "Vitti usou minhas luvas deliberadamente para deixar marcas de propósito no painel elétrico e jogar a culpa em mim!"
                },
                new ClueBlueprint
                {
                    Title = "Ferimento na Nuca do Suspeito",
                    Description = "Dario apresenta um corte com hematoma na parte posterior da cabeça compatível com coronhada.",
                    Keywords = new[] { "ferimento", "corte", "nuca", "cabeça", "coronhada", "médico", "hematoma" },
                    ExcuseSpeech = "Eu caí desacordado no concreto com o golpe de ferro! Como eu poderia ter quebrado a vitrine no salão de cima se estava desmaiado no porão?!",
                    AdmissionSpeech = "Foi o capanga alto de Lorenzo Vitti que me acertou por trás quando eu alcancei a lanterna no escuro!"
                },
                new ClueBlueprint
                {
                    Title = "Ordem de Serviço Manuscrita",
                    Description = "No bolso de Dario havia uma folha de papel mandando o guarda descer ao subsolo às 23h35.",
                    Keywords = new[] { "ordem", "serviço", "manuscrita", "papel", "vitti", "bilhete" },
                    ExcuseSpeech = "Aquele bilhete foi deixado na minha guarita com a assinatura do próprio curador Lorenzo Vitti!",
                    AdmissionSpeech = "Essa ordem de serviço prova que Vitti me mandou para a emboscada no porão minutos antes do alarme tocar!"
                },
                new ClueBlueprint
                {
                    Title = "Marcas de Alavanca na Vitrine",
                    Description = "A vitrine foi forçada com pé-de-cabra profissional pesado, incompatível com a chave de fenda de Dario.",
                    Keywords = new[] { "marcas", "alavanca", "vitrine", "pé-de-cabra", "ferramentas", "curadoria" },
                    ExcuseSpeech = "Minha caixa só tem chaves de fenda pequenas para fiação! A vitrine foi rebentada com ferro pesado da curadoria!",
                    AdmissionSpeech = "O pé-de-cabra pertencia à oficina de restauro de Lorenzo Vitti! Ele mesmo arrombou a vitrine antes de me golpear!"
                }
            }
        },

        // 5. Dívida de Sangue no Hipódromo
        new()
        {
            Title = "Dívida de Sangue no Hipódromo",
            Context = "O jóquei campeão Rubens 'Relâmpago' Dias foi achado morto nos estábulos após vencer o Grande Prêmio contra as ordens da máfia.",
            Objective = "Faça o treinador César Galvão confessar quem ordenou a execução nos estábulos e onde está a seringa usada para dopar os cavalos.",
            NpcName = "César Galvão",
            NpcRole = "Treinador-Chefe • Hipódromo Cidade Baixa",
            NpcPersonality = "Rude, supersticioso e orgulhoso de seus cavalos, mas aterrorizado de perder a licença e deixar sua esposa Marta na miséria.",
            SecretTruth = "O banqueiro do jogo do bicho, Otávio 'Barão' Mendes, apostou 200 mil na derrota de Rubens. Quando Rubens venceu, Mendes foi aos estábulos à meia-noite e o matou; César foi forçado a limpar a cena e escondeu a seringa de veneno e os tíquetes de aposta dentro da sela de couro nº 7.",
            OpeningGmNarration = "O cheiro de feno úmido e tabaco barato acompanha César Galvão na cadeira de ferro. Ele gira o chapéu amassado entre os dedos calejados, evitando encarar a lâmpada.",
            OpeningNpcDialogue = "\"O garoto Rubens era como um filho pra mim naquela pista, detetive. Quando voltei pros estábulos depois da meia-noite pra dar água ao cavalo, já encontrei o rapaz caído na palha. Deve ter sido algum apostador bêbado.\"",
            AlibiSpeech = "Eu estava comemorando a vitória do Grande Prêmio no bar dos proprietários até a meia-noite. Quando desci às baias, o Rubens já estava sem vida.",
            EmpathyEarlySpeech = "Ninguém vence uma corrida marcada na Cidade Baixa e sai impune, detetive. Eu implorei pro Rubens segurar as rédeas na reta final, mas o garoto tinha orgulho demais.",
            EarlyWhoWhereSpeech = "Os homens que mandam nas bancas de apostas são donos de metade desta rua. Se eu apontar o dedo para o Barão sem proteção, incendeiam minha casa com minha mulher dentro.",
            WeakPointLabel = "sua esposa Marta e sua casa",
            WeakPointKeywords = new[] { "esposa", "mulher", "marta", "casa", "licença", "licenca", "proteção", "proteger", "acordo" },
            WeakPointReactionSpeech = "Minha esposa Marta tem o coração fraco, ela não aguentaria se os capangas do Barão queimarem nossa casa! Se você me der garantia, eu conto: foi o banqueiro Otávio 'Barão' Mendes quem matou o Rubens nas baias!",
            ConfessionSpeech = "Eu conto a verdade, detetive! O banqueiro Otávio 'Barão' Mendes perdeu duzentos mil dólares quando o Rubens se recusou a entregar a corrida. À meia-noite o Barão Mendes invadiu o estábulo, matou o garoto e me obrigou a esconder a seringa de dopagem e os comprovantes dentro do forro da sela de couro número 7!",
            KeyAccompliceWord = "Barão Mendes",
            KeyConfessionWord = "sela de couro",
            HiddenObjectLabel = "a seringa e os comprovantes",
            BonusClueTitle = "Aposta de 200 Mil do Barão",
            BonusClueDescription = "César admitiu que Otávio 'Barão' Mendes ordenou a derrota do cavalo nº 7 e foi cobrar Rubens pessoalmente.",
            BonusClueKeywords = new[] { "barão", "barao", "otávio", "otavio", "mendes", "sela", "seringa" },
            Clues = new[]
            {
                new ClueBlueprint
                {
                    Title = "Espora Suja de Sangue",
                    Description = "No balde de limpeza do galpão de César foi encontrada uma bota lavada às pressas com vestígios de sangue da vítima.",
                    Keywords = new[] { "espora", "bota", "sangue", "balde", "limpeza", "galpão" },
                    ExcuseSpeech = "Aquele sangue no balde era de um cavalo baio que machucou a canela na ferradura durante o treino da tarde, detetive.",
                    AdmissionSpeech = "O sangue na minha bota era do Rubens sim! Eu tentei socorrer o garoto no chão do estábulo logo depois que bateram nele!"
                },
                new ClueBlueprint
                {
                    Title = "Bilhete de Aposta Rasgado",
                    Description = "No bolso do colete de César havia um canhoto de aposta clandestina de 30 mil dólares contra o próprio cavalo vencedor.",
                    Keywords = new[] { "bilhete", "aposta", "canhoto", "30 mil", "trinta mil", "colete", "contra" },
                    ExcuseSpeech = "Achei esse canhoto de aposta jogado no chão da arquibancada, nunca apostaria contra o meu próprio cavalo campeão.",
                    AdmissionSpeech = "Eu apostei contra o nosso cavalo porque a corrida estava comprada! O Rubens tinha prometido perder, mas mudou de ideia na última curva!"
                },
                new ClueBlueprint
                {
                    Title = "Frasco de Sedativo Equino",
                    Description = "O armário veterinário de César estava destrancado e faltava uma ampola de sedativo cardíaco de alta dosagem.",
                    Keywords = new[] { "frasco", "sedativo", "ampola", "veterinário", "veterinario", "armário", "cavalo", "veneno" },
                    ExcuseSpeech = "Aquela ampola de sedativo quebrou sozinha na prateleira semana passada durante o transporte das caixas.",
                    AdmissionSpeech = "Entreguei aquela ampola antes da largada para desacelerar o cavalo, mas o Rubens não aplicou... e usaram a seringa contra o próprio garoto!"
                },
                new ClueBlueprint
                {
                    Title = "Ferradura Adulterada com Chumbo",
                    Description = "Uma ferradura modificada para desequilibrar o cavalo campeão foi achada escondida sob a palha da baia.",
                    Keywords = new[] { "ferradura", "chumbo", "adulterada", "cavalo", "baia", "peso", "palha", "estábulo" },
                    ExcuseSpeech = "Aquela ferradura velha foi descartada pelo ferrador na semana passada por estar torta.",
                    AdmissionSpeech = "Fui eu quem colocou o chumbo na ferradura a mando do Barão, mas o Rubens percebeu e trocou antes da largada!"
                }
            }
        },

        // 6. A Queda do Maestro no Teatro Municipal
        new()
        {
            Title = "A Queda do Maestro no Teatro Municipal",
            Context = "O primeiro-violinista Enrico Bianco despencou no fosso da orquestra após o rompimento intencional do cabo do mezanino às 01h30.",
            Objective = "Faça o maestro Arthur Montenegro confessar quem cortou os cabos e onde escondeu a tesoura cirúrgica e as partituras chantagistas.",
            NpcName = "Arthur Montenegro",
            NpcRole = "Maestro Titular • Orquestra Sinfônica Municipal",
            NpcPersonality = "Arrogante, perfeccionista e elitista, mas apavorado de perder a tutela de sua filha doente Cecília.",
            SecretTruth = "A soprano rival Bianca Morena chantageou Arthur para sabotar o palco de Enrico. Arthur usou uma tesoura cirúrgica e escondeu as cartas de chantagem e a tesoura dentro da caixa do violoncelo nº 3 no fosso da orquestra.",
            OpeningGmNarration = "Arthur Montenegro tamborila os dedos longos na mesa de metal com desprezo visível, o lenço de bolso de seda ainda alinhado no paletó de veludo escuro.",
            OpeningNpcDialogue = "\"Enrico Bianco era um instrumentista medíocre que cambaleava bêbado após cada récita. Se aquele cabo de aço cedeu no mezanino, a culpa é da manutenção precária deste teatro decrépito, não da minha batuta.\"",
            AlibiSpeech = "Eu permaneci no camarote da diretoria revisando a partitura do segundo ato até a uma e meia da manhã, como os porteiros bem sabem.",
            EmpathyEarlySpeech = "O mundo da ópera é uma cova de serpentes, detetive... você não imagina as pressões que recaem sobre a cabeça de quem comanda esta casa.",
            EarlyWhoWhereSpeech = "A mulher por trás daquela sabotagem tem metade dos juízes desta cidade jantando em sua mesa aos domingos. Se eu abrir a boca, minha filha é arrancada de mim.",
            WeakPointLabel = "sua filha Cecília no leito",
            WeakPointKeywords = new[] { "filha", "cecília", "cecilia", "tutela", "doente", "leito", "família", "proteção" },
            WeakPointReactionSpeech = "Não encoste na minha Cecília! Ela depende dos meus cuidados diários... foi a soprano Bianca Morena quem me chantageou com dívidas para cortar a sustentação do mezanino!",
            ConfessionSpeech = "Chega desse tormento! A soprano Bianca Morena jurou me tirar a tutela de Cecília se eu não calasse Enrico Bianco. Fui eu quem afrouxou o cabo às pressas, e escondi a tesoura e as cartas de chantagem dentro da caixa do violoncelo número 3 no fosso!",
            KeyAccompliceWord = "Bianca Morena",
            KeyConfessionWord = "violoncelo número 3",
            HiddenObjectLabel = "a tesoura cirúrgica e as cartas",
            BonusClueTitle = "Chantagem de Bianca Morena",
            BonusClueDescription = "Montenegro admitiu que a soprano Bianca Morena o forçou a cortar a sustentação do mezanino para assumir os solos.",
            BonusClueKeywords = new[] { "bianca", "morena", "soprano", "violoncelo", "partitura" },
            Clues = new[]
            {
                new ClueBlueprint
                {
                    Title = "Fita de Resina Cortada",
                    Description = "No mezanino foi encontrada fita de sustentação cortada em bisel por lâmina cirúrgica de alta precisão.",
                    Keywords = new[] { "fita", "resina", "corte", "bisel", "lâmina", "cirúrgica", "cabo" },
                    ExcuseSpeech = "Aquele cabo foi desgastado pela ferrugem do ar úmido dos bastidores, qualquer perito incompetente sabe disso.",
                    AdmissionSpeech = "O corte em bisel foi feito por mim sim... cortei três quartos das fibras para que o cabo se rompesse quando ele pisasse!"
                },
                new ClueBlueprint
                {
                    Title = "Luvas de Seda com Óleo Mineral",
                    Description = "As luvas brancas de Arthur continham vestígios de graxa mineral usada exclusivamente nas roldanas do teto.",
                    Keywords = new[] { "luva", "luvas", "seda", "óleo", "oleo", "graxa", "roldana" },
                    ExcuseSpeech = "Toquei no corrimão da escada de serviço quando subi para verificar a iluminação cênica antes do ensaio geral.",
                    AdmissionSpeech = "Subi até as roldanas sim! Usei as luvas para não deixar digitais quando sabotei o guincho do mezanino!"
                },
                new ClueBlueprint
                {
                    Title = "Partitura Rasgada com Ameaça",
                    Description = "Na pasta de couro do maestro havia uma folha pautada de Enrico com a inscrição 'Você pagará por cada nota roubada'.",
                    Keywords = new[] { "partitura", "ameaça", "rasgada", "enrico", "pasta", "música" },
                    ExcuseSpeech = "Enrico vivia me acusando de plágio em delírios etílicos; guardei o bilhete para abrir um processo disciplinar na diretoria.",
                    AdmissionSpeech = "Enrico tinha descoberto que registrei os arranjos dele no meu nome... ele ia me desmascarar na frente de toda a imprensa esta manhã!"
                },
                new ClueBlueprint
                {
                    Title = "Alicate Cirúrgico no Camarote",
                    Description = "Um alicate cirúrgico de aço com marcas de pressão e dentes afiados foi localizado na lixeira privativa do maestro.",
                    Keywords = new[] { "alicate", "cirúrgico", "cirurgico", "camarote", "aço", "cabo", "dentes" },
                    ExcuseSpeech = "Uso aquela ferramenta para afinar as cravelhas emperradas dos instrumentos de corda da orquestra.",
                    AdmissionSpeech = "Fui eu quem usou aquele alicate para romper os fios de sustentação antes do início do espetáculo!"
                }
            }
        },

        // 7. A Emboscada na Alfaiataria Camargo (SUSPEITO INOCENTE / INCULPADO)
        new()
        {
            Title = "A Emboscada na Alfaiataria Camargo",
            Context = "O agiota Miro Peçanha foi executado dentro do provador da Alfaiataria Camargo. O idoso alfaiate Raul foi detido com o revólver ensanguentado escondido em sua loja.",
            Objective = "Comprove a inocência de Raul Camargo, desmascare a farsa montada pelo Delegado Peixoto e encontre a prova que inocenta o alfaiate.",
            NpcName = "Raul Camargo",
            NpcRole = "Mestre Alfaiate • Baixa Cidade",
            NpcPersonality = "Senhor pacato, piedoso e aterrorizado por ver seu irmão caçula Tiago e sua oficina de uma vida inteira destruídos.",
            IsGuilty = false,
            SecretTruth = "Raul Camargo é completamente inocente. O Delegado corrupto Peixoto devia cem mil ao agiota Miro. Peixoto entrou na alfaiataria enquanto Raul foi à esquina, atirou em Miro e enfiou o revólver no forro do manequim nº 4 para incriminar o velho alfaiate.",
            OpeningGmNarration = "Raul Camargo mantém a fita métrica pendurada no pescoço sobre o jaleco gasto. Suas mãos trêmulas de idoso tremem sem parar diante da acusação terrível.",
            OpeningNpcDialogue = "\"Eu costuro ternos nesta praça há quarenta anos e nunca matei uma formiga, detetive! Quando voltei da mercearia da esquina com linhas novas, a polícia já estava na minha porta arrombando tudo!\"",
            AlibiSpeech = "Eu saí para comprar linhas de seda na armarinho da Rua Direita por volta das duas da tarde; tenho até a nota de compra no bolso!",
            EmpathyEarlySpeech = "Graças a Deus o senhor é um homem sensato... o Delegado Peixoto me odeia porque me recusei a costurar distintivos falsos para a quadrilha dele no mês passado.",
            EarlyWhoWhereSpeech = "O verdadeiro criminoso usa distintivo dourado na lapela e manda nos guardas desta delegacia. O Delegado Peixoto estava na minha porta minutos antes do crime!",
            WeakPointLabel = "seu irmão caçula Tiago",
            WeakPointKeywords = new[] { "irmão", "irmao", "tiago", "caçula", "cacula", "família", "salvar", "proteção" },
            WeakPointReactionSpeech = "Pelo amor que você tem à sua mãe, salvem meu irmão Tiago da pensão! O Delegado Peixoto jurou matar o garoto se eu contasse que foi ele quem entrou no provador com uma arma!",
            ConfessionSpeech = "O Delegado Peixoto armou tudo! Ele devia cem mil ao agiota e aproveitou minha ida à esquina para atirar em Miro pelas costas e enfiar o revólver 32 no forro do manequim número 4 da minha oficina!",
            ExonerationSpeech = "Deus abençoe suas mãos, detetive! Você salvou um velho alfaiate da forca! O verdadeiro assassino é o Delegado Peixoto e a arma está no manequim número 4!",
            KeyAccompliceWord = "Delegado Peixoto",
            KeyConfessionWord = "manequim número 4",
            HiddenObjectLabel = "o revólver calibre 32 de Peixoto",
            BonusClueTitle = "Conluio do Delegado Peixoto",
            BonusClueDescription = "Raul revelou que o Delegado Peixoto forjou o homicídio para se livrar do agiota e jogou a arma em seu manequim.",
            BonusClueKeywords = new[] { "delegado", "peixoto", "manequim", "revólver", "revolver", "armação" },
            Clues = new[]
            {
                new ClueBlueprint
                {
                    Title = "Fita Métrica com Pólvora",
                    Description = "A fita métrica deixada sobre o balcão continha manchas superficiais de fuligem.",
                    Keywords = new[] { "fita", "métrica", "metrica", "pólvora", "polvora", "resíduos", "queima" },
                    ExcuseSpeech = "Isso não é pólvora! É carvão do ferro de passar a vapor que usamos para passar as golas de terno!",
                    AdmissionSpeech = "A fuligem na fita caiu quando o capanga de Peixoto puxou a fita métrica para estrangular Miro antes de atirar!"
                },
                new ClueBlueprint
                {
                    Title = "Paletó Encomendado por Peixoto",
                    Description = "No cabide do provador havia um paletó de oficial da lei com coldre embutido sob medida.",
                    Keywords = new[] { "terno", "paletó", "paleto", "peixoto", "arma", "cabide", "coldre" },
                    ExcuseSpeech = "Peixoto encomendou aquele paletó semana passada e exigiu que ficasse pronto hoje às duas da tarde!",
                    AdmissionSpeech = "Aquele paletó era a desculpa de Peixoto para entrar no provador armado sem levantar suspeitas de ninguém!"
                },
                new ClueBlueprint
                {
                    Title = "Nota Fiscal da Armarinho",
                    Description = "No bolso de Raul havia um canhoto carimbado com data e horário das 14h05 da loja da esquina.",
                    Keywords = new[] { "nota", "fiscal", "armarinho", "canhoto", "14h05", "recibo", "álibi" },
                    ExcuseSpeech = "Olhe o horário carimbado: 14h05! O tiro soou às 14h02 enquanto eu estava no caixa da armarinho!",
                    AdmissionSpeech = "Esse recibo é a prova irrefutável de que eu estava a três quadras de distância quando Peixoto apertou o gatilho!"
                },
                new ClueBlueprint
                {
                    Title = "Coldre sob Medida no Manequim",
                    Description = "No forro interno do manequim nº 4 havia um coldre de couro com o brasão oficial da delegacia gravado.",
                    Keywords = new[] { "coldre", "brasão", "brasao", "manequim", "couro", "delegacia", "oficial" },
                    ExcuseSpeech = "Eu nunca usei coldres na minha alfaiataria! Aquele coldre com brasão é de uso exclusivo de oficiais da lei!",
                    AdmissionSpeech = "Aquele coldre pertence ao Delegado Peixoto! Foi ele quem enfiou a arma ali enquanto a loja estava vazia!"
                }
            }
        },

        // 8. Os Negativos Proibidos da Alta Roda
        new()
        {
            Title = "Os Negativos Proibidos da Alta Roda",
            Context = "O colunista de escândalos Otávio Brandão foi encontrado asfixiado no laboratório fotográfico da Gazeta da Tarde.",
            Objective = "Faça o fotógrafo forense Bruno Alencar confessar quem furtou as chapas fotográficas e onde escondeu os negativos comprometedores.",
            NpcName = "Bruno Alencar",
            NpcRole = "Fotógrafo Forense • Gazeta da Tarde",
            NpcPersonality = "Cínico, observador e acostumado a lidar com cadáveres, mas entra em desespero quando o nome de sua noiva Olívia surge na apuração.",
            SecretTruth = "O advogado da máfia Dr. Salazar comprou Bruno para sumir com as fotos do suborno do Senador. Bruno escondeu o rolo de negativos de nitrato dentro do fundo falso da bandeja de revelação fotográfica.",
            OpeningGmNarration = "O cheiro penetrante de vinagre e fixador químico impregna as roupas de Bruno Alencar. Ele cruza as pernas na sala de interrogatório, girando a aliança de noivado no anelar.",
            OpeningNpcDialogue = "\"Fotografo mortos e desgraças nesta cidade há doze anos, doutor. Otávio Brandão colecionava inimigos de todos os calibres políticos. Ele deve ter respirado ácido de revelação por engano no escuro.\"",
            AlibiSpeech = "Eu passei a noite inteira no plantão do necrotério cobrindo os acidentes de trânsito da Zona Norte até as três da manhã.",
            EmpathyEarlySpeech = "Brandão era um parasita que vivia de devassar a intimidade alheia para extorquir ricaços desavisados... ninguém vai chorar sobre a cova dele.",
            EarlyWhoWhereSpeech = "O homem que mandou queimar aqueles negativos tem poder para transferir o senhor para uma delegacia de fronteira antes do amanhecer, detetive.",
            WeakPointLabel = "sua noiva Olívia",
            WeakPointKeywords = new[] { "noiva", "olívia", "olivia", "casamento", "família", "noivado", "segredo", "proteção" },
            WeakPointReactionSpeech = "Não tragam a Olívia para este esgoto! Ela acredita que sou um homem honesto... foi o Dr. Salazar quem me deu o ultimato para apagar as fotos do Senador!",
            ConfessionSpeech = "Eu desisto, detetive! Dr. Salazar mandou dois capangas sufocarem Brandão com um pano embebido em solvente. Antes de levarem o corpo, eu escondi os negativos de nitrato no fundo falso da bandeja de revelação número três do laboratório!",
            KeyAccompliceWord = "Dr. Salazar",
            KeyConfessionWord = "bandeja de revelação",
            HiddenObjectLabel = "os negativos de nitrato",
            BonusClueTitle = "Extorsão do Dr. Salazar",
            BonusClueDescription = "Alencar confessou que o advogado mafioso Dr. Salazar ordenou a morte de Brandão para blindar o Senador.",
            BonusClueKeywords = new[] { "salazar", "advogado", "bandeja", "negativos", "nitrato" },
            Clues = new[]
            {
                new ClueBlueprint
                {
                    Title = "Filme de 35mm Queimado",
                    Description = "No cinzeiro do laboratório havia tiras queimadas de celulóide com traços de imagens de uma mala de dinheiro.",
                    Keywords = new[] { "filme", "35mm", "queimado", "celulóide", "celuloide", "mala", "fotos" },
                    ExcuseSpeech = "Queimei rolos velhos com defeito de exposição, fazemos isso todas as semanas para não acumular lixo inflamável.",
                    AdmissionSpeech = "Eram as fotos do Senador recebendo o envelope na suíte! Queimei apenas as bordas para assustar o Brandão, mas Salazar queria sangue!"
                },
                new ClueBlueprint
                {
                    Title = "Frasco de Fixador com Solvente",
                    Description = "O frasco químico de Alencar continha vestígios de clorofórmio e solvente puro idêntico ao inalado pela vítima.",
                    Keywords = new[] { "frasco", "fixador", "solvente", "clorofórmio", "cloroformio", "químico" },
                    ExcuseSpeech = "Usamos solventes industriais para limpar as lentes objetivas e os filtros ópticos das câmeras de estúdio.",
                    AdmissionSpeech = "Fui eu quem comprou o solvente a mando de Salazar... eu sabia que iam apagar o homem no quartinho escuro!"
                },
                new ClueBlueprint
                {
                    Title = "Recibo de Joalheria de 25 Mil",
                    Description = "No bolso de Bruno havia a nota fiscal de um anel de diamantes quitado em dinheiro vivo duas horas após o crime.",
                    Keywords = new[] { "recibo", "anel", "diamantes", "25 mil", "joalheria", "dinheiro", "olívia" },
                    ExcuseSpeech = "Economizei cada centavo dos meus extras no jornal durante cinco anos para dar esse anel de noivado à mulher que amo.",
                    AdmissionSpeech = "Aquele adiantamento em dinheiro vivo foi a paga de Salazar para garantir meu silêncio na perícia do necrotério!"
                },
                new ClueBlueprint
                {
                    Title = "Chapa de Vidro Submersa no Ácido",
                    Description = "Uma chapa de revelação com os negativos intactos do senador foi achada no fundo do tanque de lavagem ácida.",
                    Keywords = new[] { "chapa", "vidro", "tanque", "ácida", "negativos", "senador", "submersa", "fotos" },
                    ExcuseSpeech = "Esqueci chapas velhas mergulhadas no tanque de enxágue durante a limpeza do fim de semana.",
                    AdmissionSpeech = "Mergulhei aquela chapa no ácido para que Salazar pensasse que destruí tudo... era a minha única garantia de vida!"
                }
            }
        },

        // 9. A Trapaça Mortal do Cassino Riviera
        new()
        {
            Title = "A Trapaça Mortal do Cassino Riviera",
            Context = "O financista corrupto Gastão Morais foi alvejado com silenciador na adega privativa do Cassino Riviera após embolsar 200 mil fichas.",
            Objective = "Faça a crupiê de elite Verônica Prado confessar quem ordenou o disparo e onde escondeu os tíquetes de caixa clandestinos.",
            NpcName = "Verônica Prado",
            NpcRole = "Crupiê de Roleta • Cassino Riviera",
            NpcPersonality = "Sedutora, calculista e perspicaz, finge indiferença mundana, mas entra em colapso se sua mãe doente for desamparada.",
            SecretTruth = "O dono clandestino do cassino, 'Tubarão' Silveira, descobriu que Gastão usava um ímã nos dados. Silveira executou Gastão e Verônica escondeu as fichas de alto valor e os tíquetes no duto de ventilação da sala VIP.",
            OpeningGmNarration = "Verônica Prado observa você por trás de uma nuvem de fumaça aromática de cigarro turco, com seu vestido de seda esmeralda destoando das paredes cinzentas da delegacia.",
            OpeningNpcDialogue = "\"Gastão Morais era um trapaceiro arrogante que pensava que a sorte sorria para o charme dele. Se ele levou chumbo na adega, detetive, aposto que foi cobrança de alguma das dezenas de mulheres que ele lesou nesta praça.\"",
            AlibiSpeech = "Eu estava operando a mesa principal de bacará diante de trinta testemunhas no salão dourado até as três da manhã.",
            EmpathyEarlySpeech = "Um homem que ganha duzentos mil na roleta em meia hora assina o próprio atestado de óbito nesta cidade, detetive... você sabe como a casa opera.",
            EarlyWhoWhereSpeech = "O homem que puxou o gatilho naquela adega é quem banca a campanha do Secretário de Segurança. Quer mesmo que eu morra envenenada na cela esta noite?",
            WeakPointLabel = "sua mãe idosa dona Alzira",
            WeakPointKeywords = new[] { "mãe", "mae", "alzira", "idosa", "remédios", "asilo", "família", "proteção" },
            WeakPointReactionSpeech = "Parem! A minha mãe não suportaria um escândalo desses, ela nem imagina onde eu trabalho à noite! Foi o 'Tubarão' Silveira quem ordenou a execução do Gastão!",
            ConfessionSpeech = "Eu conto onde estão as provas! Silveira descobriu o golpe do Gastão e disparou à queima-roupa na adega. Fui eu quem pegou os tíquetes clandestinos de duzentos mil e escondi tudo dentro do duto de ventilação atrás da tapeçaria da sala VIP!",
            KeyAccompliceWord = "Tubarão Silveira",
            KeyConfessionWord = "duto de ventilação",
            HiddenObjectLabel = "os tíquetes de caixa e as fichas",
            BonusClueTitle = "Ordem do Tubarão Silveira",
            BonusClueDescription = "Verônica admitiu que o proprietário clandestino Silveira ordenou a morte de Gastão para não pagar o prêmio.",
            BonusClueKeywords = new[] { "tubarão", "tubarao", "silveira", "duto", "fichas", "ventilação" },
            Clues = new[]
            {
                new ClueBlueprint
                {
                    Title = "Ficha de Marfim Manchada",
                    Description = "No forro da bolsa de festa de Verônica foi achada uma ficha especial de marfim de 50 mil gravada com o nome de Gastão.",
                    Keywords = new[] { "ficha", "marfim", "50 mil", "bolsa", "gastão", "gastao", "sangue" },
                    ExcuseSpeech = "Gastão me deu aquela ficha como gorjeta pública após a primeira rodada vitoriosa no pano verde, dezenas de apostadores viram.",
                    AdmissionSpeech = "Gastão tentou me dar aquela ficha na adega implorando para que eu abrisse a saída de emergência antes de Silveira chegar!"
                },
                new ClueBlueprint
                {
                    Title = "Ímã Cilíndrico de Precisão",
                    Description = "Um eletroímã miniaturizado compatível com a roleta foi localizado na caixa de maquiagem de Verônica.",
                    Keywords = new[] { "ímã", "ima", "eletroímã", "roleta", "maquiagem", "cilíndrico", "trapaça" },
                    ExcuseSpeech = "Aquele cilindro de metal é apenas a haste retrátil do meu estojo de rímel para cílios, qualquer mulher carrega isso.",
                    AdmissionSpeech = "Eu operava o ímã sob o tapete da roleta para Silveira controlar as perdas... Gastão descobriu o mecanismo e começou a nos chantagear!"
                },
                new ClueBlueprint
                {
                    Title = "Chave da Adega Privativa",
                    Description = "O registro de segurança indicou que o cartão perfurado pessoal de Verônica destrancou a adega às 02h45.",
                    Keywords = new[] { "chave", "cartão", "cartao", "adega", "02h45", "perfurado", "registro" },
                    ExcuseSpeech = "Desci para buscar champanhe safra especial para a mesa do cônsul e devo ter registrado o ponto por mero engano de relógio.",
                    AdmissionSpeech = "Eu abri a porta da adega às duas e quarenta e cinco porque Silveira me mandou atrair o Gastão até lá com a promessa do pagamento em dinheiro vivo!"
                },
                new ClueBlueprint
                {
                    Title = "Dados com Centro de Chumbo",
                    Description = "No cinzeiro de cristal da sala VIP foram encontrados dados viciados com núcleo de chumbo pesado.",
                    Keywords = new[] { "dados", "chumbo", "viciados", "núcleo", "nucleo", "roleta", "trapaça", "pesado" },
                    ExcuseSpeech = "Esses dados são relíquias decorativas que ficam expostas no mostruário do cassino.",
                    AdmissionSpeech = "Gastão usou esses dados viciados para quebrar a banca! Silveira não aceitou a perda e desceu armado para cobrar a conta!"
                }
            }
        },

        // 10. O Xarope da Farmácia Central
        new()
        {
            Title = "O Xarope da Farmácia Central",
            Context = "O venerável Juiz Eurico Brandão faleceu em convulsões após tomar seu xarope noturno adulterado com estricnina concentrada.",
            Objective = "Faça o químico farmacêutico Dr. Rodolfo Mendes confessar quem pagou pela manipulação letal e onde guardou o livro de registros de venenos.",
            NpcName = "Dr. Rodolfo Mendes",
            NpcRole = "Químico Farmacêutico • Farmácia Central",
            NpcPersonality = "Metódico, tímido e ansioso, ajeita os óculos sem parar e desmorona com facilidade quando sua filha universitária é mencionada.",
            SecretTruth = "A viúva do juiz, dona Amélia Brandão, pagou 100 mil dólares para Rodolfo trocar o sedativo por estricnina. Rodolfo escondeu o livro negro de substâncias controladas no fundo falso do armário de morfina.",
            OpeningGmNarration = "Dr. Rodolfo Mendes esfrega as mãos nervosamente sob a luz da mesa, o jaleco branco salpicado de manchas amareladas de reagentes químicos.",
            OpeningNpcDialogue = "\"Eu avio receitas com precisão matemática há trinta anos nesta praça, senhor detetive. Se o frasco do juiz continha veneno, o xarope foi manipulado após sair do balcão da minha farmácia.\"",
            AlibiSpeech = "Eu encerrei meu plantão às oito da noite e entreguei o frasco de xarope lacrado ao motorista particular da família Brandão.",
            EmpathyEarlySpeech = "O juiz Brandão era um homem autoritário que tratava a própria esposa como prisioneira em casa... uma situação deplorável que a vizinhança inteira conhecia.",
            EarlyWhoWhereSpeech = "A pessoa que comprou aquele frasco tem advogados influentes no Supremo Tribunal. Se eu falar, minha filha perde a bolsa de estudos e a vida.",
            WeakPointLabel = "sua filha Júlia que estuda no exterior",
            WeakPointKeywords = new[] { "filha", "júlia", "julia", "exterior", "bolsa", "estudos", "família", "médica" },
            WeakPointReactionSpeech = "Minha filha Júlia não pode pagar pelos meus erros! Ela é tudo o que me resta... foi a viúva Amélia Brandão quem me coagiu a trocar as ampolas!",
            ConfessionSpeech = "Eu confesso perante a lei! Dona Amélia Brandão descobriu que seria deserdada e me ofereceu cem mil dólares para dosar a estricnina no xarope. O livro com as fórmulas originais e os pagamentos está escondido no fundo falso do armário de morfina da farmácia!",
            KeyAccompliceWord = "Amélia Brandão",
            KeyConfessionWord = "armário de morfina",
            HiddenObjectLabel = "o livro de substâncias controladas",
            BonusClueTitle = "Pacto com Amélia Brandão",
            BonusClueDescription = "Mendes confessou que a viúva Amélia Brandão encomendou o envenenamento para herdar a fortuna do juiz.",
            BonusClueKeywords = new[] { "amélia", "amelia", "viúva", "morfina", "estricnina", "livro" },
            Clues = new[]
            {
                new ClueBlueprint
                {
                    Title = "Balança Analítica com Estricnina",
                    Description = "A perícia encontrou microcristais de estricnina nos pratos da balança de precisão privativa de Rodolfo.",
                    Keywords = new[] { "balança", "balanca", "estricnina", "precisão", "cristais", "química" },
                    ExcuseSpeech = "Estricnina é usada em microdoses terapêuticas em tônicos cardíacos legais devidamente autorizados pela vigilância sanitária.",
                    AdmissionSpeech = "Pesei cinco gramas da estricnina pura sim... a dosagem que coloquei naquele xarope era suficiente para parar o coração de dez homens!"
                },
                new ClueBlueprint
                {
                    Title = "Cheque Nominal de 50 Mil",
                    Description = "Um cheque da conta pessoal de Amélia Brandão foi depositado na caderneta de Mendes na véspera da morte.",
                    Keywords = new[] { "cheque", "50 mil", "cinquenta mil", "amélia", "amelia", "depósito", "banco" },
                    ExcuseSpeech = "Aquele cheque quitava uma antiga encomenda de medicamentos caros e cosméticos importados da Europa para a senhora Brandão.",
                    AdmissionSpeech = "Aquele cheque foi apenas a primeira metade do suborno que dona Amélia me prometeu para preparar a poção mortal!"
                },
                new ClueBlueprint
                {
                    Title = "Ampola Quebrada no Lixo da Botica",
                    Description = "No lixo químico dos fundos foram encontrados cacos de ampola com selo de importação alemão de veneno concentrado.",
                    Keywords = new[] { "ampola", "lixo", "cacos", "alemão", "veneno", "botica", "selo" },
                    ExcuseSpeech = "Descartei ampolas velhas vencidas que estavam ocupando espaço nas gavetas superiores do laboratório.",
                    AdmissionSpeech = "Quebrei a ampola para fingir acidente caso a polícia fizesse vistoria de estoque logo após o velório!"
                },
                new ClueBlueprint
                {
                    Title = "Livro de Fórmulas com Rasuras",
                    Description = "No livro oficial de controle da farmácia, a página da receita do juiz teve a dosagem apagada com ácido.",
                    Keywords = new[] { "livro", "fórmulas", "formulas", "receitas", "rasuras", "ácido", "dosagem", "estricnina" },
                    ExcuseSpeech = "Derramei reagente sobre o livro por acidente enquanto limpava a bancada de vidro.",
                    AdmissionSpeech = "Eu rasurei a quantidade de estricnina no livro para que os fiscais da saúde não descobrissem a dose letal!"
                }
            }
        }
    };

    /// <summary>
    /// Seleciona um caso que ainda não foi visto na sessão, ou gera um novo caso procedural inédito.
    /// </summary>
    public static (NoirCaseBlueprint Blueprint, bool IsProcedurallyGenerated) GetUniqueBlueprint(HashSet<string>? usedTitles = null)
    {
        usedTitles ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Tenta pegar um caso dos artesanais que ainda não foi jogado
        var available = BaseBlueprints.Where(b => !usedTitles.Contains(b.Title)).ToList();
        if (available.Count > 0)
        {
            var chosen = available[Rng.Next(available.Count)];
            usedTitles.Add(chosen.Title);
            return (chosen, false);
        }

        // 2. Se todos os casos base já foram jogados, sintetiza um caso procedural inédito e único!
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
