using CadastroService.Infrastructure.IoC;
using CadastroService.Presentation.Web.Features.Cadastrar;
using CadastroService.Presentation.Web.Features.ConsultarCadastro;
using CadastroService.Presentation.Web.Features.ConsultarUsuario;
using CadastroService.Presentation.Web.Features.ExcluirUsuario;
using CadastroService.Presentation.Web.Features.Health;
using CadastroService.Presentation.Web.Features.SimularFalhas;

// Container "web": a API REST. Ela só registra e consulta os cadastros; quem executa a saga é o container worker.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCadastroWeb(builder.Configuration);

var app = builder.Build();
HealthEndpoint.Map(app);
CadastrarEndpoint.Map(app);
ConsultarCadastroEndpoint.Map(app);
ConsultarUsuarioEndpoint.Map(app);
ExcluirUsuarioEndpoint.Map(app);
if (Simulations.Enabled(builder.Configuration)) SimularFalhasEndpoint.Map(app);
app.Run();
