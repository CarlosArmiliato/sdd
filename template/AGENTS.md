# Contexto do projeto

Este template contém três aplicativos independentes:

- um frontend React;
- um backend .NET 10 empresarial, executado no Azure Kubernetes Service (AKS);
- um firmware .NET 10 para Linux ARM64 embarcado.

O backend é um mini-monólito modular com hosts independentes para HTTP e processamento em segundo plano. O firmware usa um único host para REST, SignalR e workers, reduzindo o consumo de recursos no dispositivo.

Consulte [`backend/ARCHITECTURE.md`](backend/ARCHITECTURE.md) para a arquitetura do backend e [`firmware/README.md`](firmware/README.md) para a arquitetura embarcada.

## Padrões e regras

- Consulte [`.agents/rules/code-standards.md`](.agents/rules/code-standards.md) para padrões gerais de código.
- Consulte [`.agents/rules/dotnet.md`](.agents/rules/dotnet.md) antes de alterar aplicações ou workers .NET.
- Consulte [`.agents/rules/folder-structure.md`](.agents/rules/folder-structure.md) para a organização obrigatória de projetos, código e testes.
- Consulte [`.agents/rules/tests.md`](.agents/rules/tests.md) para cobertura, pirâmide de testes e ferramentas.
- Consulte [`.agents/rules/javascript-typescript.md`](.agents/rules/javascript-typescript.md) para o frontend.

## Backend

- Tecnologia: .NET 10, ASP.NET Core Controllers, Cortex.Mediator, FluentValidation, Entity Framework Core, PostgreSQL, Redis, Hangfire, Polly, Microsoft Entra ID e Azure Event Hubs.
- Diretório: `backend/`.
- Solução: `Backend.slnx`.
- A API expõe endpoints HTTP por Controllers; Controllers são finos e não contêm regras de negócio.
- `Backend.App` orquestra os casos de uso com Cortex.Mediator; `Backend.Domain` não depende de infraestrutura.
- Processamentos de longa duração, agendados, por polling e por eventos não são executados no host HTTP.

## Firmware

- Papel: aplicação embarcada que integra `can0`, `can1`, `can2`, GPS, LTE, PostgreSQL, Redis e Azure Event Hubs e expõe REST e SignalR ao frontend local.
- Tecnologia: .NET 10, ASP.NET Core, Linux ARM64 e SocketCAN.
- Diretório: `firmware/`.
- Solução: `Firmware.slnx`.
- Arquitetura: host único e modular; API e workers executam no mesmo processo, enquanto domínio, aplicação, contratos e adaptadores permanecem separados.
- Implantação: configuração externa, scripts e unidades systemd ficam em `firmware/deploy/`.
- Regras de negócio: consulte [`firmware/docs/architecture/regras-de-negocio.md`](firmware/docs/architecture/regras-de-negocio.md).

### Linguagem do domínio do firmware

- Conceitos de negócio usam nomes em português, sem acentos nos identificadores C#.
- O intervalo operacional se chama `PeriodoRastreio`; não use `Turno`, `Shift` ou `WorkShift` no código.
- Os estados válidos da máquina são `Efetivo`, `Parada`, `Manobra` e `Deslocamento`.
- Termos técnicos podem permanecer em inglês, inclusive em nomes mistos como `SendPeriodoRastreioHandler`.

## Frontend

- Tecnologia: React 19, TypeScript, Vite, Tailwind CSS e ESLint.
- Diretório: `frontend/`.
- Antes de implementar, corrigir ou revisar frontend, carregue a skill [`react`](.agents/skills/react/SKILL.md).

## Testes, validações e build

Execute os comandos dentro do diretório correspondente:

```bash
# backend
cd backend
dotnet build Backend.slnx
dotnet test Backend.slnx

# firmware
cd firmware
dotnet build Firmware.slnx
dotnet test Firmware.slnx

# frontend
cd frontend
npm run lint
npm run typecheck
npm run build
```

Todo código produzido requer testes automatizados. A cobertura mínima é 80%; consulte as regras de testes para a estratégia por camada.

## Segurança e operação

- Não versione segredos, connection strings, chaves de API ou tokens.
- No AKS, use identidade de workload e configuração externa para recursos Azure.
- No firmware, use variáveis de ambiente ou arquivos externos protegidos e preserve a operação local durante indisponibilidade da LTE.
- Propague correlation IDs e contexto operacional entre APIs, workers e eventos.
- Cada worker deve encerrar graciosamente e concluir ou devolver o trabalho em andamento de forma segura.
