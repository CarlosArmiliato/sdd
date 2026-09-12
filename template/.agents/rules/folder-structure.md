# Estrutura de pastas

O repositório contém três aplicativos independentes: `frontend/`, `backend/` e `firmware/`.

## Backend

O backend é um mini-monólito modular .NET, com hosts separados para API e workers.

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
- `Backend.Contracts`: contratos serializáveis de jobs e eventos de integração, sem lógica de negócio.
- `Backend.Infra`: implementações técnicas compartilhadas que não pertencem a um provedor específico.
- `Backend.Infra.Postgres`: EF Core, mapeamentos, migrations, repositórios e persistência PostgreSQL.
- `Backend.Infra.Redis`: cache distribuído, locks e coordenação baseada em Redis.
- `Backend.Infra.Hangfire`: configuração, persistência e despacho de jobs Hangfire.
- `Backend.Infra.EventHub`: publicação, consumo, serialização, checkpoints e telemetria do Azure Event Hubs.
- `Backend.Infra.Gate.<Fornecedor>`: adapter isolado para cada fornecedor externo.

### Direção das dependências

```text
Backend.Domain       → nenhuma camada
Backend.Contracts    → nenhuma camada
Backend.App          → Backend.Domain, Backend.Contracts
Backend.Infra.*      → Backend.App, Backend.Domain, Backend.Contracts
Hosts                → Backend.App, Backend.Infra.*, Backend.Contracts
```

`Backend.App` define as portas. Projetos `Backend.Infra.*` as implementam. Os hosts realizam a composição de dependências.

Dentro de `Backend.App`, organize Commands, Queries, handlers e validators por funcionalidade. Contratos HTTP pertencem a `Backend.Api/Contracts`; contratos usados entre processos pertencem a `Backend.Contracts`.

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

## Firmware

O firmware é um aplicativo .NET modular com um único host para API, SignalR e workers.

```text
firmware/
├── src/
│   ├── Firmware.Api/
│   ├── Firmware.Application/
│   ├── Firmware.Contracts/
│   ├── Firmware.Domain/
│   ├── Firmware.Infrastructure/
│   └── Firmware.Workers/
├── tests/
│   ├── Firmware.Api.IntegrationTests/
│   ├── Firmware.Application.UnitTests/
│   ├── Firmware.ArchitectureTests/
│   ├── Firmware.Domain.UnitTests/
│   ├── Firmware.Infrastructure.IntegrationTests/
│   └── Firmware.Workers.UnitTests/
├── deploy/
└── docs/
```

- `Firmware.Api`: endpoints REST, hubs SignalR, middleware, configuração e composição do processo.
- `Firmware.Application`: casos de uso e portas para CAN, GPS, persistência, cache, mensageria e realtime.
- `Firmware.Contracts`: DTOs da API, mensagens SignalR e contratos versionados do Event Hubs.
- `Firmware.Domain`: `EstadoMaquina`, `PeriodoRastreio`, paradas, posicionamento e telemetria.
- `Firmware.Infrastructure`: SocketCAN, decodificação PGN/SPN, GPS serial, LTE, PostgreSQL, Redis e Event Hubs.
- `Firmware.Workers`: aquisição contínua, janelas de dois segundos, processamento e sincronização.
- `deploy`: configuração externa, scripts de publicação ARM64 e unidades systemd.
- `docs`: regras de negócio, redes CAN e contratos de eventos.

### Direção das dependências

```text
Firmware.Domain          → nenhuma camada
Firmware.Contracts       → nenhuma camada
Firmware.Application     → Firmware.Domain
Firmware.Infrastructure  → Firmware.Application, Firmware.Domain, Firmware.Contracts
Firmware.Workers         → Firmware.Application
Firmware.Api             → composição das demais camadas
```

O host único é uma decisão operacional do ambiente embarcado; isso não autoriza dependências diretas entre domínio e infraestrutura.

### Fluxo do firmware

```text
can0, can1, can2 → aquisição contínua → JanelaCan de 2 s → decodificação PGN/SPN
GPS serial       → leitura mais recente ───────────────────┘
                                                         ↓
                                                  EstadoMaquina
                                             ┌───────────┼───────────┐
                                             ↓           ↓           ↓
                                        PostgreSQL     Redis      SignalR
                                             ↓
                                           Outbox → Event Hubs

Event Hubs → Inbox → aplicação/domínio → PostgreSQL, Redis e SignalR
```

### Convenções do firmware

- Pastas e identificadores de negócio usam português sem acentos: `EstadosMaquina`, `PeriodosRastreio`, `Paradas`, `Posicionamento` e `Telemetria`.
- Termos técnicos podem permanecer em inglês: `Infrastructure`, `Workers`, `Handler`, `Endpoints`, `EventHubs`, `Outbox` e `Inbox`.
- Use `PeriodoRastreio` para o intervalo operacional; não use `Turno`, `Shift` ou `WorkShift` no código.
- Use somente `Efetivo`, `Parada`, `Manobra` e `Deslocamento` como valores de `EstadoMaquina`.
- Mantenha hardware, banco, cache e mensageria fora de `Firmware.Domain` e `Firmware.Application`.
- Não coloque loops de longa duração em endpoints ou hubs; hospede-os em `Firmware.Workers`.
- Trate PostgreSQL como fonte persistente e Redis como cache ou estado efêmero.
- Isole contratos de Event Hubs dos modelos internos do domínio.
- Use simuladores, interfaces `vcan`, containers ou fixtures controladas nos testes de integração.
