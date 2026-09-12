# Contexto do projeto

Este template contém um frontend e um backend .NET 10 para sistemas empresariais de médio porte, executados no Azure Kubernetes Service (AKS). O backend é um mini-monólito modular: compartilha domínio e código entre os processos, mas disponibiliza hosts independentes para HTTP e processamento em segundo plano.

Consulte [`backend/ARCHITECTURE.md`](backend/ARCHITECTURE.md) para a arquitetura de referência, os limites entre projetos e os fluxos entre API, workers e integrações.

## Padrões e regras

- Consulte [`.agents/rules/code-standards.md`](.agents/rules/code-standards.md) para padrões gerais de código.
- Consulte [`.agents/rules/dotnet.md`](.agents/rules/dotnet.md) antes de alterar o backend .NET ou qualquer worker.
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

# frontend
cd frontend
npm run lint
npm run typecheck
npm run build
```

Todo código produzido requer testes automatizados. A cobertura mínima é 80%; consulte as regras de testes para a estratégia por camada.

## Observações de segurança e operação

- Não versione segredos, connection strings, chaves de API ou tokens.
- Em AKS, use identidade de workload e configuração externa para recursos Azure.
- Propague correlation IDs e contexto de tenant entre API, jobs e eventos.
- Cada worker deve poder encerrar graciosamente e concluir ou devolver o trabalho em andamento de forma segura.
