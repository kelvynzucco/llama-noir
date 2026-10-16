using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LlamaNoir.Services;

public enum DownloadPhase
{
    Idle,
    DownloadingLlamafile,
    DownloadingModel,
    Finished,
    Error,
    Cancelled
}

public class AiDownloadProgress
{
    public DownloadPhase Phase { get; set; } = DownloadPhase.Idle;
    public string CurrentFileName { get; set; } = string.Empty;
    public int StepIndex { get; set; } = 1;
    public int TotalSteps { get; set; } = 2;
    public long BytesDownloaded { get; set; }
    public long TotalBytes { get; set; }
    public double SpeedBytesPerSecond { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public double Percentage => TotalBytes > 0
        ? Math.Clamp((double)BytesDownloaded / TotalBytes * 100.0, 0, 100.0)
        : 0.0;
}

public class AiModelDownloader
{
    private const string LlamafileUrl = "https://github.com/Mozilla-Ocho/llamafile/releases/download/0.8.16/llamafile-0.8.16";
    private const string ModelUrl = "https://huggingface.co/mradermacher/Qwen2.5-3B-Instruct-abliterated-GGUF/resolve/main/Qwen2.5-3B-Instruct-abliterated.Q4_K_M.gguf";

    private CancellationTokenSource? _cts;
    private readonly HttpClient _httpClient;

    public bool IsDownloading { get; private set; }
    public AiDownloadProgress CurrentProgress { get; } = new();

    public AiModelDownloader()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10
        };
        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromHours(4) // Permite downloads longos de arquivos de 2GB
        };
    }

    public void Cancel()
    {
        if (IsDownloading)
        {
            _cts?.Cancel();
            CurrentProgress.Phase = DownloadPhase.Cancelled;
            CurrentProgress.StatusMessage = "Download cancelado pelo detetive.";
            IsDownloading = false;
        }
    }

    public async Task<bool> StartDownloadAsync(string targetDirectory, Action<AiDownloadProgress>? onProgress = null)
    {
        if (IsDownloading)
            return false;

        IsDownloading = true;
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            Directory.CreateDirectory(targetDirectory);

            string exeTarget = Path.Combine(targetDirectory, "llamafile.exe");
            string modelTarget = Path.Combine(targetDirectory, "Qwen2.5-3B-Instruct-abliterated.Q4_K_M.gguf");

            // ETAPA 1: Executável Llamafile (se não existir ou for incompleto)
            CurrentProgress.StepIndex = 1;
            CurrentProgress.TotalSteps = 2;
            CurrentProgress.CurrentFileName = "llamafile.exe (Motor da IA)";
            CurrentProgress.Phase = DownloadPhase.DownloadingLlamafile;
            CurrentProgress.StatusMessage = "Baixando Motor de Inferência Mozilla Llamafile...";

            bool needsExe = !File.Exists(exeTarget) || new FileInfo(exeTarget).Length < 100_000_000;
            if (needsExe)
            {
                await DownloadFileWithProgressAsync(LlamafileUrl, exeTarget, onProgress, token);
            }
            else
            {
                CurrentProgress.StatusMessage = "Motor Llamafile já instalado!";
                onProgress?.Invoke(CurrentProgress);
            }

            // ETAPA 2: Modelo GGUF Qwen 2.5 3B (se não existir ou for incompleto)
            CurrentProgress.StepIndex = 2;
            CurrentProgress.TotalSteps = 2;
            CurrentProgress.CurrentFileName = "Qwen2.5-3B-Instruct-abliterated.Q4_K_M.gguf (Cérebro da IA)";
            CurrentProgress.Phase = DownloadPhase.DownloadingModel;
            CurrentProgress.StatusMessage = "Baixando Modelo Qwen 2.5 3B Instruct (~1.9 GB)...";

            bool needsModel = !File.Exists(modelTarget) || new FileInfo(modelTarget).Length < 1_000_000_000;
            if (needsModel)
            {
                await DownloadFileWithProgressAsync(ModelUrl, modelTarget, onProgress, token);
            }
            else
            {
                CurrentProgress.StatusMessage = "Modelo de IA já instalado!";
                onProgress?.Invoke(CurrentProgress);
            }

            CurrentProgress.Phase = DownloadPhase.Finished;
            CurrentProgress.StatusMessage = "Agente de IA instalado e pronto para interrogatório policial!";
            onProgress?.Invoke(CurrentProgress);
            IsDownloading = false;
            return true;
        }
        catch (OperationCanceledException)
        {
            CurrentProgress.Phase = DownloadPhase.Cancelled;
            CurrentProgress.StatusMessage = "Download cancelado.";
            onProgress?.Invoke(CurrentProgress);
            IsDownloading = false;
            return false;
        }
        catch (Exception ex)
        {
            CurrentProgress.Phase = DownloadPhase.Error;
            CurrentProgress.ErrorMessage = $"Falha no download: {ex.Message}";
            CurrentProgress.StatusMessage = "Erro ao baixar os arquivos da IA.";
            onProgress?.Invoke(CurrentProgress);
            IsDownloading = false;
            return false;
        }
        finally
        {
            IsDownloading = false;
        }
    }

    private async Task DownloadFileWithProgressAsync(
        string url,
        string destinationFilePath,
        Action<AiDownloadProgress>? onProgress,
        CancellationToken cancellationToken)
    {
        string tempFilePath = destinationFilePath + ".tmp";
        if (File.Exists(tempFilePath))
        {
            try { File.Delete(tempFilePath); } catch { }
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "LlamaNoir-Game/1.0");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        long totalBytes = response.Content.Headers.ContentLength ?? -1L;
        CurrentProgress.TotalBytes = totalBytes;
        CurrentProgress.BytesDownloaded = 0;

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 131072, useAsync: true);

        byte[] buffer = new byte[131072];
        long totalRead = 0;
        int read;

        var stopwatch = Stopwatch.StartNew();
        long lastSpeedCalcBytes = 0;
        long lastSpeedCalcTimeMs = 0;

        while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            totalRead += read;

            long elapsedMs = stopwatch.ElapsedMilliseconds;
            if (elapsedMs - lastSpeedCalcTimeMs >= 400)
            {
                long bytesDiff = totalRead - lastSpeedCalcBytes;
                double secondsDiff = (elapsedMs - lastSpeedCalcTimeMs) / 1000.0;
                if (secondsDiff > 0)
                {
                    CurrentProgress.SpeedBytesPerSecond = bytesDiff / secondsDiff;
                }

                lastSpeedCalcBytes = totalRead;
                lastSpeedCalcTimeMs = elapsedMs;

                CurrentProgress.BytesDownloaded = totalRead;
                onProgress?.Invoke(CurrentProgress);
            }
        }

        fileStream.Close();

        if (File.Exists(destinationFilePath))
        {
            File.Delete(destinationFilePath);
        }
        File.Move(tempFilePath, destinationFilePath);

        CurrentProgress.BytesDownloaded = totalRead;
        CurrentProgress.SpeedBytesPerSecond = 0;
        onProgress?.Invoke(CurrentProgress);
    }
}
