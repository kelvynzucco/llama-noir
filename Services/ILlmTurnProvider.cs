using System.Threading;
using System.Threading.Tasks;
using LlamaNoir.Models;

namespace LlamaNoir.Services;

public interface ILlmTurnProvider
{
    string ProviderName { get; }

    Task<LlmGeneratedCase> GenerateNewCaseAsync(
        string? customTheme = null,
        CancellationToken cancellationToken = default);

    Task<LlmTurnOutput> ProcessPlayerTurnAsync(
        InterrogationState state,
        string playerInput,
        CancellationToken cancellationToken = default);
}
