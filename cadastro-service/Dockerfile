# Um build só, duas imagens: --target web (API) e --target worker (worker do Temporal).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/Core/CadastroService.Domain/CadastroService.Domain.csproj src/Core/CadastroService.Domain/
COPY src/Core/CadastroService.Application/CadastroService.Application.csproj src/Core/CadastroService.Application/
COPY src/Infrastructure/CadastroService.Infrastructure/CadastroService.Infrastructure.csproj src/Infrastructure/CadastroService.Infrastructure/
COPY src/Infrastructure/CadastroService.IoC/CadastroService.IoC.csproj src/Infrastructure/CadastroService.IoC/
COPY src/Presentation/CadastroService.Web/CadastroService.Web.csproj src/Presentation/CadastroService.Web/
COPY src/Presentation/CadastroService.Worker/CadastroService.Worker.csproj src/Presentation/CadastroService.Worker/
COPY tests/CadastroService.Domain.Tests/CadastroService.Domain.Tests.csproj tests/CadastroService.Domain.Tests/
COPY tests/CadastroService.Application.Tests/CadastroService.Application.Tests.csproj tests/CadastroService.Application.Tests/
COPY tests/CadastroService.Infrastructure.Tests/CadastroService.Infrastructure.Tests.csproj tests/CadastroService.Infrastructure.Tests/
RUN dotnet restore src/Presentation/CadastroService.Web/CadastroService.Web.csproj \
 && dotnet restore src/Presentation/CadastroService.Worker/CadastroService.Worker.csproj \
 && dotnet restore tests/CadastroService.Domain.Tests/CadastroService.Domain.Tests.csproj \
 && dotnet restore tests/CadastroService.Application.Tests/CadastroService.Application.Tests.csproj \
 && dotnet restore tests/CadastroService.Infrastructure.Tests/CadastroService.Infrastructure.Tests.csproj
COPY . .
RUN dotnet test tests/CadastroService.Domain.Tests/CadastroService.Domain.Tests.csproj --no-restore \
 && dotnet test tests/CadastroService.Application.Tests/CadastroService.Application.Tests.csproj --no-restore \
 && dotnet publish src/Presentation/CadastroService.Web/CadastroService.Web.csproj -c Release -o /app/web --no-restore \
 && dotnet publish src/Presentation/CadastroService.Worker/CadastroService.Worker.csproj -c Release -o /app/worker --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS web
WORKDIR /app
COPY --from=build /app/web .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CadastroService.Web.dll"]

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS worker
WORKDIR /app
COPY --from=build /app/worker .
ENTRYPOINT ["dotnet", "CadastroService.Worker.dll"]
