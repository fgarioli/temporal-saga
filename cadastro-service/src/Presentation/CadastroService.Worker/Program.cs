using CadastroService.Infrastructure.IoC;
using CadastroService.Presentation.Worker.Features.ProcessarCadastro;
using CadastroService.Presentation.Worker.Features.ProcessarCadastro.Temporal;

// Container "worker": executa a saga. Aplica as migrations ao subir; depois entrega os cadastros pendentes ao Temporal
// (o dispatcher) e roda o worker do Temporal. É o único projeto que conhece o Temporal.
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddCadastroWorker(builder.Configuration);
builder.Services.AddTemporal(builder.Configuration);
builder.Services.AddHostedService<DispatcherService>();

var host = builder.Build();
await host.Services.ApplyMigrationsAsync(host.Services.GetRequiredService<ILogger<Program>>());
host.Run();
