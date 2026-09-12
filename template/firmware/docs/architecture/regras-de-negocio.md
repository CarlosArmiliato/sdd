# Regras de negócio

## Vocabulário

| Conceito | Definição |
|---|---|
| `JanelaCan` | Conjunto de frames recebidos das três redes CAN durante dois segundos. |
| `LeituraMaquina` | Valores normalizados obtidos da janela CAN e do GPS serial. |
| `EstadoMaquina` | Classificação operacional: `Efetivo`, `Parada`, `Manobra` ou `Deslocamento`. |
| `PeriodoRastreio` | Intervalo operacional associado a um operador, automático ou desconsiderado. |
| `OcorrenciaParada` | Período contínuo em que a máquina permanece em `Parada`. |
| `MotivoParada` | Justificativa informada pelo operador para uma `OcorrenciaParada`. |

## Aquisição e telemetria

- **RN-001 — Leitura contínua:** `can0`, `can1` e `can2` devem ser lidas continuamente. O intervalo de dois segundos delimita o processamento, não a abertura das interfaces CAN.
- **RN-002 — Janela de processamento:** a cada dois segundos, os frames acumulados formam uma `JanelaCan` identificada por início e fim.
- **RN-003 — Origem do frame:** todo frame deve preservar interface CAN, identificador, payload e instante de recebimento.
- **RN-004 — Decodificação:** cada janela pode gerar `Velocidade`, `Rpm`, `Marcha`, `Vazao`, `AlturaImplemento`, posição CAN e outros sinais configurados.
- **RN-005 — Posições independentes:** `PosicaoCan` e `PosicaoGps` são valores distintos e preservam fonte, instante e qualidade. Uma fonte nunca deve sobrescrever silenciosamente a outra.
- **RN-006 — GPS mais recente:** o processamento utiliza a leitura válida mais recente do GPS serial, sujeita a um limite configurável de idade.
- **RN-007 — Persistência:** leituras processadas e transições de estado são persistidas no PostgreSQL. Frames brutos possuem retenção diagnóstica configurável e limitada.

## Estado da máquina

- **RN-008 — Estados válidos:** `EstadoMaquina` aceita somente `Efetivo`, `Parada`, `Manobra` e `Deslocamento`.
- **RN-009 — Parada:** velocidade normalizada igual a zero resulta em `Parada`.
- **RN-010 — Efetivo:** com velocidade maior que zero, o estado é `Efetivo` quando todos os critérios configurados de operação efetiva forem atendidos.
- **RN-011 — Manobra:** com velocidade maior que zero e sem operação efetiva, os primeiros trinta segundos contínuos de movimento resultam em `Manobra`.
- **RN-012 — Deslocamento:** com velocidade maior que zero, sem operação efetiva e após trinta segundos contínuos de movimento, o estado é `Deslocamento`.
- **RN-013 — Reinício da duração:** uma transição para `Parada` reinicia a contagem usada para distinguir `Manobra` de `Deslocamento`.
- **RN-014 — Transições:** somente mudanças de estado geram uma nova transição; janelas sucessivas com o mesmo estado apenas atualizam a telemetria.

Ordem de decisão:

```text
Velocidade == 0                          → Parada
Critérios de operação efetiva atendidos → Efetivo
Movimento contínuo <= 30 segundos       → Manobra
Movimento contínuo > 30 segundos        → Deslocamento
```

## Período de rastreio

- **RN-015 — Início pelo operador:** um crachá válido inicia um `PeriodoRastreio` do tipo `Operador` e associa o operador identificado.
- **RN-016 — Exclusividade:** somente um `PeriodoRastreio` pode permanecer ativo por máquina.
- **RN-017 — Idempotência:** repetir a solicitação de início com a mesma chave de operação não cria períodos duplicados.
- **RN-018 — Encerramento manual:** o operador pode encerrar seu período ativo pela API.
- **RN-019 — Período desconsiderado:** após o encerramento manual, inicia-se um `PeriodoRastreio` do tipo `Desconsiderado` enquanto a máquina permanecer em `Parada`.
- **RN-020 — Movimento após encerramento:** quando a máquina deixa `Parada`, o período `Desconsiderado` é encerrado no instante da transição e um `PeriodoRastreio` do tipo `Automatico` é iniciado sem operador.
- **RN-021 — Novo operador:** ao iniciar um período com crachá, qualquer período `Automatico` ou `Desconsiderado` ativo é encerrado antes do início do período `Operador`.
- **RN-022 — Rastreabilidade:** início, encerramento, tipo e motivo de cada transição de período devem ser persistidos.

## Paradas e interação com o operador

- **RN-023 — Abertura da parada:** uma transição para `Parada` abre uma única `OcorrenciaParada`.
- **RN-024 — Solicitação de motivo:** o motivo é solicitado somente quando existe um `PeriodoRastreio` do tipo `Operador` ativo.
- **RN-025 — Sem duplicação:** janelas CAN subsequentes em `Parada` não criam novas ocorrências nem novas solicitações para a mesma parada.
- **RN-026 — Associação:** o motivo informado é associado à ocorrência aberta, ao período de rastreio e ao operador.
- **RN-027 — Encerramento da parada:** a ocorrência é encerrada quando o estado deixa de ser `Parada`.
- **RN-028 — Validação da API:** uma tentativa de informar motivo sem ocorrência elegível deve ser rejeitada como conflito de estado.

## Sincronização

- **RN-029 — Outbox:** dados destinados ao Event Hubs devem ser persistidos na mesma transação dos dados de negócio antes de qualquer tentativa de publicação.
- **RN-030 — Operação offline:** indisponibilidade da LTE não impede aquisição, processamento ou persistência local.
- **RN-031 — Retentativa:** eventos pendentes permanecem na `Outbox` e são reenviados após o restabelecimento da conexão.
- **RN-032 — Inbox:** mensagens recebidas do Event Hubs devem possuir identidade e não podem produzir efeitos duplicados.
- **RN-033 — SignalR:** notificações para o frontend representam mudanças já aceitas pelo domínio; SignalR não é fonte persistente.

## Convenções de código

- Nomes do domínio ficam em português e sem acentos: `PeriodoRastreio`, `EstadoMaquina`, `Velocidade`, `Rpm` e `MotivoParada`.
- Termos técnicos podem permanecer em inglês: `Handler`, `Worker`, `Repository`, `Endpoint`, `Hub`, `Outbox` e `Inbox`.
- Verbos técnicos em inglês podem anteceder o conceito em português: `SendPeriodoRastreioHandler`, `GetEstadoMaquinaHandler` e `ProcessJanelaCanHandler`.
- Contratos JSON usam `camelCase`, sem abreviações diferentes das adotadas no domínio.
- Valores de enumeração enviados para fora da aplicação devem ser versionados e não podem ser renomeados sem compatibilidade.

## Definições pendentes

- Critérios completos de operação `Efetivo` para cada tipo de máquina ou implemento.
- Tolerância usada para normalizar pequenas oscilações de velocidade para zero.
- Idade máxima aceitável da última posição GPS.
- Encerramento de um período `Desconsiderado` quando a máquina continua parada por tempo prolongado.
- Catálogo de PGNs, SPNs, DBCs e mensagens específicas por interface CAN.
- Contratos, particionamento e retenção dos tópicos do Event Hubs.
