# Tarefa 6.0: Consultar identidades no Microsoft Graph

## Visão geral

Permitir que operadores autorizados resolvam um ActorId humano para os dados atuais do usuário no Entra ID, sem persistir PII adicional nas entidades de negócio. A consulta será administrativa, auditável e resiliente a indisponibilidade ou remoção do usuário.

<skills>
### Conformidade com skills

As skills disponíveis em `template/.agents/skills` foram avaliadas. Nenhuma delas é aplicável, pois a tarefa é uma integração backend com Microsoft Graph.
</skills>

<rules>
### Conformidade com o AGENTS.md e as rules

Foram lidos o `AGENTS.md` da raiz, o `template/AGENTS.md` e todas as rules em `template/.agents/rules`. O adaptador externo deve ficar em `Backend.Infra.Entra`, atrás de uma abstração de `Backend.App`; Controllers permanecem finos; opções e `HttpClient` são tipados; falhas externas devem ser tratadas e observáveis. A rule de JavaScript/TypeScript não se aplica.
</rules>

<requirements>

- RF3 — Localizar posteriormente um usuário no Entra ID a partir do identificador estável persistido.

</requirements>

## Subtarefas

- [ ] 6.1 Criar a abstração de aplicação para resolução de identidade por tenant ID e object ID.
- [ ] 6.2 Criar o adaptador `Backend.Infra.Entra` para Microsoft Graph com credencial gerenciada, options tipadas e `HttpClient` resiliente.
- [ ] 6.3 Implementar o caso de uso que valida e decompõe ActorIds humanos antes da consulta externa.
- [ ] 6.4 Criar o endpoint administrativo `GET /api/admin/directory-identities/{tenantId}/{objectId}` protegido por permissão específica.
- [ ] 6.5 Definir respostas para usuário não encontrado, removido, tenant inválido e indisponibilidade do Graph sem alterar a auditoria histórica.
- [ ] 6.6 Registrar telemetria sem expor tokens nem PII desnecessária.
- [ ] 6.7 Implementar testes de integração para resolução bem-sucedida e indisponibilidade do Graph.

## Detalhes de implementação

Seguir as seções “Resolução posterior de usuários”, “Integração com Microsoft Graph” e “Endpoints administrativos” da TechSpec. O endpoint deve retornar apenas os campos necessários ao operador autorizado e nunca substituir o ActorId histórico pelo nome ou e-mail resolvido.

## Critérios de aceitação relacionados

- CA-04 — Um operador autorizado consegue resolver a identidade atual do usuário a partir de `tid + oid`.

## Testes da tarefa

### Testes de unidade

A validação do formato de ActorId humano é coberta por TU-01 e TU-03 na tarefa 1.0.

### Testes de integração

- [ ] TI-04 — Resolver usuário no Microsoft Graph.
- [ ] TI-05 — Tratar indisponibilidade ou ausência no Microsoft Graph.

### Testes E2E

O fluxo com identidade real será coberto por E2E-01 na tarefa 8.0.

## Arquivos relevantes

- `template/backend/src/Backend.App/Directory/` — abstração e caso de uso de resolução.
- `template/backend/src/Backend.Infra.Entra/` — novo adaptador do Microsoft Graph.
- `template/backend/src/Backend.API/Controllers/AdminDirectoryIdentitiesController.cs` — novo endpoint administrativo.
- `template/backend/tests/Backend.IntegrationTests/Directory/` — testes com Graph simulado.
- `tasks/prd-seguranca-entra/techspec.md` — contrato, segurança e comportamento de falhas.
