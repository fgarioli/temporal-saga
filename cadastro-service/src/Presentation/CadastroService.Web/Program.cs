using CadastroService.IoC;
using CadastroService.Web.Features.Cadastrar;
using CadastroService.Web.Features.ConsultarCadastro;
using CadastroService.Web.Features.ConsultarUsuario;
using CadastroService.Web.Features.ExcluirUsuario;
using CadastroService.Web.Features.Health;
using CadastroService.Web.Features.SimularFalhas;

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
