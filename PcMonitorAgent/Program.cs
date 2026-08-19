using Microsoft.Extensions.Options;
using PcMonitorAgent;

var builder = Host.CreateApplicationBuilder(args);

// Windows Service
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "PcMonitorAgent";
});

// Configuración
builder.Services.Configure<MonitorOptions>(
    builder.Configuration.GetSection("Monitor"));

// HttpClient con URL base y API Key
builder.Services.AddHttpClient("Monitor", (sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<MonitorOptions>>().Value;
    client.BaseAddress = new Uri(opts.ApiUrl);
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", opts.ApiKey);
    client.Timeout = TimeSpan.FromSeconds(20);
});

builder.Services.AddHostedService<HeartbeatWorker>();

var host = builder.Build();
host.Run();
