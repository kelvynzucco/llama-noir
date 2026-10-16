using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FontStashSharp;
using LlamaInterrogation.Models;
using LlamaInterrogation.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LlamaInterrogation;

public class InterrogationGame : Game
{
    private const int WindowWidth = 1360;
    private const int WindowHeight = 820;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private FontSystem _fontSystem = null!;
    private RasterizerState _scissorRasterizer = null!;

    private readonly InterrogationState _state = new();
    private readonly MockLlmTurnProvider _mockProvider = new();
    private readonly LlamafileTurnProvider _llamafileProvider = new("http://127.0.0.1:8080");

    private bool _isWaitingForLlm = false;
    private bool _isGeneratingCase = false;
    private string _pendingPlayerAction = string.Empty;
    private bool _focusCurrentTurnOnly = false;

    private string _statusBanner = "A lâmpada fria oscila sobre a mesa metálica. O interrogatório vai começar.";

    // Efeito Typewriter (máquina de escrever)
    private float _typewriterCharsVisible = 9999f;
    private int _lastAnimatedTurnCount = 1;
    private const float TypewriterSpeed = 85f;

    // Cache de layout para evitar realocações de strings e medições a 60 FPS em PCs modestos
    private readonly List<(TurnRecord Record, bool IsLatest, int Height, string WrappedPlayer, string WrappedGm, string WrappedNpc)> _cachedLayoutBlocks = new();
    private int _cachedTotalContentHeight = 18;
    private int _cachedPendingBlockHeight = 0;
    private string _cachedWrappedPendingPlayer = string.Empty;
    private int _lastCacheTurnCount = -1;
    private int _lastCacheTypewriterChars = -1;
    private bool _lastCacheFocusMode = false;
    private bool _lastCacheWaitingLlm = false;
    private string _lastCacheCaseTitle = string.Empty;

    // Animação suave das barras
    private float _displayTrust = 35f;
    private float _displayStress = 25f;
    private float _displayProgress = 10f;

    // Scroll do histórico de chat
    private float _chatScrollOffset = 0f;
    private float _maxChatScroll = 0f;
    private bool _autoScrollToBottom = true;
    private int _previousScrollWheelValue = 0;

    private MouseState _prevMouse;
    private KeyboardState _prevKeyboard;
    private double _cursorBlinkTimer = 0;

    // Regiões clicáveis da UI e estado de pistas anexadas
    private Rectangle _toggleViewModeRect;
    private Rectangle _newCaseButtonRect;
    private Rectangle _dialogueRestartRect;
    private Rectangle _clearClueButtonRect;
    private ClueItem? _selectedClue = null;
    private readonly List<(Rectangle Rect, DialogueOption Option)> _dialogueOptionRects = new();
    private readonly List<(Rectangle Rect, ClueItem Clue)> _clueRects = new();

    public InterrogationGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = WindowWidth,
            PreferredBackBufferHeight = WindowHeight,
            SynchronizeWithVerticalRetrace = true
        };
        Window.Title = "DELEGACIA CENTRAL 1947 • SALA DE INTERROGATÓRIO NOIR";
        Window.AllowUserResizing = false;
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) => _llamafileProvider.KillAllLlamafileProcesses();
        _state.CaseTitle = "ABRINDO NOVO INQUÉRITO FORENSE...";
        _state.CaseObjective = "Consultando arquivos confidenciais do departamento...";
        _state.NpcName = "Convocando Suspeito...";
        _state.NpcRole = "Verificando antecedentes criminais...";
        base.Initialize();

        // Já abre o jogo encontrando e gerando um caso aleatório inédito!
        _ = GenerateNewProceduralCaseAsync();
    }

    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        _llamafileProvider.Dispose();
        base.OnExiting(sender, args);
    }

    protected override void Dispose(bool disposing)
    {
        _llamafileProvider.Dispose();
        _scissorRasterizer?.Dispose();
        base.Dispose(disposing);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _scissorRasterizer = new RasterizerState { ScissorTestEnable = true };

        _fontSystem = new FontSystem();
        string[] candidateFonts =
        {
            @"C:\Windows\Fonts\segoeui.ttf",
            @"C:\Windows\Fonts\arial.ttf",
            @"C:\Windows\Fonts\calibri.ttf",
            @"C:\Windows\Fonts\consola.ttf"
        };

        foreach (string fontPath in candidateFonts)
        {
            if (File.Exists(fontPath))
            {
                _fontSystem.AddFont(File.ReadAllBytes(fontPath));
                break;
            }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _cursorBlinkTimer += dt;

        if (_state.TurnRecords.Count != _lastAnimatedTurnCount)
        {
            _lastAnimatedTurnCount = _state.TurnRecords.Count;
            _typewriterCharsVisible = 0f;
        }
        else
        {
            _typewriterCharsVisible = Math.Min(9999f, _typewriterCharsVisible + dt * TypewriterSpeed);
        }

        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        // Atalhos de função
        if (keyboard.IsKeyDown(Keys.F3) && !_prevKeyboard.IsKeyDown(Keys.F3))
            _focusCurrentTurnOnly = !_focusCurrentTurnOnly;

        if (keyboard.IsKeyDown(Keys.F5) && !_prevKeyboard.IsKeyDown(Keys.F5))
            _ = GenerateNewProceduralCaseAsync();

        // Interpolação suave dos medidores
        _displayTrust = MathHelper.Lerp(_displayTrust, _state.Trust, dt * 6f);
        _displayStress = MathHelper.Lerp(_displayStress, _state.Stress, dt * 6f);
        _displayProgress = MathHelper.Lerp(_displayProgress, _state.CaseProgress, dt * 6f);

        // Rolagem do histórico com a roda do mouse
        int scrollDelta = mouse.ScrollWheelValue - _previousScrollWheelValue;
        _previousScrollWheelValue = mouse.ScrollWheelValue;
        if (scrollDelta != 0 && !_focusCurrentTurnOnly)
        {
            _autoScrollToBottom = false;
            _chatScrollOffset = MathHelper.Clamp(_chatScrollOffset - scrollDelta * 0.42f, 0f, _maxChatScroll);
        }

        // Teclas de atalho para seleção rápida das opções de diálogo (1, 2, 3, 4)
        if (!_isWaitingForLlm && !_isGeneratingCase && _state.Outcome == GameOutcome.EmAndamento)
        {
            var active = _state.GetActiveDialogueOptions(4);
            if ((keyboard.IsKeyDown(Keys.D1) && !_prevKeyboard.IsKeyDown(Keys.D1)) ||
                (keyboard.IsKeyDown(Keys.NumPad1) && !_prevKeyboard.IsKeyDown(Keys.NumPad1)))
            {
                if (active.Count > 0)
                    _ = SubmitDialogueChoiceAsync(active[0], _selectedClue);
            }
            else if ((keyboard.IsKeyDown(Keys.D2) && !_prevKeyboard.IsKeyDown(Keys.D2)) ||
                     (keyboard.IsKeyDown(Keys.NumPad2) && !_prevKeyboard.IsKeyDown(Keys.NumPad2)))
            {
                if (active.Count > 1)
                    _ = SubmitDialogueChoiceAsync(active[1], _selectedClue);
            }
            else if ((keyboard.IsKeyDown(Keys.D3) && !_prevKeyboard.IsKeyDown(Keys.D3)) ||
                     (keyboard.IsKeyDown(Keys.NumPad3) && !_prevKeyboard.IsKeyDown(Keys.NumPad3)))
            {
                if (active.Count > 2)
                    _ = SubmitDialogueChoiceAsync(active[2], _selectedClue);
            }
            else if ((keyboard.IsKeyDown(Keys.D4) && !_prevKeyboard.IsKeyDown(Keys.D4)) ||
                     (keyboard.IsKeyDown(Keys.NumPad4) && !_prevKeyboard.IsKeyDown(Keys.NumPad4)))
            {
                if (active.Count > 3)
                    _ = SubmitDialogueChoiceAsync(active[3], _selectedClue);
            }
        }

        // Cliques do mouse
        bool leftClick = mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
        if (leftClick)
        {
            if (_typewriterCharsVisible < 600f)
                _typewriterCharsVisible = 9999f;

            Point mp = mouse.Position;

            if (_toggleViewModeRect.Contains(mp))
            {
                _focusCurrentTurnOnly = !_focusCurrentTurnOnly;
                _autoScrollToBottom = true;
            }
            else if (_newCaseButtonRect.Contains(mp))
            {
                _ = GenerateNewProceduralCaseAsync();
            }
            else if (_state.Outcome != GameOutcome.EmAndamento && _dialogueRestartRect.Contains(mp))
            {
                _ = GenerateNewProceduralCaseAsync();
            }
            else if (!_clearClueButtonRect.IsEmpty && _clearClueButtonRect.Contains(mp))
            {
                _selectedClue = null;
            }
            else if (!_isWaitingForLlm && !_isGeneratingCase && _state.Outcome == GameOutcome.EmAndamento)
            {
                foreach (var (rect, option) in _dialogueOptionRects)
                {
                    if (rect.Contains(mp))
                    {
                        _ = SubmitDialogueChoiceAsync(option, _selectedClue);
                        break;
                    }
                }

                foreach (var (rect, clue) in _clueRects)
                {
                    if (rect.Contains(mp))
                    {
                        clue.IsNew = false;
                        if (ReferenceEquals(_selectedClue, clue))
                            _selectedClue = null;
                        else
                            _selectedClue = clue;
                        break;
                    }
                }
            }
        }

        _prevMouse = mouse;
        _prevKeyboard = keyboard;
        base.Update(gameTime);
    }

    private async Task GenerateNewProceduralCaseAsync()
    {
        if (_isWaitingForLlm || _isGeneratingCase)
            return;

        _isGeneratingCase = true;
        _selectedClue = null;
        _pendingPlayerAction = string.Empty;

        _statusBanner = "Abrindo arquivos da delegacia... Convocando suspeito para interrogatório...";

        try
        {
            var generatedCase = await _llamafileProvider.GenerateNewCaseAsync();
            _state.ApplyGeneratedCase(generatedCase);
            _statusBanner = $"Inquérito Aberto: {_state.CaseTitle} • Suspeito: {_state.NpcName}";
        }
        catch
        {
            var fallbackCase = await _mockProvider.GenerateNewCaseAsync();
            _state.ApplyGeneratedCase(fallbackCase);
            _statusBanner = $"Inquérito Aberto: {_state.CaseTitle} • Suspeito: {_state.NpcName}";
        }

        _autoScrollToBottom = true;
        _typewriterCharsVisible = 0f;
        _lastAnimatedTurnCount = 0; // força animação typewriter na abertura
        _isGeneratingCase = false;
    }

    private async Task SubmitDialogueChoiceAsync(DialogueOption option, ClueItem? attachedClue = null)
    {
        if (_isWaitingForLlm || _isGeneratingCase || _state.Outcome != GameOutcome.EmAndamento)
            return;

        option.IsUsed = true;
        _isWaitingForLlm = true;

        string actionText;
        ClueSynergyType synergy;
        int trustDelta = option.TrustDelta;
        int stressDelta = option.StressDelta;
        int progressDelta = option.ProgressDelta;
        string gmNarration;
        string npcDialogue;

        if (attachedClue != null)
        {
            bool isCompatible = option.IsClueCompatible(attachedClue);
            if (isCompatible)
            {
                synergy = ClueSynergyType.Positive;
                actionText = $"{option.PlayerDialogue} [Apresentando a prova: '{attachedClue.Title}']";
                gmNarration = $"{option.GmNarration} Você apresenta '{attachedClue.Title}' sobre a mesa. {_state.NpcName} empalidece diante da contradição escancarada pela evidência documental.";
                npcDialogue = $"\"Maldita seja essa evidência... não imaginei que vocês tinham '{attachedClue.Title.ToLowerInvariant()}' em mãos! Não tenho como negar diante dessa prova!\"";
                trustDelta = option.TrustDelta + 10;
                stressDelta = Math.Clamp(option.StressDelta, -10, 15);
                progressDelta = option.ProgressDelta + 20;
            }
            else
            {
                synergy = ClueSynergyType.Negative;
                actionText = $"{option.PlayerDialogue} [Tentando associar com: '{attachedClue.Title}']";
                gmNarration = $"Você joga '{attachedClue.Title}' sobre a mesa, mas a prova não tem qualquer relação com o que você acabou de dizer. {_state.NpcName} percebe seu blefe desconexo e recupera a compostura.";
                npcDialogue = $"\"Detetive, o que '{attachedClue.Title}' tem a ver com o que você acabou de me dizer?! Você está atirando no escuro e querendo me intimidar com papéis aleatórios!\"";
                trustDelta = option.TrustDelta - 15;
                stressDelta = option.StressDelta + 15;
                progressDelta = Math.Max(0, option.ProgressDelta - 10);
            }
        }
        else
        {
            synergy = ClueSynergyType.None;
            actionText = option.PlayerDialogue;
            gmNarration = option.GmNarration;
            npcDialogue = option.NpcDialogue;
        }

        _selectedClue = null;
        _pendingPlayerAction = actionText;
        foreach (var c in _state.Clues) c.IsNew = false;

        int executedTurn = _state.CurrentTurn;
        _state.History.Add(new ChatLogEntry
        {
            Type = LogEntryType.PlayerAction,
            Speaker = "VOCÊ (DETETIVE)",
            Text = actionText,
            Turn = executedTurn
        });
        _autoScrollToBottom = true;

        string synStatus = synergy switch
        {
            ClueSynergyType.Positive => " (★ Prova Pericial Compatível!)",
            ClueSynergyType.Negative => " (⚠ Evidência Desconexa!)",
            _ => ""
        };
        _statusBanner = $"Registrando depoimento oficial no prontuário{synStatus}...";

        await Task.Delay(180);

        var output = new LlmTurnOutput
        {
            GmNarration = gmNarration,
            NpcDialogue = npcDialogue,
            NpcEmotion = option.NpcEmotion.ToString(),
            TrustDelta = trustDelta,
            StressDelta = stressDelta,
            ProgressDelta = progressDelta,
            NewClueTitle = option.NewClueTitle,
            NewClueDescription = option.NewClueDescription,
            AttachedClueTitle = attachedClue?.Title,
            ClueSynergy = synergy
        };

        ApplyTurnOutput(output, executedTurn, actionText);
        _pendingPlayerAction = string.Empty;
        _isWaitingForLlm = false;
        _autoScrollToBottom = true;
    }

    private async Task SubmitTurnAsync(string message)
    {
        var matching = _state.DialogueTree.FirstOrDefault(o => !o.IsUsed && o.PlayerDialogue == message)
                       ?? _state.DialogueTree.FirstOrDefault(o => !o.IsUsed);
        if (matching != null)
        {
            await SubmitDialogueChoiceAsync(matching, _selectedClue);
        }
    }

    private void ApplyTurnOutput(LlmTurnOutput output, int executedTurn, string playerMessage)
    {
        _state.Trust = Math.Clamp(_state.Trust + output.TrustDelta, 0, 100);
        _state.Stress = Math.Clamp(_state.Stress + output.StressDelta, 0, 100);
        _state.CaseProgress = Math.Clamp(_state.CaseProgress + output.ProgressDelta, 0, 100);

        if (Enum.TryParse<EmotionalState>(output.NpcEmotion, true, out var parsedEmotion))
            _state.Emotion = parsedEmotion;

        if (!string.IsNullOrWhiteSpace(output.GmNarration))
        {
            _state.History.Add(new ChatLogEntry
            {
                Type = LogEntryType.GmNarration,
                Speaker = "OBSERVAÇÃO FORENSE",
                Text = output.GmNarration,
                Turn = executedTurn
            });
        }

        if (!string.IsNullOrWhiteSpace(output.NpcDialogue))
        {
            _state.History.Add(new ChatLogEntry
            {
                Type = LogEntryType.NpcDialogue,
                Speaker = _state.NpcName,
                Text = output.NpcDialogue,
                Turn = executedTurn
            });
        }

        string? unlockedClueTitle = null;
        if (!string.IsNullOrWhiteSpace(output.NewClueTitle) &&
            !_state.Clues.Any(c => c.Title.Equals(output.NewClueTitle, StringComparison.OrdinalIgnoreCase)))
        {
            unlockedClueTitle = output.NewClueTitle!;
            _state.Clues.Add(new ClueItem
            {
                Id = $"clue_{_state.Clues.Count + 1}",
                Title = unlockedClueTitle,
                Description = output.NewClueDescription ?? "Pista revelada durante o interrogatório.",
                IsNew = true
            });
        }

        var record = new TurnRecord
        {
            TurnNumber = executedTurn,
            IsOpeningScene = false,
            PlayerAction = playerMessage,
            GmNarration = output.GmNarration,
            NpcDialogue = output.NpcDialogue,
            NpcEmotion = _state.Emotion,
            TrustDelta = output.TrustDelta,
            StressDelta = output.StressDelta,
            ProgressDelta = output.ProgressDelta,
            UnlockedClueTitle = unlockedClueTitle,
            AttachedClueTitle = output.AttachedClueTitle,
            ClueSynergy = output.ClueSynergy
        };

        if (output.SuggestedPrompts is { Count: > 0 })
            _state.QuickSuggestions = output.SuggestedPrompts.Take(3).ToList();

        // Condição de Derrota 1: Confiança Zero (O suspeito fecha a guarda por hostilidade desmedida)
        if (_state.Trust <= 0)
        {
            _state.Outcome = GameOutcome.DerrotaSilencioTotal;
            record.SystemConclusionTitle = "INTERROGATÓRIO ENCERRADO — CONFIANÇA ZERO";
            record.SystemConclusionText = _state.IsGuilty
                ? $"Suas acusações agressivas sem base empurraram {_state.NpcName} para a defensiva total! Ele percebeu que você não tem provas concretas e recusa-se a pronunciar uma única palavra sem advogado."
                : $"{_state.NpcName} percebeu que você está determinado a incriminá-lo a qualquer custo, ignorando sua inocência! Desesperado com a parcialidade policial, o suspeito calou-se em pânico.";
            record.IsVictoryConclusion = false;
            _state.TurnRecords.Add(record);
            _statusBanner = "DERROTA: A confiança do suspeito caiu a zero e o interrogatório travou. Pressione [F5] para novo caso.";
            return;
        }

        // Condição de Derrota 2: Pressão Psicológica 100%
        if (_state.Stress >= 100)
        {
            if (_state.IsGuilty)
            {
                _state.Outcome = GameOutcome.DerrotaSilencioTotal;
                record.SystemConclusionTitle = "INTERROGATÓRIO ENCERRADO — PRESSÃO 100%";
                record.SystemConclusionText = $"A pressão psicológica passou do limite! {_state.NpcName} fechou-se em silêncio absoluto e exigiu a presença imediata do advogado. Pressione [F5] para gerar um novo caso.";
                _statusBanner = "DERROTA: O suspeito culpado travou sob pressão extrema (100%). Pressione [F5] para novo caso.";
            }
            else
            {
                _state.Outcome = GameOutcome.DerrotaCoacaoInocente;
                record.SystemConclusionTitle = "FALSO TESTEMUNHO POR COAÇÃO — INQUÉRITO ANULADO!";
                record.SystemConclusionText = $"TRAGÉDIA FORENSE: {_state.NpcName} era inocente, mas a pressão extrema (100%) o levou ao colapso mental! Em pânico, ele assumiu a culpa de um crime que não cometeu. A promotoria descobriu a coação indevida, o caso foi anulado e o verdadeiro assassino escapou impune!";
                _statusBanner = "DERROTA: Você coagiu um inocente até uma falsa confissão (100% estresse)! Pressione [F5] para novo caso.";
            }
            record.IsVictoryConclusion = false;
            _state.TurnRecords.Add(record);
            return;
        }

        // Condição de Vitória: Progresso 100%
        if (_state.CaseProgress >= 100)
        {
            if (_state.IsGuilty)
            {
                _state.Outcome = GameOutcome.VitoriaConfissao;
                record.SystemConclusionTitle = "CASO SOLUCIONADO — CONFISSÃO OBTIDA!";
                record.SystemConclusionText = $"{_state.NpcName} confessou todos os crimes e entregou a localização da prova oculta antes da chegada do advogado! Pressione [F5] para assumir um novo inquérito forense.";
                _statusBanner = "VITÓRIA! Você obteve a confissão completa do culpado antes do tempo acabar! Pressione [F5] para novo inquérito.";
            }
            else
            {
                _state.Outcome = GameOutcome.VitoriaInocencia;
                record.SystemConclusionTitle = "INOCÊNCIA COMPROVADA — FARSA DESMASCARADA!";
                record.SystemConclusionText = $"BRILHANTE DEDUÇÃO: Você provou que {_state.NpcName} foi vítima de uma armadilha, desmontou as falsas evidências e obteve as pistas para prender o verdadeiro mandante! Pressione [F5] para assumir outro inquérito.";
                _statusBanner = "VITÓRIA! Inocência comprovada e armação desmascarada antes do advogado chegar! Pressione [F5] para novo inquérito.";
            }
            record.IsVictoryConclusion = true;
            _state.TurnRecords.Add(record);
            return;
        }

        // Condição de Derrota 3: Tempo Esgotado (10 Turnos)
        if (_state.CurrentTurn >= _state.MaxTurns)
        {
            _state.Outcome = GameOutcome.DerrotaTempoEsgotado;
            record.SystemConclusionTitle = "TEMPO ESGOTADO — O ADVOGADO CHEGOU";
            record.SystemConclusionText = $"Os 10 turnos acabaram! O advogado de defesa abriu a porta e retirou {_state.NpcName} da sala antes da elucidação completa do caso. Pressione [F5] para gerar um novo caso.";
            record.IsVictoryConclusion = false;
            _state.TurnRecords.Add(record);
            _statusBanner = "DERROTA: Os 10 turnos se esgotaram. Pressione [F5] para gerar um novo caso.";
            return;
        }

        _state.TurnRecords.Add(record);
        _state.CurrentTurn++;
        _statusBanner = $"Turno {_state.CurrentTurn} de {_state.MaxTurns} • Sua vez de interrogar (faltam {_state.MaxTurns - _state.CurrentTurn + 1} turnos para o advogado chegar).";
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(13, 16, 23));
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);

        DrawTopBar(gameTime);
        DrawLeftSuspectAndCluesPanel(gameTime);
        DrawRightConversationPanel(gameTime);

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawTopBar(GameTime gameTime)
    {
        var fontTitle = _fontSystem.GetFont(22);
        var fontObj = _fontSystem.GetFont(17);
        var fontSub = _fontSystem.GetFont(15);

        // 1. HEADER PRINCIPAL (Y = 0, Altura = 48)
        var topRect = new Rectangle(0, 0, WindowWidth, 48);
        DrawPanel(topRect, new Color(19, 24, 35), new Color(40, 50, 72), 1);

        // Distintivo policial dourado em pixel art ao lado do título
        DrawPixelBadge(18, 16, new Color(250, 204, 21));

        string rawTitle = _isGeneratingCase ? "ABRINDO NOVO INQUÉRITO FORENSE..." : _state.CaseTitle;
        var fTitle = fontTitle.MeasureString(rawTitle).X <= 750 ? fontTitle : _fontSystem.GetFont(18);
        string titleDisplay = TruncateText(fTitle, rawTitle, 750);
        _spriteBatch.DrawString(fTitle, titleDisplay, new Vector2(40, 13), new Color(240, 244, 255));

        // Indicador de Gravação Forense Contínua (Sem alternância, sempre gravando o interrogatório)
        int recX = WindowWidth - 530;
        DrawPixelTapeRecorder(recX, 16, gameTime, true);
        _spriteBatch.DrawString(fontSub, "GRAVAÇÃO FORENSE", new Vector2(recX + 28, 15), new Color(156, 175, 205));

        // Botão de Modo de Visualização (Cena Atual vs Histórico Completo)
        _toggleViewModeRect = new Rectangle(WindowWidth - 340, 6, 160, 36);
        bool hoverView = _toggleViewModeRect.Contains(_prevMouse.Position);
        DrawPanel(_toggleViewModeRect,
            hoverView ? new Color(45, 58, 82) : new Color(30, 38, 56),
            _focusCurrentTurnOnly ? new Color(56, 189, 248) : new Color(85, 100, 135), 1);
        DrawPixelMagnifyingGlass(_toggleViewModeRect.X + 8, _toggleViewModeRect.Y + 11, _focusCurrentTurnOnly ? new Color(56, 189, 248) : new Color(160, 175, 205));
        string viewText = _focusCurrentTurnOnly ? "[F3] Turno Atual" : "[F3] Arquivo Geral";
        _spriteBatch.DrawString(fontSub, viewText, new Vector2(_toggleViewModeRect.X + 28, _toggleViewModeRect.Y + 8), Color.White);

        // Botão de Gerar Novo Inquérito Policial
        _newCaseButtonRect = new Rectangle(WindowWidth - 170, 6, 154, 36);
        bool hoverReset = _newCaseButtonRect.Contains(_prevMouse.Position);
        DrawPanel(_newCaseButtonRect,
            _isGeneratingCase ? new Color(75, 60, 24) : (hoverReset ? new Color(98, 48, 120) : new Color(68, 34, 88)),
            new Color(192, 132, 252), 1);
        DrawPixelFolder(_newCaseButtonRect.X + 8, _newCaseButtonRect.Y + 12, new Color(250, 204, 21));
        string newCaseLabel = _isGeneratingCase ? "Buscando Arquivo..." : "[F5] NOVO CASO";
        _spriteBatch.DrawString(fontSub, newCaseLabel, new Vector2(_newCaseButtonRect.X + 30, _newCaseButtonRect.Y + 8), new Color(245, 230, 255));

        // 2. BARRA DE CONTEXTO NOIR + OBJETIVO EM DESTAQUE ABAIXO DO HEADER (Centralizada e sem cortar o texto)
        // 2. BARRA DE CONTEXTO NOIR + OBJETIVO EM DESTAQUE ABAIXO DO HEADER (Sem cortar textos)
        var objBannerRect = new Rectangle(16, 52, WindowWidth - 32, 48);
        DrawPanel(objBannerRect, new Color(30, 24, 14), new Color(234, 179, 8), 2);
        DrawFilledRect(new Rectangle(objBannerRect.X, objBannerRect.Y, 5, objBannerRect.Height), new Color(250, 204, 21));
        DrawFilledRect(new Rectangle(objBannerRect.Right - 5, objBannerRect.Y, 5, objBannerRect.Height), new Color(250, 204, 21));

        string rawObj = _isGeneratingCase ? "Abrindo novos arquivos da delegacia..." : _state.CaseObjective;
        float maxObjWidth = objBannerRect.Width - 32f;

        if (_isGeneratingCase || !rawObj.Contains('\n'))
        {
            var fontClean = fontObj.MeasureString(rawObj).X <= maxObjWidth ? fontObj : _fontSystem.GetFont(14);
            var sz = fontClean.MeasureString(rawObj);
            float ox = objBannerRect.X + Math.Max(12, (objBannerRect.Width - sz.X) / 2f);
            float oy = objBannerRect.Y + (objBannerRect.Height - sz.Y) / 2f;
            _spriteBatch.DrawString(fontClean, rawObj, new Vector2(ox, oy), new Color(254, 240, 138));
        }
        else
        {
            // Exibe Linha 1 (CONTEXTO do crime Noir) + Linha 2 (OBJETIVO em destaque dourado) sem cortes
            string[] parts = rawObj.Split('\n', 2, StringSplitOptions.RemoveEmptyEntries);
            string contextLine = parts[0].Trim();
            string objectiveLine = parts.Length > 1 ? parts[1].Trim() : string.Empty;

            var fontCtx = fontSub.MeasureString(contextLine).X <= maxObjWidth ? fontSub : _fontSystem.GetFont(13);
            var fontGoal = fontObj.MeasureString(objectiveLine).X <= maxObjWidth ? fontObj : _fontSystem.GetFont(14);

            var ctxSize = fontCtx.MeasureString(contextLine);
            float ctxX = objBannerRect.X + Math.Max(12, (objBannerRect.Width - ctxSize.X) / 2f);
            _spriteBatch.DrawString(fontCtx, contextLine, new Vector2(ctxX, objBannerRect.Y + 6), new Color(226, 232, 240));

            if (!string.IsNullOrEmpty(objectiveLine))
            {
                var objSize = fontGoal.MeasureString(objectiveLine);
                float objX = objBannerRect.X + Math.Max(12, (objBannerRect.Width - objSize.X) / 2f);
                _spriteBatch.DrawString(fontGoal, objectiveLine, new Vector2(objX, objBannerRect.Y + 26), new Color(250, 204, 21));
            }
        }
    }

    private void DrawLeftSuspectAndCluesPanel(GameTime gameTime)
    {
        var fontHeader = _fontSystem.GetFont(20);
        var fontBody = _fontSystem.GetFont(16);
        var fontSmall = _fontSystem.GetFont(14);
        var fontTiny = _fontSystem.GetFont(13);

        // Painel do Suspeito (X=16, Y=106, Largura=440, Altura=400)
        var suspectPanel = new Rectangle(16, 106, 440, 400);
        DrawPanel(suspectPanel, new Color(20, 24, 34), new Color(46, 56, 78), 1);

        var portraitRect = new Rectangle(30, 120, 148, 156);
        if (_isGeneratingCase)
        {
            DrawPanel(portraitRect, new Color(14, 18, 26), new Color(96, 165, 250), 2);
            DrawAnimatedMagnifyingGlass(portraitRect.Center.X, portraitRect.Center.Y, 22, gameTime);
        }
        else
        {
            DrawNpcPortrait(portraitRect, gameTime);
            DrawCornerPhotoMounts(portraitRect, new Color(85, 95, 115));
        }

        int infoX = 192;
        int infoWidth = suspectPanel.Right - infoX - 14; // ~234px
        string displayNpcName = _isGeneratingCase ? "Identificando..." : _state.NpcName;
        string displayNpcRole = _isGeneratingCase ? "Buscando antecedentes..." : _state.NpcRole;

        // Nome do suspeito com ajuste inteligente de fonte
        var fontName = fontHeader.MeasureString(displayNpcName).X <= infoWidth ? fontHeader : _fontSystem.GetFont(16);
        _spriteBatch.DrawString(fontName, displayNpcName, new Vector2(infoX, 118), new Color(240, 244, 255));

        // Profissão / Papel do suspeito com auto-ajuste de linha
        var fontRole = fontTiny;
        string wrappedRole = WrapText(fontRole, displayNpcRole, infoWidth);
        if (fontRole.MeasureString(wrappedRole).Y > 30)
        {
            fontRole = _fontSystem.GetFont(11.5f);
            wrappedRole = WrapText(fontRole, displayNpcRole, infoWidth);
        }
        _spriteBatch.DrawString(fontRole, wrappedRole, new Vector2(infoX, 142), new Color(165, 180, 210));

        // Postura Emocional
        Color emotionColor = GetEmotionColor(_state.Emotion);
        var emotionBox = new Rectangle(infoX, 176, 192, 28);
        DrawPanel(emotionBox, emotionColor * 0.22f, emotionColor, 1);
        _spriteBatch.DrawString(fontSmall, $"Postura: {_state.Emotion.ToString().ToUpperInvariant()}",
            new Vector2(emotionBox.X + 8, emotionBox.Y + 5), emotionColor);

        // Cinzeiro com cigarro e fumaça animada na sala de interrogatório
        DrawPixelCigaretteSmoke(infoX + 204, 184, gameTime);

        // Polígrafo Forense
        int bpm = 68 + (int)(_displayStress * 0.72f);
        _spriteBatch.DrawString(fontTiny, $"Polígrafo Forense: {bpm} BPM",
            new Vector2(infoX, 212), bpm > 115 ? new Color(255, 110, 110) : new Color(130, 210, 175));
        DrawPolygraphOscilloscope(new Rectangle(infoX, 230, infoWidth, 26), gameTime, bpm, bpm > 115 ? new Color(244, 63, 94) : new Color(52, 211, 153));

        // Medidores Psicológicos & Investigativos (Espaçamento padronizado de 68px, sem sobreposição)
        int meterY = 274;
        int meterWidth = suspectPanel.Width - 28;
        int meterX = suspectPanel.X + 14;

        DrawMeterBar(new Rectangle(meterX, meterY, meterWidth, 52),
            "CONFIANÇA DO SUSPEITO",
            $"{_state.Trust}%",
            _displayTrust / 100f,
            new Color(56, 189, 248),
            "Sobe com empatia e acordos. Evite hostilidade sem provas.");

        DrawMeterBar(new Rectangle(meterX, meterY + 68, meterWidth, 52),
            "PRESSÃO PSICOLÓGICA (LIMITE: 100%)",
            $"{_state.Stress}%",
            _displayStress / 100f,
            _state.Stress >= 75 ? new Color(244, 63, 94) : new Color(245, 158, 11),
            "Em 100%, o suspeito cala-se ou sofre colapso emocional.");

        DrawMeterBar(new Rectangle(meterX, meterY + 136, meterWidth, 52),
            "PROGRESSO DO INQUÉRITO (META: 100%)",
            $"{_state.CaseProgress}%",
            _displayProgress / 100f,
            new Color(52, 211, 153),
            "Ao atingir 100%, a verdade factual é totalmente elucidada.");

        // Dossiê de Evidências Forenses (X=16, Y=514, Largura=440, Altura=292)
        var cluesPanel = new Rectangle(16, 514, 440, 292);
        DrawPanel(cluesPanel, new Color(20, 24, 34), new Color(46, 56, 78), 1);
        DrawPixelFolder(30, 526, new Color(250, 204, 21));
        _spriteBatch.DrawString(fontBody, "DOSSIÊ DE EVIDÊNCIAS FORENSES", new Vector2(52, 522), new Color(250, 204, 21));
        _spriteBatch.DrawString(fontTiny, "Clique em uma evidência para anexá-la ao seu próximo confronto:", new Vector2(30, 544), new Color(145, 165, 195));

        _clueRects.Clear();
        if (_isGeneratingCase)
        {
            _spriteBatch.DrawString(fontBody, "Abrindo arquivos confidenciais...", new Vector2(32, 574), new Color(168, 180, 205));
        }
        else
        {
            var visibleClues = _state.Clues.Take(4).ToList();
            int clueCount = visibleClues.Count;
            int cardH = clueCount <= 3 ? 66 : 54;
            int gap = clueCount <= 3 ? 8 : 5;
            int clueY = 564;

            foreach (var clue in visibleClues)
            {
                var cardRect = new Rectangle(28, clueY, cluesPanel.Width - 24, cardH);
                bool hovered = cardRect.Contains(_prevMouse.Position);
                bool isSelected = ReferenceEquals(clue, _selectedClue);

                Color cardBg;
                Color cardBorder;
                int borderThickness = 1;

                if (isSelected)
                {
                    cardBg = new Color(55, 45, 18);
                    cardBorder = new Color(250, 204, 21);
                    borderThickness = 2;
                }
                else if (clue.IsNew)
                {
                    cardBg = new Color(50, 42, 22);
                    cardBorder = new Color(234, 179, 8);
                }
                else
                {
                    cardBg = hovered ? new Color(34, 42, 60) : new Color(26, 32, 46);
                    cardBorder = hovered ? new Color(100, 150, 220) : new Color(48, 58, 82);
                }

                DrawPanel(cardRect, cardBg, cardBorder, borderThickness);

                string actionLabel = isSelected ? "[Soltar]" : (hovered ? "[Anexar]" : "");
                int actWidth = 0;
                if (!string.IsNullOrEmpty(actionLabel))
                {
                    Color actionCol = isSelected ? new Color(250, 204, 21) : new Color(147, 197, 253);
                    var actSz = fontTiny.MeasureString(actionLabel);
                    actWidth = (int)actSz.X + 8;
                    _spriteBatch.DrawString(fontTiny, actionLabel, new Vector2(cardRect.Right - actSz.X - 10, cardRect.Y + 4), actionCol);
                }

                string titleText = isSelected
                    ? $"[✓ ANEXADA] {clue.Title}"
                    : (clue.IsNew ? $"★ [NOVA] {clue.Title}" : $"• {clue.Title}");
                Color titleColor = isSelected ? new Color(254, 240, 138) : (clue.IsNew ? new Color(254, 240, 138) : new Color(225, 235, 252));

                int maxTitleWidth = cardRect.Width - actWidth - 20;
                string safeTitle = TruncateText(fontSmall, titleText, maxTitleWidth);
                _spriteBatch.DrawString(fontSmall, safeTitle, new Vector2(cardRect.X + 10, cardRect.Y + 3), titleColor);

                // Descrição com ajuste dinâmico para nunca cortar
                var fDesc = fontTiny;
                string wrappedDesc = WrapText(fDesc, clue.Description, cardRect.Width - 20);
                if (fDesc.MeasureString(wrappedDesc).Y > cardH - 22)
                {
                    fDesc = _fontSystem.GetFont(11.5f);
                    wrappedDesc = WrapText(fDesc, clue.Description, cardRect.Width - 20);
                }
                _spriteBatch.DrawString(fDesc, wrappedDesc, new Vector2(cardRect.X + 10, cardRect.Y + 20), new Color(175, 190, 215));

                _clueRects.Add((cardRect, clue));
                clueY += cardH + gap;
            }
        }
    }

    private void DrawRightConversationPanel(GameTime gameTime)
    {
        var fontTitle = _fontSystem.GetFont(20);
        var fontNpc = _fontSystem.GetFont(20);
        var fontBody = _fontSystem.GetFont(18);
        var fontSmall = _fontSystem.GetFont(15);

        int rightX = 472;
        int rightWidth = WindowWidth - rightX - 16;

        // 1. BARRA VISUAL DE LINHA DO TEMPO DE TURNOS
        var timelineRect = new Rectangle(rightX, 106, rightWidth, 58);
        DrawTurnTimelineBar(timelineRect, fontTitle, fontSmall);

        // 2. PALCO DE DIÁLOGO E NARRATIVA
        var stagePanel = new Rectangle(rightX, 170, rightWidth, 378);
        DrawPanel(stagePanel, new Color(17, 21, 30), new Color(46, 56, 78), 1);

        if (_isGeneratingCase)
        {
            int dots = ((int)(gameTime.TotalGameTime.TotalSeconds * 3) % 3) + 1;
            string dotsStr = new('.', dots);
            var genBox = new Rectangle(stagePanel.X + 60, stagePanel.Y + 70, stagePanel.Width - 120, 230);
            DrawPanel(genBox, new Color(24, 30, 46), new Color(250, 204, 21), 2);

            // Desenha a Lupa Animada no centro superior do card
            DrawAnimatedMagnifyingGlass(genBox.Center.X, genBox.Y + 86, 34, gameTime);

            var fontSearch = _fontSystem.GetFont(24);
            string searchTitle = $"Abrindo arquivos confidenciais{dotsStr}";
            var stSize = fontSearch.MeasureString(searchTitle);
            _spriteBatch.DrawString(fontSearch, searchTitle,
                new Vector2(genBox.X + (genBox.Width - stSize.X) / 2f, genBox.Y + 164),
                new Color(254, 240, 138));
        }
        else
        {
            int innerWidth = stagePanel.Width - 40;
            int currentTypewriterInt = (int)_typewriterCharsVisible;
            bool needsRebuild =
                _lastCacheTurnCount != _state.TurnRecords.Count ||
                _lastCacheTypewriterChars != currentTypewriterInt ||
                _lastCacheFocusMode != _focusCurrentTurnOnly ||
                _lastCacheWaitingLlm != _isWaitingForLlm ||
                _lastCacheCaseTitle != _state.CaseTitle;

            if (needsRebuild)
            {
                _lastCacheTurnCount = _state.TurnRecords.Count;
                _lastCacheTypewriterChars = currentTypewriterInt;
                _lastCacheFocusMode = _focusCurrentTurnOnly;
                _lastCacheWaitingLlm = _isWaitingForLlm;
                _lastCacheCaseTitle = _state.CaseTitle;

                _cachedLayoutBlocks.Clear();
                _cachedTotalContentHeight = 18;

                var recordsToShow = _focusCurrentTurnOnly && _state.TurnRecords.Count > 0
                    ? new List<TurnRecord> { _state.TurnRecords[^1] }
                    : _state.TurnRecords;

                for (int i = 0; i < recordsToShow.Count; i++)
                {
                    var rec = recordsToShow[i];
                    bool isLatest = ReferenceEquals(rec, _state.TurnRecords[^1]) && !_isWaitingForLlm;

                    string npcTextToRender = rec.NpcDialogue;
                    if (isLatest && currentTypewriterInt < npcTextToRender.Length)
                    {
                        int count = Math.Clamp(currentTypewriterInt, 0, npcTextToRender.Length);
                        npcTextToRender = npcTextToRender[..count] + " ▌";
                    }

                    string wrappedPlayer = !string.IsNullOrWhiteSpace(rec.PlayerAction)
                        ? WrapText(fontBody, $"\"{rec.PlayerAction}\"", (int)(innerWidth * 0.78f) - 32)
                        : string.Empty;
                    string wrappedGm = WrapText(fontBody, rec.GmNarration, innerWidth - 44);
                    string wrappedNpc = WrapText(fontNpc, npcTextToRender, innerWidth - 48);

                    int blockH = MeasureTurnBlockHeight(rec, wrappedPlayer, wrappedGm, wrappedNpc, fontBody, fontNpc, innerWidth);
                    _cachedLayoutBlocks.Add((rec, isLatest, blockH, wrappedPlayer, wrappedGm, wrappedNpc));
                    _cachedTotalContentHeight += blockH + 22;
                }

                _cachedPendingBlockHeight = 0;
                _cachedWrappedPendingPlayer = string.Empty;
                if (_isWaitingForLlm)
                {
                    _cachedWrappedPendingPlayer = WrapText(fontBody, $"\"{_pendingPlayerAction}\"", (int)(innerWidth * 0.78f) - 32);
                    int pTextH = (int)fontBody.MeasureString(_cachedWrappedPendingPlayer).Y;
                    _cachedPendingBlockHeight = 40 + (pTextH + 40) + 14 + 68;
                    _cachedTotalContentHeight += _cachedPendingBlockHeight + 18;
                }
            }

            _maxChatScroll = Math.Max(0f, _cachedTotalContentHeight - (stagePanel.Height - 18));
            if (_autoScrollToBottom || _focusCurrentTurnOnly)
                _chatScrollOffset = _focusCurrentTurnOnly ? 0f : _maxChatScroll;

            _spriteBatch.End();
            var prevScissor = GraphicsDevice.ScissorRectangle;
            GraphicsDevice.ScissorRectangle = new Rectangle(stagePanel.X + 2, stagePanel.Y + 2, stagePanel.Width - 4, stagePanel.Height - 4);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, _scissorRasterizer);

            int drawY = stagePanel.Y + 14 - (int)_chatScrollOffset;
            foreach (var (rec, isLatest, blockH, wrappedPlayer, wrappedGm, wrappedNpc) in _cachedLayoutBlocks)
            {
                if (drawY + blockH >= stagePanel.Y && drawY <= stagePanel.Bottom)
                {
                    DrawTurnBlock(new Rectangle(stagePanel.X + 16, drawY, innerWidth, blockH),
                        rec, isLatest, wrappedPlayer, wrappedGm, wrappedNpc, fontSmall, fontBody, fontNpc);
                }
                drawY += blockH + 22;
            }

            if (_isWaitingForLlm)
            {
                DrawPendingTurnBlock(new Rectangle(stagePanel.X + 16, drawY, innerWidth, _cachedPendingBlockHeight),
                    _state.CurrentTurn, _cachedWrappedPendingPlayer, fontSmall, fontBody, gameTime);
            }

            _spriteBatch.End();
            GraphicsDevice.ScissorRectangle = prevScissor;
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);

            if (_maxChatScroll > 0 && !_focusCurrentTurnOnly)
            {
                int trackHeight = stagePanel.Height - 16;
                int thumbHeight = Math.Max(40, (int)(trackHeight * (stagePanel.Height / (float)_cachedTotalContentHeight)));
                int thumbY = stagePanel.Y + 8 + (int)((trackHeight - thumbHeight) * (_chatScrollOffset / _maxChatScroll));
                DrawFilledRect(new Rectangle(stagePanel.Right - 9, thumbY, 5, thumbHeight), new Color(90, 115, 160));
            }
        }

        // 3. OPÇÕES DE DIÁLOGO DO DETETIVE (RAMIFICAÇÃO TÁTICA)
        _dialogueOptionRects.Clear();
        var activeOptions = _state.GetActiveDialogueOptions(4);

        int optionsAreaY = 556;
        int optionsAreaHeight = 250;

        if (_state.Outcome != GameOutcome.EmAndamento)
        {
            _clearClueButtonRect = Rectangle.Empty;
            var endPanelRect = new Rectangle(rightX, optionsAreaY, rightWidth, optionsAreaHeight);
            bool isVictory = _state.Outcome is GameOutcome.VitoriaConfissao or GameOutcome.VitoriaInocencia;
            Color endBg = isVictory ? new Color(18, 34, 26) : new Color(36, 20, 22);
            Color endBorder = isVictory ? new Color(52, 211, 153) : new Color(248, 113, 113);
            DrawPanel(endPanelRect, endBg, endBorder, 2);

            string endTitle = _state.Outcome switch
            {
                GameOutcome.VitoriaConfissao => "CASO ARQUIVADO — CONFISSÃO OBTIDA!",
                GameOutcome.VitoriaInocencia => "INQUÉRITO CONCLUÍDO — INOCÊNCIA COMPROVADA!",
                GameOutcome.DerrotaCoacaoInocente => "INQUÉRITO ANULADO — FALSO TESTEMUNHO POR COAÇÃO",
                GameOutcome.DerrotaSilencioTotal => "INTERROGATÓRIO TRANCADO — SILÊNCIO TOTAL DO SUSPEITO",
                GameOutcome.DerrotaTempoEsgotado => "PRAZO ESGOTADO — ADVOGADO ASSUMIU A CUSTÓDIA",
                _ => "INQUÉRITO POLICIAL ENCERRADO"
            };

            string endSub = _state.Outcome switch
            {
                GameOutcome.VitoriaConfissao => $"Você obteve a confissão formal de {_state.NpcName} e recuperou a prova decisiva antes do advogado chegar!",
                GameOutcome.VitoriaInocencia => $"Você provou que {_state.NpcName} foi vítima de uma armadilha, desmascarou a farsa e apreendeu a prova cabal!",
                GameOutcome.DerrotaCoacaoInocente => $"Você coagiu um inocente até 100% de estresse e extraiu uma falsa confissão. A corregedoria anulou o inquérito!",
                GameOutcome.DerrotaSilencioTotal => (_state.Trust <= 0 ? "A desconfiança atingiu o fundo do poço (0%) e o suspeito recusa-se a pronunciar uma única palavra sem advogado." : $"{_state.NpcName} atingiu 100% de estresse e fechou-se em silêncio absoluto."),
                GameOutcome.DerrotaTempoEsgotado => "Os 10 turnos regulamentares se esgotaram antes que os fatos fossem plenamente elucidados.",
                _ => "O interrogatório policial chegou ao fim."
            };

            // Carimbo retrô de inquérito criminal no topo
            DrawRetroStamp(new Rectangle(endPanelRect.X + 24, endPanelRect.Y + 14, endPanelRect.Width - 48, 36), endTitle, endBorder, fontTitle);

            // Subtítulo descritivo com quebra automática de texto para nunca cortar
            string wrappedEndSub = WrapText(fontBody, endSub, endPanelRect.Width - 48);
            _spriteBatch.DrawString(fontBody, wrappedEndSub, new Vector2(endPanelRect.X + 24, endPanelRect.Y + 58), Color.White);

            var restartBtnRect = new Rectangle(endPanelRect.X + 24, endPanelRect.Y + 148, endPanelRect.Width - 48, 48);
            bool hoverRestart = restartBtnRect.Contains(_prevMouse.Position);
            DrawPanel(restartBtnRect, hoverRestart ? new Color(37, 99, 235) : new Color(29, 78, 216), new Color(147, 197, 253), 2);
            DrawPixelFolder(restartBtnRect.X + 16, restartBtnRect.Y + 16, new Color(254, 240, 138));
            string rText = "▶ [F5] ARQUIVAR & ASSUMIR PRÓXIMO INQUÉRITO POLICIAL";
            _spriteBatch.DrawString(fontTitle, rText, new Vector2(restartBtnRect.X + 40, restartBtnRect.Y + 12), Color.White);

            _dialogueRestartRect = restartBtnRect;
        }
        else
        {
            if (_selectedClue != null)
            {
                string headerLeft = _isWaitingForLlm
                    ? "O silêncio pesa na sala... O suspeito mede cada palavra..."
                    : "INTERROGAR SUSPEITO [1-4]:";
                _spriteBatch.DrawString(fontSmall, headerLeft, new Vector2(rightX + 4, optionsAreaY + 1), new Color(165, 185, 225));
                var headerLeftSize = fontSmall.MeasureString(headerLeft);

                string clearText = "[✕ DESANEXAR]";
                var clearBtnSize = fontSmall.MeasureString(clearText);
                _clearClueButtonRect = new Rectangle(rightX + rightWidth - (int)clearBtnSize.X - 16, optionsAreaY - 2, (int)clearBtnSize.X + 14, 22);
                bool hoverClear = _clearClueButtonRect.Contains(_prevMouse.Position);
                DrawPanel(_clearClueButtonRect, hoverClear ? new Color(185, 28, 28) : new Color(127, 29, 29), new Color(248, 113, 113), 1);
                _spriteBatch.DrawString(fontSmall, clearText, new Vector2(_clearClueButtonRect.X + 7, _clearClueButtonRect.Y + 2), Color.White);

                string clueBanner = $"📂 PROVA ANEXADA: {_selectedClue.Title}";
                int bannerStartX = rightX + (int)headerLeftSize.X + 16;
                int maxBannerW = _clearClueButtonRect.Left - bannerStartX - 16;
                if (maxBannerW > 60)
                {
                    _spriteBatch.DrawString(fontSmall, TruncateText(fontSmall, clueBanner, maxBannerW),
                        new Vector2(bannerStartX, optionsAreaY + 1), new Color(250, 204, 21));
                }
            }
            else
            {
                _clearClueButtonRect = Rectangle.Empty;
                string headerText = _isWaitingForLlm
                    ? "O silêncio pesa na sala... O suspeito mede cada palavra..."
                    : "INTERROGAR SUSPEITO [Escolha uma linha de inquirição ou pressione 1, 2, 3, 4]:";
                Color headerColor = _isWaitingForLlm ? new Color(250, 204, 21) : new Color(165, 185, 225);
                _spriteBatch.DrawString(fontSmall, headerText, new Vector2(rightX + 4, optionsAreaY + 1), headerColor);
            }

            int startCardY = optionsAreaY + 24;
            int cardHeight = 47;
            int cardGap = 5;

            for (int i = 0; i < activeOptions.Count; i++)
            {
                var opt = activeOptions[i];
                var cardRect = new Rectangle(rightX, startCardY + i * (cardHeight + cardGap), rightWidth, cardHeight);
                bool canClick = !_isWaitingForLlm && !_isGeneratingCase;
                bool hover = cardRect.Contains(_prevMouse.Position) && canClick;

                Color tagColor;
                Color cardBorder;
                Color cardBg;

                switch (opt.Category)
                {
                    case "Empatia":
                        tagColor = new Color(52, 211, 153);
                        cardBorder = hover ? new Color(52, 211, 153) : new Color(30, 70, 50);
                        cardBg = hover ? new Color(22, 45, 35) : new Color(16, 28, 22);
                        break;
                    case "Pressao":
                        tagColor = new Color(248, 113, 113);
                        cardBorder = hover ? new Color(248, 113, 113) : new Color(80, 35, 40);
                        cardBg = hover ? new Color(45, 22, 25) : new Color(28, 16, 18);
                        break;
                    case "Revelacao":
                    case "Final":
                        tagColor = new Color(250, 204, 21);
                        cardBorder = hover ? new Color(250, 204, 21) : new Color(85, 70, 25);
                        cardBg = hover ? new Color(46, 40, 18) : new Color(30, 26, 14);
                        break;
                    default:
                        tagColor = new Color(96, 165, 250);
                        cardBorder = hover ? new Color(96, 165, 250) : new Color(40, 60, 95);
                        cardBg = hover ? new Color(26, 40, 65) : new Color(18, 26, 42);
                        break;
                }

                DrawPanel(cardRect, cardBg, cardBorder, hover ? 2 : 1);

                // Badge do Teclado + Ícone Temático + Categoria
                string categoryIcon = opt.Category switch
                {
                    "Empatia" => "🤝",
                    "Pressao" => "⚡",
                    "Revelacao" or "Final" => "⚖️",
                    _ => "🔍"
                };
                string badge = $"[{i + 1}] {categoryIcon} {opt.Tag}";
                var badgeSize = fontSmall.MeasureString(badge);
                var badgeRect = new Rectangle(cardRect.X + 8, cardRect.Y + 9, (int)badgeSize.X + 16, 28);
                DrawPanel(badgeRect, tagColor * 0.18f, tagColor * 0.7f, 1);
                _spriteBatch.DrawString(fontSmall, badge, new Vector2(badgeRect.X + 8, badgeRect.Y + 4), tagColor);

                // Badge de Sinergia da Pista Anexada (se houver)
                int maxTextRight = cardRect.Right - 16;
                if (_selectedClue != null)
                {
                    bool isCompatible = opt.IsClueCompatible(_selectedClue);
                    string synBadge = isCompatible ? "★ CONTUNDENTE (+)" : "⚠ DESCONEXA (-)";
                    Color synColor = isCompatible ? new Color(52, 211, 153) : new Color(248, 113, 113);
                    Color synBg = isCompatible ? new Color(20, 50, 35) : new Color(50, 20, 25);

                    var synSize = fontSmall.MeasureString(synBadge);
                    var synRect = new Rectangle(cardRect.Right - (int)synSize.X - 16, cardRect.Y + 9, (int)synSize.X + 12, 28);
                    DrawPanel(synRect, synBg, synColor, 1);
                    _spriteBatch.DrawString(fontSmall, synBadge, new Vector2(synRect.X + 6, synRect.Y + 4), synColor);

                    maxTextRight = synRect.Left - 12;
                }

                // Fala do Detetive com dimensionamento adaptativo para nunca cortar
                string dialogueText = $"\"{opt.PlayerDialogue}\"";
                int textStartX = badgeRect.Right + 12;
                int maxTextWidth = maxTextRight - textStartX;

                var fontOpt = fontSmall;
                string wrappedOpt = WrapText(fontOpt, dialogueText, maxTextWidth);
                var optTextSize = fontOpt.MeasureString(wrappedOpt);

                if (optTextSize.Y > cardRect.Height - 8)
                {
                    fontOpt = _fontSystem.GetFont(13);
                    wrappedOpt = WrapText(fontOpt, dialogueText, maxTextWidth);
                    optTextSize = fontOpt.MeasureString(wrappedOpt);
                }

                float textY = cardRect.Y + Math.Max(2, (cardRect.Height - optTextSize.Y) / 2f);

                _spriteBatch.DrawString(fontOpt, wrappedOpt, new Vector2(textStartX, textY),
                    hover ? Color.White : new Color(225, 235, 250));

                _dialogueOptionRects.Add((cardRect, opt));
            }

            // Barra de Status Inferior (Alinhada perfeitamente na base)
            _spriteBatch.DrawString(fontSmall, _statusBanner, new Vector2(rightX + 4, 788), new Color(155, 170, 195));
        }
    }

    private void DrawTurnTimelineBar(Rectangle rect, DynamicSpriteFont fontTitle, DynamicSpriteFont fontSmall)
    {
        DrawPanel(rect, new Color(21, 27, 40), new Color(52, 68, 98), 1);

        string mainTurnLabel = _state.Outcome == GameOutcome.EmAndamento
            ? $"REGISTRO CRONOLÓGICO: TURNO {_state.CurrentTurn:00} / {_state.MaxTurns:00}"
            : ((_state.Outcome is GameOutcome.VitoriaConfissao or GameOutcome.VitoriaInocencia) ? "INQUÉRITO CONCLUÍDO COM SUCESSO!" : "INTERROGATÓRIO ENCERRADO");

        Color turnLabelColor = _state.CurrentTurn >= _state.MaxTurns - 2
            ? new Color(251, 146, 60)
            : new Color(125, 211, 252);

        _spriteBatch.DrawString(fontTitle, mainTurnLabel, new Vector2(rect.X + 16, rect.Y + 8), turnLabelColor);

        int remaining = Math.Max(0, _state.MaxTurns - _state.CurrentTurn + 1);
        string subStatus = _isWaitingForLlm
            ? "O silêncio pesa na sala... O suspeito mede cada palavra..."
            : (_state.Outcome == GameOutcome.EmAndamento
                ? $"SUA VEZ DE AGIR • Restam {remaining} turno(s) até a chegada do advogado de defesa"
                : "Pressione [F5] no topo para assumir um novo inquérito policial");
        _spriteBatch.DrawString(fontSmall, subStatus, new Vector2(rect.X + 16, rect.Y + 35), new Color(190, 205, 230));

        int pillWidth = 36;
        int pillHeight = 36;
        int spacing = 6;
        int startX = rect.Right - (10 * (pillWidth + spacing)) - 12;
        int pillY = rect.Y + 14;

        for (int t = 1; t <= _state.MaxTurns; t++)
        {
            var pillRect = new Rectangle(startX + (t - 1) * (pillWidth + spacing), pillY, pillWidth, pillHeight);
            bool isPast = t < _state.CurrentTurn || (_state.Outcome != GameOutcome.EmAndamento && t <= _state.CurrentTurn);
            bool isCurrent = t == _state.CurrentTurn && _state.Outcome == GameOutcome.EmAndamento;

            Color bg;
            Color border;
            Color numColor;

            if (isCurrent)
            {
                bg = new Color(37, 99, 235);
                border = new Color(191, 219, 254);
                numColor = Color.White;
            }
            else if (isPast)
            {
                bg = new Color(24, 52, 48);
                border = new Color(52, 211, 153);
                numColor = new Color(110, 231, 183);
            }
            else
            {
                bg = new Color(15, 19, 28);
                border = t >= _state.MaxTurns - 1 ? new Color(120, 50, 60) : new Color(42, 52, 72);
                numColor = new Color(110, 125, 150);
            }

            DrawPanel(pillRect, bg, border, isCurrent ? 2 : 1);
            string numStr = t.ToString();
            var sz = fontSmall.MeasureString(numStr);
            _spriteBatch.DrawString(fontSmall, numStr,
                new Vector2(pillRect.X + (pillRect.Width - sz.X) / 2f, pillRect.Y + 7),
                numColor);
        }
    }

    private int MeasureTurnBlockHeight(
        TurnRecord rec,
        string wrappedPlayer,
        string wrappedGm,
        string wrappedNpc,
        DynamicSpriteFont fontBody,
        DynamicSpriteFont fontNpc,
        int width)
    {
        int h = 38;

        if (!string.IsNullOrEmpty(wrappedPlayer))
        {
            int pH = (int)fontBody.MeasureString(wrappedPlayer).Y + 40;
            h += pH + 12;
        }

        if (!string.IsNullOrEmpty(wrappedGm))
        {
            int gmH = (int)fontBody.MeasureString(wrappedGm).Y + 32;
            h += gmH + 10;
        }

        int npcTextH = (int)fontNpc.MeasureString(wrappedNpc).Y;
        bool hasPills = !rec.IsOpeningScene;
        bool hasTwoPillRows = hasPills && (rec.ClueSynergy != ClueSynergyType.None || !string.IsNullOrEmpty(rec.UnlockedClueTitle));
        int npcBoxH = npcTextH + (hasTwoPillRows ? 114 : (hasPills ? 84 : 48));
        h += npcBoxH;

        if (!string.IsNullOrEmpty(rec.SystemConclusionTitle))
        {
            string wrappedConclusion = WrapText(fontBody, rec.SystemConclusionText ?? "", width - 32);
            int cH = (int)fontBody.MeasureString(wrappedConclusion).Y + 50;
            h += 14 + cH;
        }

        return h;
    }

    private void DrawTurnBlock(
        Rectangle rect,
        TurnRecord rec,
        bool isLatest,
        string wrappedPlayer,
        string wrappedGm,
        string wrappedNpc,
        DynamicSpriteFont fontSmall,
        DynamicSpriteFont fontBody,
        DynamicSpriteFont fontNpc)
    {
        float opacity = isLatest ? 1.0f : 0.74f;
        int curY = rect.Y;

        string headerLabel = rec.IsOpeningScene
            ? "ABERTURA DO INTERROGATÓRIO • SITUAÇÃO INICIAL"
            : (isLatest ? $"★ TURNO {rec.TurnNumber} DE {_state.MaxTurns} (RESPOSTA MAIS RECENTE)" : $"TURNO {rec.TurnNumber} DE {_state.MaxTurns} (CONCLUÍDO)");

        Color headerColor = isLatest ? new Color(250, 204, 21) : new Color(130, 148, 180);
        DrawFilledRect(new Rectangle(rect.X, curY + 14, rect.Width, 1), new Color(42, 54, 78) * opacity);
        var labelSize = fontSmall.MeasureString(headerLabel);
        int labelBgWidth = (int)labelSize.X + 24;
        DrawPanel(new Rectangle(rect.X + 12, curY, labelBgWidth, 28),
            isLatest ? new Color(42, 36, 20) : new Color(22, 28, 40),
            headerColor * opacity, 1);
        _spriteBatch.DrawString(fontSmall, headerLabel, new Vector2(rect.X + 24, curY + 4), headerColor * opacity);
        curY += 38;

        if (!string.IsNullOrEmpty(wrappedPlayer))
        {
            int bubbleWidth = (int)(rect.Width * 0.78f);
            int pTextH = (int)fontBody.MeasureString(wrappedPlayer).Y;
            int bubbleHeight = pTextH + 40;
            var playerBubble = new Rectangle(rect.Right - bubbleWidth, curY, bubbleWidth, bubbleHeight);

            DrawPanel(playerBubble, new Color(22, 38, 68) * opacity, new Color(59, 130, 246) * opacity, 1);
            _spriteBatch.DrawString(fontSmall, "🎙️ INVESTIGADOR (VOCÊ):",
                new Vector2(playerBubble.X + 16, playerBubble.Y + 7), new Color(147, 197, 253) * opacity);
            _spriteBatch.DrawString(fontBody, wrappedPlayer,
                new Vector2(playerBubble.X + 16, playerBubble.Y + 30), new Color(240, 245, 255) * opacity);

            curY += bubbleHeight + 12;
        }

        if (!string.IsNullOrEmpty(wrappedGm))
        {
            int gmTextH = (int)fontBody.MeasureString(wrappedGm).Y;
            int gmBoxH = gmTextH + 32;
            var gmRect = new Rectangle(rect.X + 8, curY, rect.Width - 16, gmBoxH);

            DrawFilledRect(new Rectangle(gmRect.X, gmRect.Y + 2, 4, gmRect.Height - 4), new Color(167, 139, 250) * opacity);
            _spriteBatch.DrawString(fontSmall, "👁️ OBSERVAÇÃO FORENSE & CONDUTA CORPORAL:",
                new Vector2(gmRect.X + 14, gmRect.Y + 2), new Color(196, 181, 253) * opacity);
            _spriteBatch.DrawString(fontBody, wrappedGm,
                new Vector2(gmRect.X + 14, gmRect.Y + 25), new Color(220, 215, 245) * opacity);

            curY += gmBoxH + 10;
        }

        Color emotionColor = GetEmotionColor(rec.NpcEmotion);
        int npcTextH = (int)fontNpc.MeasureString(wrappedNpc).Y;
        bool hasPills = !rec.IsOpeningScene;
        bool hasTwoPillRows = hasPills && (rec.ClueSynergy != ClueSynergyType.None || !string.IsNullOrEmpty(rec.UnlockedClueTitle));
        int npcBoxH = npcTextH + (hasTwoPillRows ? 114 : (hasPills ? 84 : 48));
        var npcCard = new Rectangle(rect.X, curY, rect.Width, npcBoxH);

        Color npcBg = isLatest ? new Color(26, 33, 48) : new Color(21, 26, 38);
        DrawPanel(npcCard, npcBg * opacity, emotionColor * (isLatest ? 0.95f : 0.45f), isLatest ? 2 : 1);
        DrawFilledRect(new Rectangle(npcCard.X, npcCard.Y, 6, npcCard.Height), emotionColor * opacity);

        string npcHeader = $"🗣️ DEPOIMENTO DE {_state.NpcName.ToUpperInvariant()}:";
        _spriteBatch.DrawString(fontSmall, npcHeader, new Vector2(npcCard.X + 18, npcCard.Y + 9), Color.White * opacity);

        string emotionBadge = $"POSTURA: {rec.NpcEmotion.ToString().ToUpperInvariant()}";
        var emoSize = fontSmall.MeasureString(emotionBadge);
        var emoRect = new Rectangle(npcCard.Right - (int)emoSize.X - 30, npcCard.Y + 7, (int)emoSize.X + 18, 24);
        DrawPanel(emoRect, emotionColor * 0.2f * opacity, emotionColor * opacity, 1);
        _spriteBatch.DrawString(fontSmall, emotionBadge, new Vector2(emoRect.X + 9, emoRect.Y + 2), emotionColor * opacity);

        _spriteBatch.DrawString(fontNpc, wrappedNpc,
            new Vector2(npcCard.X + 18, npcCard.Y + 36), new Color(248, 250, 255) * opacity);

        if (hasPills)
        {
            int pillStartX = npcCard.X + 18;
            int maxPillRight = npcCard.Right - 18;
            int pillY = hasTwoPillRows ? npcCard.Bottom - 64 : npcCard.Bottom - 34;
            int curPillX = pillStartX;

            // Linha 1: Métricas de Confiança, Pressão e Progresso
            curPillX = DrawImpactPill(curPillX, pillY, $"Confiança {FormatDelta(rec.TrustDelta)}%",
                rec.TrustDelta >= 0 ? new Color(56, 189, 248) : new Color(244, 63, 94), fontSmall, opacity);

            curPillX = DrawImpactPill(curPillX, pillY, $"Pressão {FormatDelta(rec.StressDelta)}%",
                rec.StressDelta > 15 ? new Color(244, 63, 94) : new Color(245, 158, 11), fontSmall, opacity);

            curPillX = DrawImpactPill(curPillX, pillY, $"Progresso {FormatDelta(rec.ProgressDelta)}%",
                new Color(52, 211, 153), fontSmall, opacity);

            // Linha 2: Sinergia de Pistas e Novas Evidências Forenses
            if (hasTwoPillRows)
            {
                pillY += 30;
                curPillX = pillStartX;

                if (rec.ClueSynergy == ClueSynergyType.Positive)
                {
                    string synText = $"★ Prova Contundente: {rec.AttachedClueTitle}";
                    curPillX = DrawImpactPill(curPillX, pillY, synText,
                        new Color(52, 211, 153), fontSmall, opacity);
                }
                else if (rec.ClueSynergy == ClueSynergyType.Negative)
                {
                    string synText = $"⚠ Evidência Desconexa: {rec.AttachedClueTitle}";
                    curPillX = DrawImpactPill(curPillX, pillY, synText,
                        new Color(248, 113, 113), fontSmall, opacity);
                }

                if (!string.IsNullOrEmpty(rec.UnlockedClueTitle))
                {
                    string unlockText = $"★ Nova Evidência: {rec.UnlockedClueTitle}";
                    int remainW = maxPillRight - curPillX;
                    if (remainW > 80)
                    {
                        var textSz = fontSmall.MeasureString(unlockText);
                        if (textSz.X + 24 > remainW)
                        {
                            unlockText = TruncateText(fontSmall, unlockText, remainW - 24);
                        }
                        DrawImpactPill(curPillX, pillY, unlockText,
                            new Color(250, 204, 21), fontSmall, opacity);
                    }
                }
            }
        }

        curY += npcBoxH;

        if (!string.IsNullOrEmpty(rec.SystemConclusionTitle))
        {
            curY += 14;
            string wrappedConclusion = WrapText(fontBody, rec.SystemConclusionText ?? "", rect.Width - 32);
            int cH = (int)fontBody.MeasureString(wrappedConclusion).Y + 50;
            var endRect = new Rectangle(rect.X, curY, rect.Width, cH);
            Color endColor = rec.IsVictoryConclusion ? new Color(52, 211, 153) : new Color(244, 63, 94);
            DrawPanel(endRect, endColor * 0.16f, endColor, 2);
            _spriteBatch.DrawString(fontSmall, rec.SystemConclusionTitle!, new Vector2(endRect.X + 16, endRect.Y + 9), endColor);
            _spriteBatch.DrawString(fontBody, wrappedConclusion, new Vector2(endRect.X + 16, endRect.Y + 34), Color.White);
        }
    }

    private int DrawImpactPill(int x, int y, string text, Color color, DynamicSpriteFont font, float opacity)
    {
        var size = font.MeasureString(text);
        var pillRect = new Rectangle(x, y, (int)size.X + 18, 25);
        DrawPanel(pillRect, color * 0.18f * opacity, color * 0.85f * opacity, 1);
        _spriteBatch.DrawString(font, text, new Vector2(pillRect.X + 9, pillRect.Y + 2), color * opacity);
        return pillRect.Right + 10;
    }

    private void DrawPendingTurnBlock(
        Rectangle rect,
        int turnNumber,
        string wrappedPlayer,
        DynamicSpriteFont fontSmall,
        DynamicSpriteFont fontBody,
        GameTime gameTime)
    {
        int curY = rect.Y;
        string headerLabel = $"★ TURNO {turnNumber} DE {_state.MaxTurns} (EM ANDAMENTO...)";
        DrawPanel(new Rectangle(rect.X + 12, curY, 320, 28), new Color(42, 36, 20), new Color(250, 204, 21), 1);
        _spriteBatch.DrawString(fontSmall, headerLabel, new Vector2(rect.X + 24, curY + 4), new Color(250, 204, 21));
        curY += 38;

        int bubbleWidth = (int)(rect.Width * 0.78f);
        int pTextH = (int)fontBody.MeasureString(wrappedPlayer).Y;
        int bubbleHeight = pTextH + 40;
        var playerBubble = new Rectangle(rect.Right - bubbleWidth, curY, bubbleWidth, bubbleHeight);
        DrawPanel(playerBubble, new Color(22, 38, 68), new Color(59, 130, 246), 1);
        _spriteBatch.DrawString(fontSmall, "🎙️ INVESTIGADOR (VOCÊ):",
            new Vector2(playerBubble.X + 16, playerBubble.Y + 7), new Color(147, 197, 253));
        _spriteBatch.DrawString(fontBody, wrappedPlayer,
            new Vector2(playerBubble.X + 16, playerBubble.Y + 30), Color.White);

        curY += bubbleHeight + 14;

        int dots = ((int)(gameTime.TotalGameTime.TotalSeconds * 3) % 3) + 1;
        string dotsStr = new('.', dots);
        var waitRect = new Rectangle(rect.X, curY, rect.Width, 60);
        DrawPanel(waitRect, new Color(24, 30, 44), new Color(139, 92, 246), 1);
        string waitMsg = WrapText(fontBody, $"O silêncio pesa na sala... {_state.NpcName} mede as palavras sob a luz fria{dotsStr}", waitRect.Width - 36);
        _spriteBatch.DrawString(fontBody, waitMsg,
            new Vector2(waitRect.X + 18, waitRect.Y + 18), new Color(196, 181, 253));
    }

    /// <summary>
    /// Desenha o retrato procedural adaptando pele, cabelo, roupa, óculos e barba gerados pelo LLM!
    /// </summary>
    private void DrawNpcPortrait(Rectangle rect, GameTime gameTime)
    {
        Color auraColor = GetEmotionColor(_state.Emotion);
        DrawPanel(rect, new Color(14, 18, 26), auraColor, 2);

        for (int y = rect.Y + 12; y < rect.Bottom - 12; y += 18)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X + 6, y, rect.Width - 12, 4), new Color(24, 31, 46));
        }

        float breath = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2.5) * 2f;
        int cx = rect.Center.X;
        int cy = rect.Center.Y + (int)breath;

        var app = _state.Appearance;

        Color skinColor = (app.SkinTone?.ToLowerInvariant()) switch
        {
            "clara" => new Color(235, 198, 172),
            "morena" => new Color(196, 145, 110),
            "escura" => new Color(128, 86, 62),
            _ => new Color(218, 175, 145)
        };

        Color hairColor = (app.HairColor?.ToLowerInvariant()) switch
        {
            "preto" => new Color(32, 34, 40),
            "castanho" => new Color(92, 60, 42),
            "loiro" => new Color(218, 186, 98),
            "ruivo" => new Color(168, 68, 42),
            _ => new Color(105, 112, 126) // grisalho
        };

        Color outfitColor = (app.OutfitStyle?.ToLowerInvariant()) switch
        {
            "terno" => new Color(30, 34, 44),
            "jaqueta" => new Color(68, 46, 36),
            "uniforme" => new Color(32, 64, 108),
            "vinho" => new Color(98, 28, 44),
            _ => new Color(52, 66, 88) // jaleco/padrão
        };

        // Ombros / Roupa
        DrawFilledRect(new Rectangle(cx - 50, rect.Bottom - 50, 100, 48), outfitColor);
        DrawFilledRect(new Rectangle(cx - 18, rect.Bottom - 50, 36, 48), new Color(215, 222, 235));
        DrawFilledRect(new Rectangle(cx - 5, rect.Bottom - 44, 10, 40), new Color(155, 42, 52));

        // Pescoço e Cabeça
        DrawFilledRect(new Rectangle(cx - 12, cy + 16, 24, 18), skinColor * 0.92f);
        DrawFilledRect(new Rectangle(cx - 30, cy - 34, 60, 54), skinColor);

        // Estilo de Cabelo
        string style = app.HairStyle?.ToLowerInvariant() ?? "curto";
        if (style != "calvo")
        {
            int topHeight = style == "topete" ? 20 : 14;
            int topY = style == "topete" ? cy - 48 : cy - 42;
            DrawFilledRect(new Rectangle(cx - 32, topY, 64, topHeight), hairColor);
            DrawFilledRect(new Rectangle(cx - 32, cy - 34, 8, style == "longo" ? 48 : 24), hairColor);
            DrawFilledRect(new Rectangle(cx + 24, cy - 34, 8, style == "longo" ? 48 : 24), hairColor);
        }

        // Barba opcional
        if (app.HasBeard)
        {
            DrawFilledRect(new Rectangle(cx - 26, cy + 4, 52, 16), hairColor * 0.85f);
        }

        // Olhos ou Óculos
        if (app.HasGlasses)
        {
            Color lensColor = _state.Emotion == EmotionalState.Hostil
                ? new Color(255, 120, 120, 210)
                : new Color(165, 220, 255, 200);
            DrawPanel(new Rectangle(cx - 24, cy - 16, 20, 13), lensColor, new Color(40, 45, 55), 2);
            DrawPanel(new Rectangle(cx + 4, cy - 16, 20, 13), lensColor, new Color(40, 45, 55), 2);
            DrawFilledRect(new Rectangle(cx - 4, cy - 12, 8, 2), new Color(40, 45, 55));
        }
        else
        {
            DrawFilledRect(new Rectangle(cx - 18, cy - 12, 8, 6), new Color(30, 35, 45));
            DrawFilledRect(new Rectangle(cx + 10, cy - 12, 8, 6), new Color(30, 35, 45));
        }

        // Boca
        int mouthWidth = _state.Emotion == EmotionalState.Nervoso ? 14 : 20;
        int mouthY = _state.Emotion == EmotionalState.Cooperativo ? cy + 8 : cy + 10;
        DrawFilledRect(new Rectangle(cx - mouthWidth / 2, mouthY, mouthWidth, 3), new Color(110, 60, 58));
    }

    private void DrawHeartbeatWave(Rectangle rect, GameTime gameTime, int bpm)
    {
        DrawPanel(rect, new Color(12, 16, 22), new Color(36, 48, 66), 1);
        float speed = bpm / 60f * 4.5f;
        float t = (float)gameTime.TotalGameTime.TotalSeconds * speed;
        Color waveColor = bpm > 115 ? new Color(244, 63, 94) : new Color(52, 211, 153);

        for (int x = 4; x < rect.Width - 4; x += 3)
        {
            float phase = (x * 0.08f) - t;
            float pulse = (float)Math.Sin(phase) * (float)Math.Pow(Math.Max(0, Math.Cos(phase * 0.5f)), 6);
            int y = rect.Center.Y - (int)(pulse * (rect.Height * 0.42f));
            DrawFilledRect(new Rectangle(rect.X + x, y, 2, 2), waveColor);
        }
    }

    private void DrawMeterBar(Rectangle rect, string label, string valueText, float ratio, Color barColor, string tooltip)
    {
        var fontLabel = _fontSystem.GetFont(15);
        var fontTiny = _fontSystem.GetFont(12.5f);

        _spriteBatch.DrawString(fontLabel, label, new Vector2(rect.X, rect.Y), new Color(220, 230, 248));
        var valSize = fontLabel.MeasureString(valueText);
        _spriteBatch.DrawString(fontLabel, valueText, new Vector2(rect.Right - valSize.X, rect.Y), barColor);

        var barBg = new Rectangle(rect.X, rect.Y + 20, rect.Width, 12);
        DrawPanel(barBg, new Color(12, 15, 22), new Color(45, 55, 78), 1);

        int fillWidth = (int)Math.Clamp((barBg.Width - 4) * ratio, 0, barBg.Width - 4);
        if (fillWidth > 0)
        {
            DrawFilledRect(new Rectangle(barBg.X + 2, barBg.Y + 2, fillWidth, barBg.Height - 4), barColor);
        }

        _spriteBatch.DrawString(fontTiny, tooltip, new Vector2(rect.X, rect.Y + 36), new Color(145, 160, 188));
    }

    private static string FormatDelta(int value) => value >= 0 ? $"+{value}" : value.ToString();

    private static Color GetEmotionColor(EmotionalState emotion) => emotion switch
    {
        EmotionalState.Calmo => new Color(56, 189, 248),
        EmotionalState.Cooperativo => new Color(52, 211, 153),
        EmotionalState.Defensivo => new Color(250, 204, 21),
        EmotionalState.Nervoso => new Color(251, 146, 60),
        EmotionalState.Hostil => new Color(244, 63, 94),
        EmotionalState.Acuado => new Color(192, 132, 252),
        _ => Color.LightGray
    };

    /// <summary>
    /// Desenha uma lupa animada fazendo movimento de busca circular sobre linhas de documentos.
    /// </summary>
    private void DrawAnimatedMagnifyingGlass(int centerX, int centerY, int radius, GameTime gameTime)
    {
        float t = (float)gameTime.TotalGameTime.TotalSeconds;

        // Linhas sutis simulando um documento sendo inspecionado pela lupa ao fundo
        int docWidth = radius * 3;
        for (int i = -2; i <= 2; i++)
        {
            int lineY = centerY + i * (radius / 2);
            int w = (i % 2 == 0) ? docWidth : (int)(docWidth * 0.72f);
            DrawFilledRect(new Rectangle(centerX - w / 2, lineY, w, 3), new Color(42, 54, 78));
        }

        // Movimento suave em órbita de busca (figura-8 / circular)
        int lensX = centerX + (int)(MathF.Cos(t * 2.8f) * (radius * 0.55f));
        int lensY = centerY + (int)(MathF.Sin(t * 5.6f) * (radius * 0.25f));

        // Cabo da lupa (inclinado a 45 graus para a direita/baixo)
        int handleStartOffset = (int)(radius * 0.72f);
        int handleLength = (int)(radius * 0.85f);
        DrawThickLine(
            lensX + handleStartOffset,
            lensY + handleStartOffset,
            lensX + handleStartOffset + handleLength,
            lensY + handleStartOffset + handleLength,
            Math.Max(5, radius / 4),
            new Color(250, 204, 21));

        // Vidro interno da lente (azul translúcido)
        DrawFilledCircle(lensX, lensY, radius - 3, new Color(56, 189, 248) * 0.28f);

        // Reflexo diagonal no vidro da lente
        DrawThickLine(
            lensX - (int)(radius * 0.42f),
            lensY - (int)(radius * 0.18f),
            lensX - (int)(radius * 0.12f),
            lensY - (int)(radius * 0.48f),
            Math.Max(2, radius / 9),
            new Color(224, 242, 254) * 0.75f);

        // Aro dourado da lupa
        DrawCircleRing(lensX, lensY, radius, Math.Max(4, radius / 6), new Color(250, 204, 21));
    }

    private void DrawFilledCircle(int cx, int cy, int radius, Color color)
    {
        for (int dy = -radius; dy <= radius; dy++)
        {
            int dx = (int)MathF.Sqrt(radius * radius - dy * dy);
            DrawFilledRect(new Rectangle(cx - dx, cy + dy, dx * 2 + 1, 1), color);
        }
    }

    private void DrawCircleRing(int cx, int cy, int outerRadius, int thickness, Color color)
    {
        int innerRadius = Math.Max(0, outerRadius - thickness);
        for (int dy = -outerRadius; dy <= outerRadius; dy++)
        {
            int outerDx = (int)MathF.Sqrt(outerRadius * outerRadius - dy * dy);
            if (Math.Abs(dy) > innerRadius)
            {
                DrawFilledRect(new Rectangle(cx - outerDx, cy + dy, outerDx * 2 + 1, 1), color);
            }
            else
            {
                int innerDx = (int)MathF.Sqrt(innerRadius * innerRadius - dy * dy);
                int segWidth = Math.Max(1, outerDx - innerDx);
                DrawFilledRect(new Rectangle(cx - outerDx, cy + dy, segWidth, 1), color);
                DrawFilledRect(new Rectangle(cx + innerDx + 1, cy + dy, segWidth, 1), color);
            }
        }
    }

    private void DrawThickLine(int x0, int y0, int x1, int y1, int thickness, Color color)
    {
        int steps = Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0));
        if (steps == 0)
        {
            DrawFilledRect(new Rectangle(x0 - thickness / 2, y0 - thickness / 2, thickness, thickness), color);
            return;
        }

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            int x = (int)MathF.Round(MathHelper.Lerp(x0, x1, t));
            int y = (int)MathF.Round(MathHelper.Lerp(y0, y1, t));
            DrawFilledRect(new Rectangle(x - thickness / 2, y - thickness / 2, thickness, thickness), color);
        }
    }

    private void DrawPanel(Rectangle rect, Color background, Color borderColor, int borderThickness)
    {
        DrawFilledRect(rect, background);
        DrawFilledRect(new Rectangle(rect.X, rect.Y, rect.Width, borderThickness), borderColor);
        DrawFilledRect(new Rectangle(rect.X, rect.Bottom - borderThickness, rect.Width, borderThickness), borderColor);
        DrawFilledRect(new Rectangle(rect.X, rect.Y, borderThickness, rect.Height), borderColor);
        DrawFilledRect(new Rectangle(rect.Right - borderThickness, rect.Y, borderThickness, rect.Height), borderColor);
    }

    private void DrawFilledRect(Rectangle rect, Color color)
    {
        _spriteBatch.Draw(_pixel, rect, color);
    }

    /// <summary>
    /// Distintivo policial retrô de metal dourado com estrela central (14x16 px).
    /// </summary>
    private void DrawPixelBadge(int x, int y, Color color)
    {
        Color goldDark = new Color(160, 115, 15);
        Color goldBright = new Color(254, 240, 138);

        int[] widths = { 10, 12, 14, 14, 14, 14, 14, 12, 10, 8, 6, 4, 2 };
        for (int r = 0; r < widths.Length; r++)
        {
            int w = widths[r];
            int startX = x + (14 - w) / 2;
            int rowY = y + r;
            DrawFilledRect(new Rectangle(startX, rowY, w, 1), goldDark);
            if (w > 2 && r > 0 && r < widths.Length - 1)
            {
                DrawFilledRect(new Rectangle(startX + 1, rowY, w - 2, 1), color);
            }
        }
        DrawFilledRect(new Rectangle(x + 6, y + 3, 2, 6), goldBright);
        DrawFilledRect(new Rectangle(x + 4, y + 5, 6, 2), goldBright);
    }

    /// <summary>
    /// Pasta parda clássica de inquérito criminal com aba e folha interna (16x12 px).
    /// </summary>
    private void DrawPixelFolder(int x, int y, Color color)
    {
        Color tabColor = new Color(234, 179, 8);
        Color darkEdge = new Color(133, 77, 14);

        // Aba no topo esquerdo
        DrawFilledRect(new Rectangle(x, y, 6, 2), tabColor);
        // Folha de relatório interna espreitando
        DrawFilledRect(new Rectangle(x + 7, y + 1, 7, 2), new Color(245, 245, 250));
        // Corpo principal da pasta
        DrawFilledRect(new Rectangle(x, y + 2, 16, 10), color);
        DrawFilledRect(new Rectangle(x, y + 2, 1, 10), darkEdge);
        DrawFilledRect(new Rectangle(x, y + 11, 16, 1), darkEdge);
        DrawFilledRect(new Rectangle(x + 15, y + 2, 1, 10), darkEdge);
    }

    /// <summary>
    /// Ícone procedural de lupa de detetive com aro, reflexo e cabo inclinado (14x14 px).
    /// </summary>
    private void DrawPixelMagnifyingGlass(int x, int y, Color color)
    {
        Color rim = color;
        Color lens = new Color(186, 230, 253);
        Color handle = new Color(202, 138, 4);

        DrawFilledRect(new Rectangle(x + 2, y, 5, 1), rim);
        DrawFilledRect(new Rectangle(x + 1, y + 1, 7, 1), rim);
        DrawFilledRect(new Rectangle(x, y + 2, 9, 5), rim);
        DrawFilledRect(new Rectangle(x + 1, y + 7, 7, 1), rim);
        DrawFilledRect(new Rectangle(x + 2, y + 8, 5, 1), rim);

        DrawFilledRect(new Rectangle(x + 2, y + 2, 5, 5), lens);
        DrawFilledRect(new Rectangle(x + 3, y + 3, 2, 2), Color.White);

        DrawFilledRect(new Rectangle(x + 7, y + 7, 2, 2), handle);
        DrawFilledRect(new Rectangle(x + 9, y + 9, 2, 2), handle);
        DrawFilledRect(new Rectangle(x + 11, y + 11, 2, 2), handle);
    }

    /// <summary>
    /// Gravador de rolo magnetofônico com bobinas giratórias e luz indicadora (22x15 px).
    /// </summary>
    private void DrawPixelTapeRecorder(int x, int y, GameTime gameTime, bool isSpinning)
    {
        DrawFilledRect(new Rectangle(x, y, 22, 15), new Color(26, 32, 42));
        DrawFilledRect(new Rectangle(x, y, 22, 1), new Color(71, 85, 105));
        DrawFilledRect(new Rectangle(x, y + 14, 22, 1), new Color(15, 23, 42));

        float blink = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 5.0);
        Color ledColor = isSpinning
            ? (blink > 0 ? new Color(239, 68, 68) : new Color(127, 29, 29))
            : new Color(34, 197, 94);
        DrawFilledRect(new Rectangle(x + 2, y + 2, 2, 2), ledColor);

        int r1x = x + 7;
        int r2x = x + 16;
        int ry = y + 7;

        DrawFilledCircle(r1x, ry, 3, new Color(85, 55, 35));
        DrawFilledCircle(r2x, ry, 3, new Color(85, 55, 35));

        float angle = isSpinning ? (float)gameTime.TotalGameTime.TotalSeconds * 6f : 0f;
        int s1dx = (int)(MathF.Cos(angle) * 2f);
        int s1dy = (int)(MathF.Sin(angle) * 2f);
        DrawFilledRect(new Rectangle(r1x + s1dx, ry + s1dy, 1, 1), Color.WhiteSmoke);
        DrawFilledRect(new Rectangle(r1x - s1dx, ry - s1dy, 1, 1), Color.WhiteSmoke);

        int s2dx = (int)(MathF.Cos(angle + 1.2f) * 2f);
        int s2dy = (int)(MathF.Sin(angle + 1.2f) * 2f);
        DrawFilledRect(new Rectangle(r2x + s2dx, ry + s2dy, 1, 1), Color.WhiteSmoke);
        DrawFilledRect(new Rectangle(r2x - s2dx, ry - s2dy, 1, 1), Color.WhiteSmoke);

        DrawFilledRect(new Rectangle(r1x, ry, 1, 1), Color.LightGray);
        DrawFilledRect(new Rectangle(r2x, ry, 1, 1), Color.LightGray);

        DrawFilledRect(new Rectangle(r1x, ry + 4, r2x - r1x, 1), new Color(160, 110, 70));
    }

    /// <summary>
    /// Cinzeiro de vidro clássico de delegacia noir com cigarro aceso e fumaça ondulante.
    /// </summary>
    private void DrawPixelCigaretteSmoke(int x, int y, GameTime gameTime)
    {
        DrawFilledRect(new Rectangle(x, y + 8, 22, 5), new Color(60, 75, 95));
        DrawFilledRect(new Rectangle(x + 2, y + 6, 18, 2), new Color(40, 50, 68));
        DrawFilledRect(new Rectangle(x + 1, y + 12, 20, 1), new Color(25, 32, 45));

        DrawFilledRect(new Rectangle(x + 4, y + 5, 12, 2), new Color(245, 245, 240));
        DrawFilledRect(new Rectangle(x + 12, y + 5, 4, 2), new Color(194, 120, 60));

        float emberFlicker = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 9f) * 0.25f + 0.75f;
        Color emberColor = new Color(255, 90 + (int)(emberFlicker * 50), 20);
        DrawFilledRect(new Rectangle(x + 3, y + 5, 2, 2), emberColor);

        float t = (float)gameTime.TotalGameTime.TotalSeconds;
        for (int i = 0; i < 7; i++)
        {
            float progress = ((t * 0.75f + i * 0.16f) % 1.0f);
            int smokeY = (y + 4) - (int)(progress * 42f);
            float drift = MathF.Sin(t * 2.2f + i * 1.1f) * (progress * 14f);
            int smokeX = (x + 3) + (int)drift;

            float alpha = (1.0f - progress) * 0.42f;
            int size = 1 + (int)(progress * 3.5f);
            Color smokeColor = new Color(190, 205, 225) * alpha;
            DrawFilledRect(new Rectangle(smokeX, smokeY, size, size), smokeColor);
        }
    }

    /// <summary>
    /// Cantoneiras pretas triangulares de montagem de foto de fichário criminal noir.
    /// </summary>
    private void DrawCornerPhotoMounts(Rectangle rect, Color mountColor)
    {
        int s = 8;
        for (int i = 0; i < s; i++)
            DrawFilledRect(new Rectangle(rect.X, rect.Y + i, s - i, 1), mountColor);

        for (int i = 0; i < s; i++)
            DrawFilledRect(new Rectangle(rect.Right - (s - i), rect.Y + i, s - i, 1), mountColor);

        for (int i = 0; i < s; i++)
            DrawFilledRect(new Rectangle(rect.X, rect.Bottom - 1 - i, s - i, 1), mountColor);

        for (int i = 0; i < s; i++)
            DrawFilledRect(new Rectangle(rect.Right - (s - i), rect.Bottom - 1 - i, s - i, 1), mountColor);
    }

    /// <summary>
    /// Tela de polígrafo de tubo de fósforo CRT com grade de fundo e feixe osciloscópico.
    /// </summary>
    private void DrawPolygraphOscilloscope(Rectangle rect, GameTime gameTime, int bpm, Color waveColor)
    {
        DrawPanel(rect, new Color(8, 14, 18), new Color(30, 48, 55), 1);

        Color gridColor = new Color(25, 45, 50, 90);
        for (int gx = rect.X + 8; gx < rect.Right - 4; gx += 16)
        {
            for (int gy = rect.Y + 4; gy < rect.Bottom - 4; gy += 8)
            {
                DrawFilledRect(new Rectangle(gx, gy, 1, 1), gridColor);
            }
        }

        float speed = (bpm / 60f) * 3.8f;
        float t = (float)gameTime.TotalGameTime.TotalSeconds * speed;

        int lastY = rect.Center.Y;
        for (int x = 4; x < rect.Width - 4; x += 2)
        {
            float phase = (x * 0.09f) - t;
            float pulse = MathF.Sin(phase) * MathF.Pow(Math.Max(0, MathF.Cos(phase * 0.5f)), 6);
            int y = rect.Center.Y - (int)(pulse * (rect.Height * 0.42f));

            DrawFilledRect(new Rectangle(rect.X + x, y, 2, 2), waveColor);

            if (Math.Abs(y - lastY) > 2)
            {
                int minY = Math.Min(y, lastY);
                int maxY = Math.Max(y, lastY);
                DrawFilledRect(new Rectangle(rect.X + x, minY, 1, maxY - minY), waveColor * 0.7f);
            }
            lastY = y;
        }

        int sweepX = rect.X + 4 + (int)((t * 22f) % (rect.Width - 8));
        DrawFilledRect(new Rectangle(sweepX, rect.Y + 2, 2, rect.Height - 4), new Color(180, 255, 220, 45));
    }

    /// <summary>
    /// Carimbo retro de borracha policial (estilo ARQUIVADO / CONDENADO / INOCENTE).
    /// </summary>
    private void DrawRetroStamp(Rectangle rect, string text, Color color, DynamicSpriteFont font)
    {
        DrawPanel(rect, color * 0.14f, color, 2);
        var innerRect = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width - 6, rect.Height - 6);
        DrawFilledRect(new Rectangle(innerRect.X, innerRect.Y, innerRect.Width, 1), color * 0.5f);
        DrawFilledRect(new Rectangle(innerRect.X, innerRect.Bottom - 1, innerRect.Width, 1), color * 0.5f);
        DrawFilledRect(new Rectangle(innerRect.X, innerRect.Y, 1, innerRect.Height), color * 0.5f);
        DrawFilledRect(new Rectangle(innerRect.Right - 1, innerRect.Y, 1, innerRect.Height), color * 0.5f);

        var sz = font.MeasureString(text);
        _spriteBatch.DrawString(font, text,
            new Vector2(rect.X + (rect.Width - sz.X) / 2f, rect.Y + (rect.Height - sz.Y) / 2f),
            color);
    }

    private static string WrapText(DynamicSpriteFont font, string text, float maxLineWidth)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        string[] paragraphs = text.Replace("\r\n", "\n").Split('\n');
        var resultLines = new List<string>();

        foreach (string paragraph in paragraphs)
        {
            string[] words = paragraph.Split(' ');
            var currentLine = string.Empty;

            foreach (string word in words)
            {
                string testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
                if (font.MeasureString(testLine).X <= maxLineWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentLine))
                        resultLines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                resultLines.Add(currentLine);
        }

        return string.Join("\n", resultLines);
    }

    private static string TruncateText(DynamicSpriteFont font, string text, float maxWidth)
    {
        if (font.MeasureString(text).X <= maxWidth)
            return text;

        string current = text;
        while (current.Length > 3 && font.MeasureString(current + "...").X > maxWidth)
        {
            current = current[..^1];
        }
        return current + "...";
    }
}
