# Regras de testes

Todo código com comportamento deve ser coberto por testes automatizados. A cobertura mínima é 80%, sem substituir cenários críticos por cobertura artificial.

## Backend .NET

Use xUnit e organize os testes por responsabilidade:

- `Backend.UnitTests`: regras de domínio, handlers Cortex, validators e mapeamentos puros; use fakes ou stubs para dependências externas.
- `Backend.IntegrationTests`: Controllers, contratos HTTP, EF Core/PostgreSQL, Redis, Hangfire e composição entre camadas.
- `Backend.ArchitectureTests`: direção das dependências e proibição de referências entre hosts.
- `Backend.Worker.*.Tests`: consumo de jobs ou eventos, idempotência, reprocessamento e desligamento gracioso.

Todo teste deve ser independente, repetível e autocontido. Controle relógio, geração aleatória, dependências externas e dados persistidos. Dê preferência a testes unitários; use testes de integração para verificar os contratos e os adapters efetivamente integrados.

Execute:

```bash
cd backend
dotnet test Backend.slnx
```

## Frontend

Use testes de unidade e de componentes para comportamento visível, estados de erro e acessibilidade. Mantenha testes próximos do código testado. Use Playwright para poucos fluxos E2E críticos, fora de `frontend/` e `backend/`.

## Integrações e processamento assíncrono

Para cada integração externa ou assíncrona, teste ao menos:

- cenário de sucesso;
- erro transitório e retry;
- erro definitivo;
- timeout;
- idempotência;
- serialização e versionamento do contrato;
- propagação de correlation ID.
