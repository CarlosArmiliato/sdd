# Especificação técnica

## Resumo

A solução evolui o backend de referência para separar autenticação, identidade, perfil interno e autorização por funcionalidade. O Microsoft Entra ID continua emitindo os tokens, mas nomes de app roles não entram nas regras de negócio: claims são normalizados em um contexto de ator, roles são mapeadas para perfis persistidos no PostgreSQL e os perfis concedem permissões semânticas avaliadas por policies e `AuthorizationHandler` do ASP.NET Core.

A auditoria manterá os campos físicos e propriedades `CreatorUsername` e `ModifierUsername` para reduzir o impacto nas aplicações existentes. A semântica muda: novos registros armazenam um `ActorId` canônico e versionado, nunca nome ou e-mail. Aplicações internas usam workload identity no AKS; fornecedores usam client credentials com certificado como padrão e federação quando compatível. A API identifica cada fornecedor por App Registration próprio, valida sua atribuição de app roles e cruza `tid`, `oid` e `azp`/`appid` com um cadastro interno ativo.

## Arquitetura do sistema

### Visão dos componentes

```text
Microsoft Entra ID
        ↓ access token
Backend.Api: JwtBearer + ActorContextFactory
        ↓
PermissionAuthorizationHandler
        ↓
IPermissionContext
        ↓
Role → Perfil → Permissão + Escopo (PostgreSQL)
        ↓
Controller → Backend.App → Backend.Domain
        ↓
EF Core SaveChangesInterceptor
        ↓
CreatorUsername / ModifierUsername = ActorId canônico
```

Componentes novos ou modificados:

- `Backend.Api/Identity/HttpUserContext`: implementa `IUserContext` a partir do `ClaimsPrincipal`; exige `tid` e `oid`, diferencia usuário de aplicação pelo claim `idtyp`, lê `azp` ou `appid` e produz o `ActorId` canônico.
- `Backend.Api/Identity/ActorContextFactory`: valida a combinação mínima de claims após a validação criptográfica do token feita pelo `Microsoft.Identity.Web`; tokens incompletos falham fechados.
- `Backend.Api/Authorization/PermissionRequirement`: requisito semântico contendo somente o código da permissão.
- `Backend.Api/Authorization/PermissionAuthorizationHandler`: consulta `IPermissionContext`, avalia permissão e escopo e nunca conhece roles específicas do Entra.
- `Backend.Api/Configuration/AuthorizationPolicies`: substitui a policy genérica por policies de funcionalidade.
- `Backend.App/Abstractions/Authorization/IPermissionContext`: abstração usada pela aplicação para decisões semânticas como `HasPermission("Cadastros.Write")`.
- `Backend.App/Abstractions/Authorization/IAuthorizationCatalog`: porta para resolver roles em perfis, permissões e escopos.
- `Backend.App/Abstractions/Identity/IIdentityDirectory`: porta de consulta posterior de usuários pelo `tid + oid`.
- `Backend.Domain/Authorization`: entidades `PerfilAcesso`, `Permissao`, `PerfilPermissao`, `MapeamentoRolePerfil`, `MapeamentoRoleEscopo` e `IdentidadeAplicacao`.
- `Backend.Infra.Postgres/Authorization`: repositórios e mapeamentos EF Core do catálogo de autorização e das identidades de aplicação.
- `Backend.Infra.Entra`: adapter de consulta ao Microsoft Graph usando `HttpClient`, token obtido por workload identity e resiliência explícita.
- `Backend.Domain/Auditing` e `Backend.Infra.Postgres/Persistence/Auditing`: preservam as quatro colunas existentes, mas escrevem o `ActorId` em `CreatorUsername` e `ModifierUsername`.
- `Backend.App/Identity/SystemUserContext`: recebe a identidade da própria aplicação worker e gera `ActorId` versionado para Hangfire, polling e Event Hub; `Username` permanece como alias de compatibilidade de `ActorId`.
- `Backend.App/Abstractions/Identity/IActorContextAccessor`: mantém, no escopo de uma única execução, o ator executor e o originador opcional; o boundary do job inicializa e limpa esse contexto.
- `Backend.Contracts/Identity/ActorReferenceV1`: snapshot mínimo opcional do ator originador em jobs e eventos; nunca contém token, nome ou e-mail.
- `Backend.Api/Controllers/CadastrosController`: usa policies distintas para leitura, escrita e exclusão e serve como fluxo demonstrativo para usuário, aplicação interna e fornecedor.
- `Backend.Api/Controllers/DirectoryIdentitiesController`: endpoint administrativo de resolução de usuário no Entra ID.

Fluxo de usuário:

1. O frontend obtém token delegado para a API com `tid`, `oid`, `scp`, `roles` e `idtyp=user`.
2. `HttpUserContext` cria `v1:entra:user:{tid}:{oid}`.
3. O catálogo resolve as roles para perfis, permissões e escopos.
4. A policy exige a permissão do endpoint; a aplicação aplica o escopo quando a regra depender de filial.
5. O interceptor grava o `ActorId` nos campos legados de auditoria.

Fluxo app-only interno ou de fornecedor:

1. O cliente solicita token com `grant_type=client_credentials` e scope `api://{api-client-id}/.default`.
2. O Entra ID autentica a workload identity, certificado ou credencial federada e emite `idtyp=app`, `tid`, `oid`, `azp`/`appid` e `roles`.
3. A API valida issuer, audience e tenant; `HttpUserContext` cria `v1:entra:app:{tid}:{oid}`.
4. `IdentidadeAplicacao` confirma que `tid + oid + clientId` está ativo e identifica aplicação interna ou fornecedor.
5. As roles são mapeadas para perfis e permissões; ausência de cadastro ou permissão resulta em `403`.

Fluxo assíncrono:

1. O comando transporta somente `ActorReferenceV1` e correlation ID quando houver ator originador.
2. No início de cada job ou mensagem, o worker cria um `ActorContext` com `v1:entra:app:{tid}:{oid}` da sua workload identity/service principal e o injeta em `IActorContextAccessor` dentro do escopo da execução. O nome do host e do job permanece na telemetria.
3. `CreatorUsername` ou `ModifierUsername` recebe `Originator ?? Executor`; sem originador, recebe o `ActorId` da própria aplicação worker.
4. O boundary limpa o accessor em `finally`, inclusive em falha ou cancelamento, impedindo vazamento de identidade entre execuções concorrentes.

## Design de implementação

### Principais interfaces

```text
IUserContext
  IsAuthenticated -> bool
  IsApplication -> bool
  ActorId -> string
  TenantId -> string?
  ObjectId -> string?
  ClientId -> string?
  Roles -> IReadOnlyCollection<string>
  Username -> string (alias legado de ActorId)
```

```text
IPermissionContext
  HasPermission(permission) -> Task<bool>
  HasScope(scopeType, scopeValue) -> Task<bool>
  GetPermissions() -> Task<IReadOnlySet<string>>
```

```text
IAuthorizationCatalog
  Resolve(tenantId, objectId, clientId, roles) -> AuthorizationGrant
```

```text
IIdentityDirectory
  FindUser(tenantId, objectId, cancellationToken) -> DirectoryIdentity?
```

```text
IActorContextAccessor
  Executor -> ActorContext
  Originator -> ActorContext?
  BeginScope(executor, originator) -> IDisposable
```

`Backend.App` e `Backend.Domain` recebem tipos próprios e não referenciam `ClaimsPrincipal`, ASP.NET Core, EF Core, Azure SDK ou Microsoft Graph.

### Modelos de dados

#### `ActorContext` — identidade normalizada da requisição ou processo

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `ActorId` | `string(320)` | sim | Identificador canônico gravado nos campos legados de auditoria |
| `ActorType` | `ActorType` | sim | `User`, `InternalApplication`, `SupplierApplication` ou `BackgroundJob` |
| `TenantId` | `Guid?` | não | Claim `tid`; ausente somente para processo local |
| `ObjectId` | `Guid?` | não | Claim `oid` do usuário ou service principal |
| `ClientId` | `Guid?` | não | Claim `azp` v2 ou `appid` v1 para aplicação |
| `Roles` | `string[]` | sim | App roles recebidas; coleção vazia quando ausentes |
| `Scopes` | `string[]` | sim | Scopes delegados recebidos; coleção vazia em app-only |
| `OriginatorActorId` | `string?` | não | Ator que iniciou um processamento assíncrono |

```text
{
  "actorId": "v1:entra:app:11111111-1111-1111-1111-111111111111:22222222-2222-2222-2222-222222222222",
  "actorType": "SupplierApplication",
  "tenantId": "11111111-1111-1111-1111-111111111111",
  "objectId": "22222222-2222-2222-2222-222222222222",
  "clientId": "33333333-3333-3333-3333-333333333333",
  "roles": ["Cadastros.Integration.Write"],
  "scopes": [],
  "originatorActorId": null
}
```

Formatos válidos de `ActorId`:

| Tipo | Formato |
| --- | --- |
| Usuário Entra | `v1:entra:user:{tid:D}:{oid:D}` |
| Aplicação Entra | `v1:entra:app:{tid:D}:{oid:D}` |
| Worker em produção | `v1:entra:app:{tid:D}:{oid:D}` da workload identity/service principal do host |
| Processo local ou de teste sem Entra | `v1:system:{host}:{processo}` como fallback explícito e nunca nulo |
| Legado | Qualquer valor sem prefixo `v1:`; somente leitura |

O formato cabe no limite atual de 320 caracteres. O parser deve rejeitar valores novos malformados, mas não bloquear leitura de registros legados.

#### `PerfilAcesso` — perfil interno independente das roles do Entra

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `Id` | `Guid` | sim | Chave primária |
| `Codigo` | `string(80)` | sim | Código único e estável, como `GerenteComercial` |
| `Nome` | `string(160)` | sim | Nome administrativo |
| `Ativo` | `bool` | sim | Controla concessão sem excluir histórico |

```text
{
  "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "codigo": "IntegracaoCadastrosEscrita",
  "nome": "Integração de cadastros com escrita",
  "ativo": true
}
```

#### `Permissao` — funcionalidade autorizável

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `Id` | `Guid` | sim | Chave primária |
| `Codigo` | `string(120)` | sim | Código único no padrão `{Recurso}.{Ação}` |
| `Descricao` | `string(240)` | sim | Finalidade administrativa |

```text
{
  "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "codigo": "Cadastros.Write",
  "descricao": "Criar e alterar cadastros"
}
```

Permissões iniciais: `Cadastros.Read`, `Cadastros.Write`, `Cadastros.Delete`, `Tickets.Read`, `Tickets.Checklist.Respond`, `Jobs.Sap.Schedule` e `DirectoryIdentities.Resolve`.

#### `PerfilPermissao` — relação entre perfil e funcionalidade

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `PerfilId` | `Guid` | sim | FK para `PerfilAcesso` |
| `PermissaoId` | `Guid` | sim | FK para `Permissao` |

```text
{
  "perfilId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "permissaoId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
}
```

#### `MapeamentoRolePerfil` — tradução de role externa para perfil interno

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `Id` | `Guid` | sim | Chave primária |
| `RoleValue` | `string(120)` | sim | Valor exato recebido no claim `roles` |
| `PerfilId` | `Guid` | sim | Perfil interno concedido |
| `SubjectType` | `RoleSubjectType` | sim | `User`, `Application` ou `Both` |
| `Ativo` | `bool` | sim | Permite revogação lógica |

```text
{
  "id": "cccccccc-cccc-cccc-cccc-cccccccccccc",
  "roleValue": "Cadastros.Integration.Write",
  "perfilId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "subjectType": "Application",
  "ativo": true
}
```

#### `MapeamentoRoleEscopo` — tradução de role externa para escopo organizacional

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `Id` | `Guid` | sim | Chave primária |
| `RoleValue` | `string(120)` | sim | Valor exato recebido no token |
| `ScopeType` | `string(80)` | sim | Tipo de escopo, inicialmente `Filial` |
| `ScopeValue` | `string(120)` | sim | Valor, como `FilialA` |
| `Ativo` | `bool` | sim | Permite revogação lógica |

```text
{
  "id": "dddddddd-dddd-dddd-dddd-dddddddddddd",
  "roleValue": "Scope.Branch.FilialA",
  "scopeType": "Filial",
  "scopeValue": "FilialA",
  "ativo": true
}
```

#### `IdentidadeAplicacao` — cadastro interno da aplicação chamadora

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `Id` | `Guid` | sim | Chave primária interna |
| `TenantId` | `Guid` | sim | Tenant emissor autorizado |
| `ObjectId` | `Guid` | sim | Object ID do service principal, claim `oid` |
| `ClientId` | `Guid` | sim | Application/Client ID, claim `azp` ou `appid` |
| `Tipo` | `IdentidadeAplicacaoTipo` | sim | `Interna` ou `Fornecedor` |
| `NomeTecnico` | `string(160)` | sim | Nome operacional, sem credencial |
| `FornecedorCodigo` | `string(80)?` | não | Obrigatório quando `Tipo=Fornecedor` |
| `Ativa` | `bool` | sim | Revogação local imediata |

```text
{
  "id": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
  "tenantId": "11111111-1111-1111-1111-111111111111",
  "objectId": "22222222-2222-2222-2222-222222222222",
  "clientId": "33333333-3333-3333-3333-333333333333",
  "tipo": "Fornecedor",
  "nomeTecnico": "Fornecedor XPTO - Produção",
  "fornecedorCodigo": "XPTO",
  "ativa": true
}
```

Restrições: índices únicos em `(TenantId, ObjectId)` e `(TenantId, ClientId)`; nenhuma chave privada, certificado, segredo ou token é persistido nessa entidade.

#### `ActorReferenceV1` — identidade mínima transportada em processamento assíncrono

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `ActorId` | `string(320)` | sim | Ator originador já normalizado |
| `CorrelationId` | `string(160)` | sim | Correlação ponta a ponta |

```text
{
  "actorId": "v1:entra:user:11111111-1111-1111-1111-111111111111:44444444-4444-4444-4444-444444444444",
  "correlationId": "01K4A6M7T2P9Y3Q8R5S1V0WXYZ"
}
```

#### `DirectoryIdentityResponse` — resultado administrativo não persistido

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `TenantId` | `Guid` | sim | Tenant consultado |
| `ObjectId` | `Guid` | sim | Usuário localizado |
| `DisplayName` | `string?` | não | Exibido somente ao administrador |
| `UserPrincipalName` | `string?` | não | Exibido somente ao administrador |
| `Mail` | `string?` | não | Exibido somente ao administrador |

```text
{
  "tenantId": "11111111-1111-1111-1111-111111111111",
  "objectId": "44444444-4444-4444-4444-444444444444",
  "displayName": "Usuário localizado",
  "userPrincipalName": "usuario@empresa.example",
  "mail": null
}
```

> **Degradação:** indisponibilidade do Microsoft Graph afeta somente a consulta administrativa. Operações de negócio e auditoria permanecem disponíveis.

#### `ProblemDetails` — envelope de erro HTTP

| Código | HTTP | Significado |
| --- | --- | --- |
| `authentication_required` | `401` | Token ausente, inválido ou destinado a outra API |
| `permission_denied` | `403` | Ator autenticado sem permissão ou escopo |
| `application_identity_not_registered` | `403` | Aplicação não cadastrada, inativa ou com claims divergentes |
| `directory_identity_not_found` | `404` | Usuário não encontrado no tenant autorizado |
| `directory_unavailable` | `503` | Microsoft Graph indisponível após a política de resiliência |

```text
{
  "type": "https://httpstatuses.com/403",
  "title": "Acesso negado",
  "status": 403,
  "detail": "A identidade não possui permissão para esta operação.",
  "code": "permission_denied",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

#### Mapeamento claims do Entra → `ActorContext`

| Origem | Destino |
| --- | --- |
| `tid` | `TenantId` |
| `oid` | `ObjectId` e segmento final de `ActorId` |
| `idtyp=user` | `ActorType=User` |
| `idtyp=app` | `ActorType=InternalApplication` ou `SupplierApplication`, após consulta a `IdentidadeAplicacao` |
| `azp` (token v2) / `appid` (token v1) | `ClientId` |
| `roles` | Entrada de `IAuthorizationCatalog` |
| `scp` | `Scopes`, exigindo o scope delegado configurado para usuários |
| `email`, `preferred_username`, `unique_name`, `upn` | Nunca usados em autorização ou `ActorId` |

#### Parâmetros fixos das integrações

| Integração | Parâmetros principais |
| --- | --- |
| **Token app-only** | `grant_type=client_credentials`, `scope=api://{api-client-id}/.default` |
| **API EmpresaX** | token v2, tenant único, audience igual ao Client ID da API, claim opcional `idtyp` habilitado |
| **Microsoft Graph** | `GET /v1.0/users/{oid}`, seleção mínima de campos, credencial da API por workload identity |

### Endpoints da API

#### Visão geral

| Método | Rota | Policy | Descrição |
| --- | --- | --- | --- |
| `GET` | `/api/cadastros` | `Cadastros.Read` | Lista cadastros dentro dos escopos do ator |
| `GET` | `/api/cadastros/{id}` | `Cadastros.Read` | Obtém cadastro autorizado |
| `POST` | `/api/cadastros` | `Cadastros.Write` | Cria cadastro e demonstra auditoria humana ou app-only |
| `PUT` | `/api/cadastros/{id}` | `Cadastros.Write` | Atualiza cadastro e auditoria |
| `DELETE` | `/api/cadastros/{id}` | `Cadastros.Delete` | Exclui cadastro autorizado |
| `GET` | `/api/admin/directory-identities/{tenantId}/{objectId}` | `DirectoryIdentities.Resolve` | Localiza usuário no Entra sem persistir seus dados de exibição |

Os contratos de cadastro permanecem os existentes. A mudança funcional comum aos cinco endpoints é a aplicação de policy semântica e do escopo organizacional. Token ausente ou inválido retorna `401`; token válido sem permissão, escopo ou identidade de aplicação ativa retorna `403` sem revelar qual condição falhou.

---

#### `GET /api/cadastros`

**Parâmetros de consulta**

| Parâmetro | Tipo | Padrão | Regras |
| --- | --- | --- | --- |
| `tipo` | `CadastroTipo` | — | Obrigatório conforme contrato atual |

**Respostas**

| Status | Corpo | Quando |
| --- | --- | --- |
| `200` | `CadastroDto[]` | Permitido; coleção vazia quando não houver dados no escopo |
| `401` | `ProblemDetails` | Token ausente ou inválido |
| `403` | `ProblemDetails` | Falta `Cadastros.Read` ou escopo aplicável |

---

#### `GET /api/cadastros/{id}`

**Parâmetros de rota**

| Parâmetro | Tipo | Padrão | Regras |
| --- | --- | --- | --- |
| `id` | `Guid` | — | Identificador obrigatório |

**Respostas**

| Status | Corpo | Quando |
| --- | --- | --- |
| `200` | `CadastroDto` | Encontrado dentro do escopo autorizado |
| `401` | `ProblemDetails` | Token ausente ou inválido |
| `403` | `ProblemDetails` | Falta de permissão ou escopo |
| `404` | vazio | Ausente ou fora do escopo; evita enumeração |

---

#### `POST /api/cadastros`

**Corpo**

| Parâmetro | Tipo | Padrão | Regras |
| --- | --- | --- | --- |
| `tipo` | `CadastroTipo` | — | Obrigatório |
| `codigo` | `string` | — | Máximo 80 caracteres |
| `nome` | `string` | — | Máximo 200 caracteres |
| `codigoExterno` | `string?` | `null` | Máximo 80 caracteres |

**Respostas**

| Status | Corpo | Quando |
| --- | --- | --- |
| `201` | `CadastroDto` | Criado e auditado com `CreatorUsername=ActorId` |
| `400` | `IResponse` | Validação de negócio |
| `401` | `ProblemDetails` | Token ausente ou inválido |
| `403` | `ProblemDetails` | Falta `Cadastros.Write` ou escopo |
| `409` | `IResponse` | Código duplicado |

---

#### `PUT /api/cadastros/{id}`

Usa o mesmo corpo do `POST`.

**Respostas**

| Status | Corpo | Quando |
| --- | --- | --- |
| `200` | `CadastroDto` | Atualizado e auditado com `ModifierUsername=ActorId` |
| `400` | `IResponse` | Validação de negócio |
| `401` | `ProblemDetails` | Token ausente ou inválido |
| `403` | `ProblemDetails` | Falta `Cadastros.Write` ou escopo |
| `404` | vazio | Cadastro ausente ou fora do escopo |
| `409` | `IResponse` | Código duplicado |

---

#### `DELETE /api/cadastros/{id}`

**Parâmetros de rota**

| Parâmetro | Tipo | Padrão | Regras |
| --- | --- | --- | --- |
| `id` | `Guid` | — | Identificador obrigatório |

**Respostas**

| Status | Corpo | Quando |
| --- | --- | --- |
| `204` | vazio | Excluído |
| `401` | `ProblemDetails` | Token ausente ou inválido |
| `403` | `ProblemDetails` | Falta `Cadastros.Delete` ou escopo |
| `404` | vazio | Cadastro ausente ou fora do escopo |

---

#### `GET /api/admin/directory-identities/{tenantId}/{objectId}`

Consulta o Microsoft Graph somente para administradores autorizados. O `tenantId` deve ser igual ao tenant configurado; outro tenant retorna `404` para evitar enumeração.

**Parâmetros de rota**

| Parâmetro | Tipo | Padrão | Regras |
| --- | --- | --- | --- |
| `tenantId` | `Guid` | — | Tenant único configurado |
| `objectId` | `Guid` | — | `oid` extraído do `ActorId` |

**Respostas**

| Status | Corpo | Quando |
| --- | --- | --- |
| `200` | `DirectoryIdentityResponse` | Usuário localizado |
| `400` | `ProblemDetails` | Identificadores inválidos |
| `401` | `ProblemDetails` | Token ausente ou inválido |
| `403` | `ProblemDetails` | Falta `DirectoryIdentities.Resolve` |
| `404` | `ProblemDetails` | Usuário ou tenant não localizado |
| `503` | `ProblemDetails` | Graph indisponível após timeout/retry |

**Exemplo — sucesso**

```http
GET /api/admin/directory-identities/11111111-1111-1111-1111-111111111111/44444444-4444-4444-4444-444444444444
```

Retorna o `DirectoryIdentityResponse` documentado anteriormente.

> A resposta não é persistida nem usada para autorização. Logs não incluem nome, e-mail, UPN ou corpo da resposta.

---

## Pontos de integração

### Microsoft Entra ID

- A API permanece single-tenant e aceita tokens v2 somente do tenant configurado.
- O middleware valida assinatura, issuer, audience, expiração e tenant antes da construção do ator.
- O App Registration da API expõe um scope delegado para usuários e app roles para clientes app-only.
- `idtyp` deve ser configurado como claim opcional para distinguir com segurança tokens de usuário e de aplicação.
- App-only sem role, identidade interna ativa ou combinação coerente de `tid + oid + clientId` falha com `403`.
- App Registrations e app role assignments são provisionados fora da aplicação, preferencialmente por IaC revisável; não haverá endpoint runtime para criar credenciais.

### Integrações internas

- Aplicações executadas no AKS usam workload identity/managed identity e recebem somente as app roles necessárias na API consumida.
- Aplicações fora do Azure usam certificado ou federação; client secret não é o padrão alvo.

### Fornecedores REST

- Cada fornecedor, ou sistema que precise de revogação independente, recebe App Registration exclusivo no tenant da EmpresaX e seu `ClientId` público.
- O fornecedor gera e protege a chave privada e entrega somente o certificado público para cadastro; a aplicação da EmpresaX não recebe a chave privada.
- O fornecedor solicita token ao endpoint do tenant usando client assertion assinada e chama a API com bearer token.
- Quando o fornecedor possuir IdP compatível, credencial federada pode substituir o certificado sem alterar autorização ou auditoria.
- A API não recebe, persiste ou valida diretamente certificado/segredo do fornecedor; ela valida apenas o access token emitido pelo Entra.

### Microsoft Graph

- `Backend.Infra.Entra` chama `GET /v1.0/users/{oid}` com `$select=id,displayName,userPrincipalName,mail`.
- A autenticação usa workload identity da API e a menor application permission que permita os campos definidos; a concessão requer consentimento administrativo.
- Timeout inicial de 3 segundos, no máximo uma repetição com jitter somente para `429`, `502`, `503` e `504`; respeitar `Retry-After` e limite total de 7 segundos.
- `404` vira `directory_identity_not_found`; esgotamento transitório vira `directory_unavailable`.
- Falha do Graph não afeta endpoints de negócio.

## Abordagem de testes

Cobertura mínima: 80% conforme `template/.agents/rules/tests.md`. Testes usam xUnit; mocks somente para Microsoft Graph e aquisição de token. Relógio, claims, IDs e dados persistidos são determinísticos.

### Testes de unidade

| ID | Nome do caso de teste | Critérios de aceitação | Resultado esperado |
| --- | --- | --- | --- |
| `TU-01` | Criar ActorId de usuário | CA-03, CA-04 | Gera `v1:entra:user:{tid}:{oid}` sem nome ou e-mail |
| `TU-02` | Criar ActorId de aplicação | CA-05, CA-08 | Gera `v1:entra:app:{tid}:{oid}` e preserva `clientId` no contexto |
| `TU-03` | Rejeitar claims incompletas | CA-02, CA-06, CA-09 | Contexto não é criado sem `tid`, `oid` ou `idtyp` esperado |
| `TU-04` | Resolver role para perfil e permissão | CA-01, CA-09 | Role ativa concede somente permissões do perfil mapeado |
| `TU-05` | Aplicar escopo de filial | CA-01, CA-02 | Acesso fica restrito às filiais concedidas |
| `TU-06` | Autorizar policy semântica | CA-01, CA-02, CA-06, CA-09 | Handler concede ou nega sem consultar nomes de roles diretamente |
| `TU-07` | Resolver identidade de fornecedor | CA-07, CA-08, CA-10 | Combinação `tid + oid + clientId` ativa identifica fornecedor correto |
| `TU-08` | Rejeitar fornecedor inativo ou divergente | CA-09, CA-10 | Acesso falha fechado |
| `TU-09` | Gravar ActorId em campo legado | CA-03, CA-05, CA-08 | Writer grava `ActorId` em `CreatorUsername`/`ModifierUsername` |
| `TU-10` | Preservar auditoria legada | CA-03 | Valor histórico sem prefixo permanece legível e não é reescrito |
| `TU-11` | Selecionar ator de auditoria do worker | CA-05, CA-08 | Originador é auditado quando informado; sem originador, usa o ActorId da própria aplicação worker |
| `TU-12` | Limpar contexto ao finalizar job | CA-05, CA-08 | Accessor fica vazio após sucesso, falha ou cancelamento |

### Testes de integração

| ID | Nome do caso de teste | Critérios de aceitação | Resultado esperado |
| --- | --- | --- | --- |
| `TI-01` | Usuário com permissão e escopo | CA-01 | Endpoint retorna sucesso somente para dados do escopo |
| `TI-02` | Usuário sem permissão ou escopo | CA-02 | Endpoint retorna `403` ou `404` sem enumerar recurso |
| `TI-03` | Persistência de auditoria humana | CA-03 | PostgreSQL recebe ActorId e nenhum e-mail novo |
| `TI-04` | Resolução administrativa no Graph | CA-04 | Adapter retorna usuário e não persiste resposta |
| `TI-05` | Indisponibilidade do Graph | CA-04 | Endpoint retorna `503` após política limitada e negócio segue saudável |
| `TI-06` | Aplicação interna autorizada | CA-05 | Alteração registra ator app-only correto |
| `TI-07` | Aplicação interna sem app role | CA-06 | Endpoint retorna `403` |
| `TI-08` | Cadastro exclusivo de fornecedor | CA-07 | Índices rejeitam reuso indevido de `objectId` ou `clientId` |
| `TI-09` | Fornecedor autorizado e auditado | CA-08 | Operação permitida identifica fornecedor e grava ActorId |
| `TI-10` | Fornecedor fora do contrato | CA-09 | Endpoint retorna `403` |
| `TI-11` | Revogação isolada | CA-10 | Fornecedor inativo falha e outra identidade continua autorizada |
| `TI-12` | Configuração sem client secret | CA-11 | Configuração e manifesto não contêm segredo de fornecedor |
| `TI-13` | Auditoria em workers | CA-05, CA-08 | Cada host usa o ActorId de sua workload identity quando não há originador e propaga originador/correlação quando houver |
| `TI-14` | Dependências arquiteturais | CA-01, CA-05 | App/Domain não referenciam ASP.NET, EF, SDK Entra ou Graph |
| `TI-15` | Isolamento entre jobs concorrentes | CA-05, CA-08 | Cada job persiste seu próprio ator sem reutilizar contexto de outra execução |

Os testes HTTP usam `WebApplicationFactory<Program>` com esquema de autenticação de teste que emite conjuntos controlados de claims. Os testes de persistência usam PostgreSQL isolado e aplicam migrations reais. Nenhum teste depende do tenant de produção.

### Testes E2E

| ID | Nome do caso de teste | Critérios de aceitação | Resultado esperado |
| --- | --- | --- | --- |
| `E2E-01` | Fluxo delegado de usuário | CA-01 a CA-04 | Token real de tenant de teste autoriza, restringe escopo, audita e permite resolução administrativa |
| `E2E-02` | Fluxo app-only interno | CA-05, CA-06 | Workload identity autorizada grava alteração; identidade sem role é negada |
| `E2E-03` | Fluxo de fornecedor com certificado | CA-07 a CA-09, CA-11 | Client assertion obtém token; role limita operação e auditoria identifica fornecedor |
| `E2E-04` | Revogação do fornecedor | CA-10 | Desativação local bloqueia imediatamente o fornecedor sem afetar outro |

Os E2E são testes REST executados em ambiente de homologação com tenant e App Registrations exclusivos de teste. Não há interface visual no escopo, portanto ferramenta de navegador não é aplicável.

## Sequenciamento do desenvolvimento

### Ordem de construção

1. Criar enums, formato de `ActorId`, `ActorContext` e testes unitários, pois todas as demais etapas dependem da identidade normalizada.
2. Alterar a auditoria para gravar `ActorId` nos campos existentes e criar o escopo de `ActorContext` no boundary de cada execução dos três workers, sem migração das tabelas auditáveis.
3. Criar o catálogo PostgreSQL de perfis, permissões, mapeamentos, escopos e identidades de aplicação, incluindo migration e dados iniciais do exemplo.
4. Implementar `IPermissionContext`, policies e handlers; aplicar policies ao `CadastrosController` e demais endpoints existentes conforme a tabela de permissões.
5. Configurar app-only, claim `idtyp`, validações adicionais e testes HTTP de usuário, aplicação interna e fornecedor.
6. Criar `Backend.Infra.Entra`, o caso de uso e o endpoint administrativo de resolução.
7. Propagar `ActorReferenceV1` e correlation ID nos fluxos assíncronos aplicáveis.
8. Provisionar tenant de teste, App Registrations, certificados/workload identities e executar integração, E2E, cobertura e arquitetura.

### Dependências técnicas

- Tenant de teste da EmpresaX e permissão administrativa para criar App Registrations, expor app roles e conceder consentimento.
- App Registration da API com access token v2, scope delegado e claim opcional `idtyp`.
- Uma identidade de usuário, uma workload identity interna e ao menos duas identidades de fornecedor para validar isolamento.
- Certificados de teste gerados pelos respectivos clientes; somente chaves públicas são cadastradas no Entra.
- Workload identity da API com permissão mínima no Microsoft Graph.
- PostgreSQL para catálogo e testes de migrations.
- Nova referência do projeto `Backend.Infra.Entra` na solução e no host `Backend.Api`.
- Retenção e acesso aos logs definidos por Segurança/Compliance; não bloqueia o protótipo funcional.

## Monitoramento e observabilidade

- Preservar e propagar `traceId`/correlation ID em API, jobs e eventos.
- Registrar decisão de autorização, policy, tipo do ator e resultado; o `ActorId` pseudonimizado pode ser registrado somente no nível e destino aprovados para auditoria.
- Nunca registrar access token, client assertion, certificado, segredo, nome, e-mail, UPN ou lista completa de claims.
- Métricas: `authorization_decisions_total{policy,actor_type,result}`, `unregistered_application_total{actor_type}`, `directory_lookup_duration_ms{result}` e `directory_lookup_failures_total{status}`. Não usar IDs como labels.
- Alertar para aumento de `403` de aplicações previamente ativas, falhas repetidas de consulta ao Graph e identidades de fornecedor inativas tentando acesso.
- A falha ao preencher auditoria aborta a transação de negócio; não existe gravação silenciosa sem ator.
- O health check principal não depende do Graph. Expor diagnóstico separado da integração, evitando retirar a API de operação por falha de uma consulta administrativa.

## Considerações técnicas

### Principais decisões

- **Compatibilidade de banco:** manter `CreatorUsername` e `ModifierUsername` e armazenar `ActorId`. Alternativa de novas FKs foi descartada para reduzir impacto nas aplicações existentes.
- **Formato versionado:** prefixo `v1:` diferencia novos valores dos e-mails legados, preserva tipo de ator e permite evolução futura.
- **Identidade canônica:** `tid + oid` identifica o objeto concreto no tenant; `clientId` é validado e mantido no cadastro de aplicações, mas não substitui o `oid` na auditoria.
- **Autorização desacoplada:** roles do Entra são entradas de mapeamento; regras e handlers consomem permissões internas. Verificações espalhadas de `Roles.Contains("Admin")` são proibidas.
- **Policies ASP.NET Core:** cada endpoint declara intenção semântica; regras contextuais usam `IPermissionContext` na aplicação.
- **Persistência como fonte de verdade:** PostgreSQL mantém perfis, permissões, escopos e identidades. Redis fica fora da primeira versão para evitar invalidação e autorização obsoleta.
- **Revogação local:** `IdentidadeAplicacao.Ativa=false` bloqueia imediatamente novos requests mesmo que um access token ainda não tenha expirado; revogar App Registration ou app role continua obrigatório como defesa em profundidade.
- **Credencial de fornecedor:** certificado é o baseline do protótipo; federação é alternativa preferível quando o ambiente do fornecedor disponibiliza IdP compatível. Client secret não faz parte do cenário alvo.
- **Workers sem originador:** o boundary do job injeta um `ActorContext` da workload identity/service principal do próprio worker. A auditoria usa `Originator ?? Executor`, e o fallback `v1:system:*` existe somente para execução local ou testes sem Entra. O escopo é descartado em `finally` para evitar vazamento entre jobs.
- **Provisionamento externo:** App Registrations, certificados públicos e role assignments são administrados por IaC/manual revisado, nunca pela API de negócio.
- **Graph isolado:** consulta administrativa usa uma porta da aplicação e adapter próprio, não participa do caminho crítico de autorização.
- **Gravações:** todas as escritas atuais passam pelo EF Core e permanecem cobertas pelo interceptor. Novas escritas diretas por SQL são proibidas; se inevitáveis, devem usar mecanismo equivalente de auditoria na mesma transação.

### Riscos conhecidos

- **Semântica legada ambígua:** a coluna chamada `Username` passa a armazenar ActorId. Mitigação: formato `v1:`, documentação, parser único e testes de compatibilidade.
- **Dados históricos:** linhas antigas continuam contendo e-mail e não ganham `tid/oid` retroativamente. Mitigação: tratá-las como legado, restringir acesso e definir migração separada somente se houver fonte confiável.
- **Role-less app-only token:** aceitar token app-only sem role pode contornar autorização. Mitigação: exigir app role, `idtyp=app`, identidade local ativa e assignment requirement no Enterprise Application.
- **Confusão entre `clientId` e `oid`:** App Registration e service principal são objetos distintos. Mitigação: validar ambos e usar `tid + oid` como ator auditável.
- **Revogação no Entra não instantânea:** tokens existentes podem permanecer válidos até expirar. Mitigação: verificação local de `Ativa` em cada request e expiração curta conforme política corporativa.
- **Alteração de role demora a propagar:** onboarding e testes devem considerar consistência eventual do Entra.
- **Graph indisponível ou limitado:** consulta administrativa pode falhar ou sofrer throttling. Mitigação: timeout, retry limitado, `Retry-After`, `503` explícito e isolamento do negócio.
- **Alta cardinalidade de autorização:** consultar PostgreSQL por request pode elevar latência. Medir antes de adicionar cache; se necessário, cachear por role-set com TTL curto e invalidação versionada.
- **Fornecedor sem suporte a certificado/federação:** tratar como bloqueador de onboarding ou exceção formal de segurança fora do baseline; não reintroduzir segredo silenciosamente.
- **ActorId originador adulterado em job:** somente código interno pode criar `ActorReferenceV1`; validar formato e não aceitar esse contrato em endpoint público.

### Conformidade com o AGENTS.md e as rules

Foram lidos integralmente `AGENTS.md`, `template/AGENTS.md` e todas as rules em `template/.agents/rules/`: `code-standards.md`, `dotnet.md`, `folder-structure.md`, `tests.md` e `javascript-typescript.md`. Não existe `.agents/` na raiz; para o aplicativo de referência foram consideradas as regras sob `template/.agents/`.

Esta especificação mantém Controllers e hosts finos, coloca portas e casos de uso em `Backend.App`, entidades em `Backend.Domain`, persistência em `Backend.Infra.Postgres` e Graph em `Backend.Infra.Entra`. `Backend.App` e `Backend.Domain` não dependem de ASP.NET Core, EF Core ou fornecedores. Operações assíncronas transportam identidade mínima e correlation ID, nunca access token. Segredos não são versionados; AKS usa workload identity. Todo comportamento terá testes automatizados, cobertura mínima de 80% e verificação arquitetural.

A rule de JavaScript/TypeScript foi lida, mas não gera restrições adicionais porque frontend e UI estão fora do escopo desta entrega.

### Conformidade com skills

As skills presentes em `template/.agents/skills/` são `react`, `caveman` e `impeccable`. Nenhuma é aplicável: esta TechSpec cobre somente backend, autenticação, autorização, persistência e integrações REST. Se uma interface administrativa entrar no escopo futuro, a skill `react` deverá ser carregada antes da especificação e implementação do frontend.

### Arquivos relevantes e dependentes

Arquivos existentes a modificar:

- `template/backend/src/Backend.Api/Program.cs`
- `template/backend/src/Backend.Api/Configuration/AuthorizationPolicies.cs`
- `template/backend/src/Backend.Api/Controllers/CadastrosController.cs`
- `template/backend/src/Backend.Api/Controllers/TicketsController.cs`
- `template/backend/src/Backend.Api/Controllers/JobsController.cs`
- `template/backend/src/Backend.Api/appsettings.json`
- `template/backend/src/Backend.App/Abstractions/Identity/IUserContext.cs`
- `template/backend/src/Backend.App/Abstractions/Identity/IActorContextAccessor.cs`
- `template/backend/src/Backend.App/Identity/SystemUserContext.cs`
- `template/backend/src/Backend.Domain/Auditing/IAuditableEntity.cs`
- `template/backend/src/Backend.Domain/Auditing/AuditableEntity.cs`
- `template/backend/src/Backend.Infra.Postgres/Persistence/BackendDbContext.cs`
- `template/backend/src/Backend.Infra.Postgres/Persistence/Auditing/AuditableSaveChangesInterceptor.cs`
- `template/backend/src/Backend.Infra.Postgres/Persistence/Auditing/AuditChangeApplier.cs`
- `template/backend/src/Backend.Infra.Postgres/Persistence/Auditing/AuditableEntryWriter.cs`
- `template/backend/src/Backend.Worker.Hangfire/Program.cs`
- `template/backend/src/Backend.Worker.Polling/Program.cs`
- `template/backend/src/Backend.Worker.EventHub/Program.cs`
- `template/backend/Backend.slnx`
- `template/backend/Directory.Packages.props`

Principais arquivos novos:

- `template/backend/src/Backend.Api/Identity/HttpUserContext.cs`
- `template/backend/src/Backend.Api/Identity/ActorContextFactory.cs`
- `template/backend/src/Backend.Api/Authorization/PermissionRequirement.cs`
- `template/backend/src/Backend.Api/Authorization/PermissionAuthorizationHandler.cs`
- `template/backend/src/Backend.Api/Controllers/DirectoryIdentitiesController.cs`
- `template/backend/src/Backend.Api/Contracts/DirectoryIdentityResponse.cs`
- `template/backend/src/Backend.App/Abstractions/Authorization/IPermissionContext.cs`
- `template/backend/src/Backend.App/Abstractions/Authorization/IAuthorizationCatalog.cs`
- `template/backend/src/Backend.App/Abstractions/Identity/IIdentityDirectory.cs`
- `template/backend/src/Backend.Domain/Authorization/PerfilAcesso.cs`
- `template/backend/src/Backend.Domain/Authorization/Permissao.cs`
- `template/backend/src/Backend.Domain/Authorization/PerfilPermissao.cs`
- `template/backend/src/Backend.Domain/Authorization/MapeamentoRolePerfil.cs`
- `template/backend/src/Backend.Domain/Authorization/MapeamentoRoleEscopo.cs`
- `template/backend/src/Backend.Domain/Authorization/IdentidadeAplicacao.cs`
- `template/backend/src/Backend.Contracts/Identity/ActorReferenceV1.cs`
- `template/backend/src/Backend.Infra.Postgres/Authorization/AuthorizationCatalog.cs`
- `template/backend/src/Backend.Infra.Postgres/Authorization/AuthorizationMappings.cs`
- `template/backend/src/Backend.Infra.Entra/Backend.Infra.Entra.csproj`
- `template/backend/src/Backend.Infra.Entra/EntraIdentityDirectory.cs`
- `template/backend/src/Backend.Infra.Entra/EntraDirectoryOptions.cs`
- migrations e testes correspondentes em `template/backend/tests/Backend.UnitTests`, `Backend.IntegrationTests` e `Backend.ArchitectureTests`.
