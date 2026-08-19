using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace PcMonitorAgent;

public class HeartbeatWorker(
    ILogger<HeartbeatWorker> logger,
    IHttpClientFactory httpClientFactory,
    IOptions<MonitorOptions> options) : BackgroundService
{
    private readonly MonitorOptions _opts = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("HeartbeatWorker iniciado. MachineId={MachineId} Intervalo={Seconds}s",
            _opts.MachineId, _opts.HeartbeatSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await SendHeartbeatAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(_opts.HeartbeatSeconds), stoppingToken)
                .ContinueWith(_ => { }, CancellationToken.None); // no lanzar si se cancela
        }
    }

    private async Task SendHeartbeatAsync(CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient("Monitor");
            var payload = new { machineId = _opts.MachineId };
            var response = await client.PostAsJsonAsync("/api/heartbeat", payload, ct);

            if (response.IsSuccessStatusCode)
                logger.LogDebug("Heartbeat OK [{StatusCode}]", (int)response.StatusCode);
            else
                logger.LogWarning("Heartbeat rechazado por el servidor: {StatusCode}", (int)response.StatusCode);
        }
        catch (OperationCanceledException)
        {
            // Detención limpia — no loguear como error
        }
        catch (Exception ex)
        {
            logger.LogError("Error enviando heartbeat: {Message}", ex.Message);
            // Nunca relanzar — el worker debe continuar
        }
    }
}
