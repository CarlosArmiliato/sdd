# Firmware embarcado

Aplicação .NET 10 para Linux ARM64 responsável pela aquisição das redes `can0`, `can1` e `can2`, leitura do GPS serial, interação com o operador e sincronização por LTE.

## Linguagem do domínio

Conceitos e regras de negócio são escritos em português, sem acentos nos identificadores C#. Termos técnicos consolidados podem permanecer em inglês.

Exemplos de negócio: `PeriodoRastreio`, `EstadoMaquina`, `Efetivo`, `Parada`, `Manobra`, `Deslocamento`, `Velocidade`, `Rpm`, `Marcha`, `Vazao`, `MotivoParada` e `PosicaoGps`.

Exemplos técnicos: `Handler`, `Worker`, `Endpoint`, `Hub`, `Repository`, `SocketCAN`, `EventHub`, `Outbox` e `Inbox`. Identificadores mistos como `SendPeriodoRastreioHandler` e `PeriodoRastreioRepository` são permitidos.

## Decisão estrutural

A solução usa um único executável ASP.NET Core (`Firmware.Api`) para hospedar REST, SignalR e os workers. Isso reduz processos no dispositivo, enquanto assemblies separados preservam os limites arquiteturais.

```text
firmware/
├── src/
│   ├── Firmware.Api/
│   │   ├── Configuration/
│   │   ├── Endpoints/
│   │   ├── Hubs/
│   │   └── Middleware/
│   ├── Firmware.Application/
│   │   ├── Abstractions/
│   │   ├── ComandosRemotos/
│   │   ├── EstadosMaquina/
│   │   ├── Paradas/
│   │   ├── PeriodosRastreio/
│   │   ├── Sincronizacao/
│   │   └── Telemetria/
│   ├── Firmware.Contracts/
│   │   ├── Api/
│   │   ├── EventHubs/
│   │   └── Realtime/
│   ├── Firmware.Domain/
│   │   ├── Can/
│   │   ├── EstadosMaquina/
│   │   ├── Maquinas/
│   │   ├── Paradas/
│   │   ├── PeriodosRastreio/
│   │   ├── Posicionamento/
│   │   ├── Sincronizacao/
│   │   └── Telemetria/
│   ├── Firmware.Infrastructure/
│   │   ├── Caching/Redis/
│   │   ├── Can/SocketCan/
│   │   ├── Connectivity/Lte/
│   │   ├── Messaging/EventHubs/
│   │   ├── Persistence/Postgres/
│   │   ├── Positioning/Gps/
│   │   └── Realtime/SignalR/
│   └── Firmware.Workers/
│       ├── AquisicaoCan/
│       ├── AquisicaoGps/
│       ├── MonitoramentoDispositivo/
│       ├── MonitoramentoEventHub/
│       ├── ProcessamentoTelemetria/
│       └── PublicacaoEventHub/
├── tests/
├── deploy/
└── docs/
    ├── architecture/
    ├── can-networks/
    └── event-contracts/
```

## Responsabilidades

- `Firmware.Api` contém transporte, endpoints, hubs e composição do processo.
- `Firmware.Application` orquestra casos de uso como `StartPeriodoRastreio`, `FinishPeriodoRastreio`, `AssignMotivoParada` e `ProcessJanelaCan`.
- `Firmware.Contracts` contém contratos versionáveis da API, SignalR e Event Hubs.
- `Firmware.Domain` contém os estados da máquina, períodos de rastreio, paradas e regras de telemetria.
- `Firmware.Infrastructure` implementa SocketCAN, GPS serial, PostgreSQL, Redis, Event Hubs, LTE e SignalR.
- `Firmware.Workers` contém os ciclos contínuos de aquisição, processamento e sincronização.

## Fluxo principal

As interfaces CAN são lidas continuamente para evitar perda de frames. Um agregador fecha uma `JanelaCan` a cada dois segundos. A janela é decodificada, combinada com a posição GPS serial mais recente e transformada em uma `LeituraMaquina`. O domínio determina o `EstadoMaquina`, registra transições e atualiza o `PeriodoRastreio` aplicável.

PostgreSQL é a fonte persistente. Redis guarda estado atual e dados efêmeros. Eventos externos são gravados na `Outbox` antes da publicação no Event Hubs; mensagens recebidas passam pela `Inbox` para garantir idempotência.

As regras detalhadas estão em [`docs/architecture/regras-de-negocio.md`](docs/architecture/regras-de-negocio.md).
