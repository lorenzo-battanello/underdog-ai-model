using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Underdog.Application.Abstractions;    

namespace Underdog.Application.Services;
public sealed class ModelWarmupService(
    IModelService modelService,
    ILogger<ModelWarmupService> logger) : IHostedService
{
    private readonly IModelService _modelService = modelService;
    private readonly ILogger<ModelWarmupService> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Avvio della fase di warm-up del modello LLM (Underdog)...");
        var timer = Stopwatch.StartNew();

        try
        {
            // Esegue un'inferenza di prova per caricare la KV Cache e allocare la RAM/VRAM
            const string dummyInput = "ping";
            await _modelService.InferRawJsonAsync(dummyInput, cancellationToken);

            timer.Stop();
            _logger.LogInformation(
                "Warm-up completato con successo in {ElapsedMilliseconds} ms. Il servizio è pronto per servire richieste.", 
                timer.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore critico durante la fase di warm-up del modello.");
            // Scegliere se far fallire l'avvio dell'applicazione o proseguire con degradazione
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}