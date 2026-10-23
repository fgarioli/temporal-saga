SOLUTION := cadastro-service/CadastroService.slnx
WEB      := cadastro-service/src/Presentation/CadastroService.Web/CadastroService.Web.csproj
WORKER   := cadastro-service/src/Presentation/CadastroService.Worker/CadastroService.Worker.csproj

# A 8080 do Mac está ocupada e a 8090 é do container cadastro-web.
WEB_URL ?= http://localhost:5090

# Os projetos são net8.0; permite rodar no runtime mais novo instalado (ex.: .NET 9).
export DOTNET_ROLL_FORWARD ?= Major

.PHONY: help build run run-web run-worker watch watch-web watch-worker infra infra-down

help: ## Lista os alvos
	@grep -E '^[a-z-]+:.*## ' $(MAKEFILE_LIST) | awk -F':.*## ' '{printf "  %-14s %s\n", $$1, $$2}'

build: ## Compila a solution
	dotnet build $(SOLUTION)

run: ## Roda web e worker juntos (requer make infra)
	$(MAKE) -j2 run-web run-worker

run-web: ## Roda a API em http://localhost:5090 (WEB_URL)
	dotnet run --project $(WEB) --urls $(WEB_URL)

run-worker: ## Roda o worker do Temporal (aplica as migrations ao subir)
	dotnet run --project $(WORKER)

watch: ## Roda web e worker com hot reload
	$(MAKE) -j2 watch-web watch-worker

watch-web: ## Roda a API com hot reload em http://localhost:5090 (WEB_URL)
	dotnet watch --project $(WEB) run --urls $(WEB_URL)

watch-worker: ## Roda o worker com hot reload
	dotnet watch --project $(WORKER) run

infra: ## Sobe só as dependências no Docker: Postgres, Temporal, Keycloak, Mailpit
	docker compose up -d postgres temporal temporal-ui keycloak mailpit

infra-down: ## Para as dependências
	docker compose stop postgres temporal temporal-ui keycloak mailpit
