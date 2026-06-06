# Examen Técnico Fullstack (.NET / Front-End)



---

## 0) Instrucciones Generales (para el candidato)

1. **Entrega** en un repositorio (GitHub/GitLab/Azure DevOps)con:
   - `/backend` (API .NET 6)
   - `/frontend` (Angular o React)
   - `/db` (scripts SQL + respuestas)
   - `/docs` (decisiones, diagramas, caso de análisis, pasos de ejecución)
2. Incluye un **README** con:
   - Requisitos para correr (SDK, Node, DB opcional)
   - Cómo ejecutar (comandos `dotnet`, `npm`)
   - Endpoints principales y ejemplo de requests
3. Puedes simular DB con in-memory, SQLite o SQL Server local **para el ejercicio práctico**, pero **la sección DB exige SQL real** (SQL Server u Oracle).
4. **Se evalúa calidad**, no solo que “funcione”.
5. Puedes usar librerías comunes (AutoMapper, FluentValidation, Serilog, EF Core, Axios/Fetch, RxJS), pero justifica si agregas dependencias extra.

---

# 1) Ejercicio Práctico Backend — API REST en .NET 8

## Contexto

Vas a construir una API REST para administrar **Tickets de Soporte** (helpdesk) en un entorno corporativo.

### Entidades

- **Ticket**
  - `Id` (GUID o INT)
  - `Title` (string, requerido, 5–120)
  - `Description` (string, requerido, 10–2000)
  - `Priority` (Low/Medium/High/Critical)
  - `Status` (Open/InProgress/Resolved/Closed)
  - `CreatedAt`, `UpdatedAt`
  - `CreatedBy` (string email o userId)
- **Comment**
  - `Id`, `TicketId`
  - `Text` (requerido, 2–1000)
  - `CreatedAt`, `CreatedBy`

## Requisitos funcionales (mínimo)

Implementa endpoints REST con **arquitectura en capas**:

1. **Tickets**
   - `GET /api/tickets?status=&priority=&q=&page=&pageSize=`
     - Soporta **filtrado**, **búsqueda por texto** (`q` en Title/Description), **paginación** y orden.
   - `GET /api/tickets/{id}`
   - `POST /api/tickets`
   - `PUT /api/tickets/{id}` (actualiza campos principales)
   - `PATCH /api/tickets/{id}/status` (cambio de estado)
2. **Comentarios**
   - `POST /api/tickets/{id}/comments`
   - `GET /api/tickets/{id}/comments`

## Requisitos NO funcionales (calidad)

- **Arquitectura en capas (obligatorio):**
  - `API` (Controllers)
  - `Application/Services` (casos de uso)
  - `Domain` (entidades, enums, reglas)
  - `Infrastructure` (repositorios, persistencia)
- **DTOs + Validación** (ej. DataAnnotations/FluentValidation)
- **Manejo de errores global** (middleware/filters) con respuestas consistentes:
  - `400` validación
  - `404` no encontrado
  - `409` conflictos (ej. transición de estado inválida)
  - `500` error inesperado
- **Seguridad básica (obligatorio):**
  - Evitar overposting (no aceptar entidad completa en POST/PUT)
  - Sanitizar/validar inputs (mínimo)
  - Preparado para auth (por ejemplo, header `X-User` o stub de JWT)
- **Buenas prácticas:**
  - Inyección de dependencias
  - Logging estructurado (mínimo en errores y operaciones críticas)
  - Código legible, nombres claros, separación de responsabilidades
- **Extra (suma puntos):**
  - Tests unitarios (xUnit) para services/reglas
  - Swagger bien documentado (summary, response codes)
  - EF Core con migraciones (opcional)

## Entregables

- Código + README
- Colección Postman/Thunder Client (opcional, suma puntos)
- Documento breve `/docs/backend-decisions.md` (3–10 bullets) con decisiones y trade-offs



---

# 2) Ejercicio Práctico Front-End — Angular (20+) o React

## Objetivo

Construir una pantalla para operar Tickets consumiendo la API del punto 2. Debe reflejar **experiencia real**: arquitectura, manejo de estado, UI/UX, errores y performance básico.

## Requerimientos funcionales (mínimo)

1. **Listado de tickets**
   - Tabla o cards con: Title, Priority, Status, CreatedAt
   - Filtros: Status, Priority
   - Búsqueda por texto
   - Paginación (client o server)
2. **Crear ticket**
   - Form con validaciones (mínimo: required + longitudes)
   - Mensajes de error UX-friendly
3. **Detalle de ticket**
   - Ver datos + comentarios
   - Agregar comentario
   - Cambiar estado (Open → InProgress → Resolved → Closed)
4. **Manejo de errores**
   - Estado de carga (spinner/skeleton)
   - Manejo de `404/400/500` con mensaje apropiado
5. **Responsive**
   - Buen comportamiento en móvil (Bootstrap o CSS Grid/Flex)

## Requerimientos técnicos

### Si eliges Angular

- TypeScript
- Estructura recomendada:
  - `core/` (servicios, interceptores, guards si aplican)
  - `shared/` (componentes reutilizables)
  - `features/tickets/` (módulo o standalone components)
- Servicio `TicketsService` con HttpClient
- Interceptor opcional para trazabilidad (ej. `X-User`) y manejo centralizado de errores (suma puntos)

### Si eliges React

- Componentes funcionales + hooks
- Manejo de estado adecuado (useState/useReducer o librería si justificas)
- Separación por capas:
  - `api/` (cliente HTTP)
  - `features/tickets/`
  - `components/`
- Manejo de formularios (nativo o librería) con validación clara

## Criterios UI/UX evaluados

- Jerarquía visual, consistencia, accesibilidad básica (labels, focus)
- Mensajes claros, sin `alert()` como solución principal
- Evitar “pantallas en blanco” sin feedback

## Entregables

- Código + README
- Capturas en `/docs/ui-screenshots/` (opcional, suma puntos)
- `/docs/frontend-notes.md` con:
  - Cómo corre
  - Estructura de carpetas
  - 3 mejoras futuras (ej. caching, optimistic updates, etc.)


---

# 3) Base de Datos — SQL Server  o SQL Lite


## Parte A — Modelado lógico simple 

Diseña un modelo para:

- Ticket
- Comment
- User (mínimo: Id, Email, DisplayName)

Requisitos:

- 1 Ticket tiene N Comments
- Ticket tiene CreatedBy (User)
- Comment tiene CreatedBy (User)

**Entrega:** DDL (`CREATE TABLE`) + claves primarias/foráneas + tipos adecuados.

## Parte B — Consultas 

Con el modelo anterior, escribe consultas para:

1. **Listado paginado** de tickets por `Status` y `Priority`, incluyendo:
   - total de comentarios por ticket
   - nombre del creador
2. **Top 5** usuarios que más tickets crearon en el último mes
3. Buscar tickets donde `q` aparezca en Title o Description (case-insensitive si aplica)
4. Tickets “atrasados”: creados hace más de X días y NO cerrados

> Debes usar **JOINs**, **GROUP BY** y paginación (OFFSET/FETCH o equivalente Oracle).

## Parte C — Performance básico 

1. Propón **2 índices** (explica por qué)
2. Explica cómo validarías mejora (plan de ejecución, IO, estadísticas)
3. Identifica un posible antipatrón (ej. `SELECT *`, funciones en columnas filtradas, falta de índice en FK)

---

# 4) Caso de Análisis (Escenario negocio + decisiones) 

## Escenario

Una empresa de logística quiere un módulo de “**Gestión de Incidencias**” integrado con sistemas existentes:

- Los operadores crean incidencias cuando una entrega falla.
- Supervisores cambian estados y asignan responsables.
- Se requiere historial de cambios (auditoría).
- Hay picos de 10.000 incidencias/día.
- Existe un portal legado en Java; debes **interpretar su estructura** para integrarte (solo lectura).

### Restricciones

- Debe exponerse una API para front web y para consumo de terceros.
- Sistema debe ser “secure-by-default” (mínimo: autenticación, autorización por rol, validación).
- Se trabaja en **Scrum** con releases quincenales.

## Preguntas / Entregables

### A) Requerimientos (5–10 ítems) 

- Redacta requerimientos funcionales y no funcionales
- Identifica supuestos y preguntas abiertas (mínimo 5)

### B) Propuesta técnica 

Incluye:

- Arquitectura en alto nivel (diagrama simple o descripción por componentes)
- Capas / módulos en .NET y front
- Estrategia de persistencia y auditoría
- Estrategia de integración con sistema legado (API Gateway, colas, batch, etc.)
- Manejo de errores y observabilidad (logs, métricas, trazas)

### C) Decisiones de arquitectura (ADR mini) 

Escribe **2 ADRs** (máx. 10 líneas cada uno):

- Ej: “EF Core vs Dapper”, “Monolito modular vs microservicios”, “JWT vs cookies”.

Incluye: contexto, decisión, consecuencias.

### D) Enfoque ágil 

- 1 sprint plan (2 semanas):
  - 6–10 user stories (formato: Como [rol] quiero [capacidad] para [beneficio])
  - Criterios de aceptación para 2 historias
  - Estimación relativa (S/M/L o story points)
  - Riesgos (mínimo 3)


---