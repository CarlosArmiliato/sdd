# Estrutura de pastas

O repositório contém dois aplicativos independentes: `frontend/` e `backend/`. O backend é um mini-monólito modular .NET, com hosts separados para API e workers.

## Backend

```text
backend/
├── src/
│   ├── Backend.Api/
│   ├── Backend.Worker.Hangfire/
│   ├── Backend.Worker.Polling/
│   ├── Backend.Worker.EventHub/
│   ├── Backend.App/
│   ├── Backend.Domain/
│   ├── Backend.Contracts/
│   ├── Backend.Infra/
│   ├── Backend.Infra.Postgres/
│   ├── Backend.Infra.Redis/
│   ├── Backend.Infra.Hangfire/
│   ├── Backend.Infra.EventHub/
│   ├── Backend.Infra.Gate.EmpresaA/
│   └── Backend.Infra.Gate.EmpresaB/
└── tests/
    ├── Backend.UnitTests/
    ├── Backend.IntegrationTests/
    ├── Backend.ArchitectureTests/
    └── Backend.Worker.*.Tests/
```

### Projetos de host

- `Backend.Api`: Controllers, contratos HTTP, autenticação, autorização, middleware, OpenAPI, health checks e composição da API.
- `Backend.Worker.Hangfire`: servidor Hangfire e handlers exclusivos de jobs em segundo plano.
- `Backend.Worker.Polling`: processos internos que obtêm e processam trabalho pendente.
- `Backend.Worker.EventHub`: consumidores de eventos de integração.

Os hosts não se referenciam entre si. A API nunca referencia um projeto `Backend.Worker.*`.

### Camadas compartilhadas

- `Backend.App`: casos de uso, Commands, Queries, handlers Cortex, validação, behaviors e portas de saída.
- `Backend.Domain`: entidades, agregados, value objects, regras e eventos de domínio. Não referencia outros projetos do backend.
- `Backend.Contracts`: contratos serializáveis de jobs e eventos de integração. Não contém entidades de domínio, lógica de negócio ou dependências de infraestrutura.
- `Backend.Infra`: implementações técnicas compartilhadas que não pertencem a um provedor específico.
- `Backend.Infra.Postgres`: EF Core, mapeamentos, migrations, repositórios e persistência PostgreSQL.
- `Backend.Infra.Redis`: cache distribuído, locks e coordenação baseada em Redis.
- `Backend.Infra.Hangfire`: configuração, persistência e despacho de jobs Hangfire.
- `Backend.Infra.EventHub`: publicação, consumo, serialização, checkpoints e telemetria do Azure Event Hubs.
- `Backend.Infra.Gate.<Fornecedor>`: adapter de saída para um fornecedor; contratos, autenticação, clientes, mappers e políticas de resiliência ficam isolados no projeto do fornecedor.

### Direção das dependências

```text
Backend.Domain       → nenhuma camada
Backend.Contracts    → nenhuma camada
Backend.App          → Backend.Domain, Backend.Contracts
Backend.Infra.*      → Backend.App, Backend.Domain, Backend.Contracts
Hosts                → Backend.App, Backend.Infra.*, Backend.Contracts
```

`Backend.App` define as portas. Projetos `Backend.Infra.*` as implementam. Os hosts realizam a composição de dependências e não se referenciam entre si.

### Organização por funcionalidade

Dentro de `Backend.App`, organize casos de uso por funcionalidade:

```text
Features/
└── Production/
    └── ScheduleExport/
        ├── ScheduleExportCommand.cs
        ├── ScheduleExportHandler.cs
        └── ScheduleExportValidator.cs
```

Contratos HTTP pertencem a `Backend.Api/Contracts`. Contratos usados entre processos pertencem a `Backend.Contracts`.

## Frontend

```text
frontend/
├── public/
├── src/
│   ├── assets/
│   ├── components/
│   ├── hooks/
│   ├── services/
│   ├── types/
│   ├── views/
│   ├── App.tsx
│   ├── main.tsx
│   └── index.css
├── package.json
└── vite.config.ts
```

O acesso ao backend é encapsulado em `services/`; views e componentes não fazem chamadas HTTP diretamente.
