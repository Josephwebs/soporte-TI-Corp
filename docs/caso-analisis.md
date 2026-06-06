# Caso de Análisis: Gestión de Incidencias

A continuación, expongo mi propuesta arquitectónica integral para solventar la modernización del sistema de Gestión de Incidencias. El diseño está enfocado en garantizar escalabilidad frente a altos volúmenes transaccionales (10k incidencias diarias) y asegurar la interoperabilidad asíncrona con los sistemas legados existentes.

## A) Análisis de Requerimientos

### Requerimientos Funcionales
1. El sistema debe permitir a los operadores registrar, editar y consultar incidencias relacionadas con entregas fallidas de manera expedita.
2. Los supervisores requerirán permisos jerárquicos para transicionar el ciclo de vida de los tickets (ej. Abierto -> Asignado -> Resuelto).
3. Debe existir una funcionalidad para la asignación y seguimiento de responsables por incidencia.
4. **Trazabilidad (Auditoría):** Todo evento de mutación de datos debe quedar registrado de forma inmutable, identificando el actor, la estampa de tiempo y los deltas de información modificada.
5. El sistema deberá establecer un flujo de lectura hacia el portal corporativo legado en Java para enriquecer las incidencias con metadatos de entregas.

### Requerimientos No Funcionales
1. **Seguridad:** Implementación de autenticación centralizada mediante tokens (JWT) y autorización por roles (RBAC).
2. **Rendimiento Operativo:** El backend deberá soportar un caudal base de 10,000 transacciones diarias, garantizando latencias API inferiores a 300ms (P95).
3. **Escalabilidad y Disponibilidad:** La arquitectura debe aprovisionarse para absorber picos de concurrencia propios del horario logístico comercial sin degradación.
4. **Interoperabilidad:** Diseño "API-First", garantizando una especificación RESTful estricta para futuras integraciones de terceros.

### Supuestos Arquitectónicos
1. Se asume que el sistema legado en Java expone una capa de servicios básicos (REST o SOAP). En caso contrario, se procedería con estrategias de integración por colas o CDC a nivel de base de datos.
2. Se asumirá un Identity Provider estándar para la emisión de los JWT (por simplicidad teórica).
3. Falta definir los umbrales específicos de SLA para determinar el "Time-to-Live" (TTL) de la data caliente antes de su archivo en almacenamiento secundario.

## B) Propuesta Técnica (Infraestructura y Componentes)

- **Capa Cliente:** Desarrollo de una SPA con React, aprovechando el renderizado estático/cliente para servir los *assets* desde un CDN, minimizando el costo computacional del frontend.
- **Capa Edge (API Gateway):** Despliegue de un Gateway proxy responsable de delegar cargas transversales como la terminación SSL, Rate Limiting, y validación preliminar de firmas JWT.
- **Microservicios (Backend):** Implementación de una API RESTful en .NET 8 con Clean Architecture, *containerizada* con Docker para permitir una rápida orquestación en Kubernetes o App Services según el modelo de despliegue.
- **Capa de Integración (Anti-Corruption Layer):** Para interactuar con el sistema en Java, estructuraremos un cliente HTTP con resiliencia garantizada mediante el ecosistema `Polly` (implementando patrones de *Retry* y *Circuit Breaker*) para mitigar caídas en cascada.
- **Persistencia de Datos:** Utilización de SQL Server con EF Core para garantizar consistencia transaccional (ACID) en la operatividad diaria de las incidencias.
- **Observabilidad:** Integración de Middleware para el reporte estandarizado de excepciones (ProblemDetails) y la inserción de *Correlation IDs* en cada request, facilitando el rastreo de flujos distribuidos en plataformas como Application Insights o el stack ELK.

## C) Decisiones de Arquitectura (ADR)

### ADR 1: Entity Framework Core vs Micro-ORMs (Dapper)
- **Contexto:** Manejaremos un volumen estimado de 10,000 incidencias al día. Se requiere un balance preciso entre *Time-to-Market* y rendimiento de I/O.
- **Decisión:** He decidido utilizar **EF Core** como motor principal para mutaciones complejas (Commands) y mantener abierta la vía para implementar **Dapper** de manera ad-hoc únicamente en consultas analíticas de alto consumo (Queries), formando un modelo CQRS ligero.
- **Consecuencias:** Aceleramos significativamente el ciclo de desarrollo por el *Change Tracking* de EF. Para contrarrestar posibles latencias de mapeo masivo, se forzará la desactivación del rastreo (`AsNoTracking`) en todas las consultas de solo lectura.

### ADR 2: Estrategia de Auditoría Invasiva vs Pasiva
- **Contexto:** Existe la necesidad de registrar un historial riguroso de cada cambio sin acoplar la lógica de guardado de logs directamente en las clases de servicio.
- **Decisión:** Implementaré **Interceptors** nativos de EF Core (`SaveChangesInterceptor`). Esto permitirá capturar las entidades modificadas justo antes del commit, serializar un delta en JSON y volcarlo a una tabla paralela de `AuditLogs`.
- **Consecuencias:** Mantenemos la capa de reglas de negocio completamente agnóstica de los requisitos de auditoría. Si bien el proceso de serialización incurrirá en un ínfimo impacto por transacción, el beneficio a nivel de pureza arquitectónica y seguridad de los datos lo justifica por completo.

## D) Planificación Ágil de Implementación (Sprint de 2 semanas)

### Backlog Propuesto (User Stories)
1. Como Operador de Logística, quiero registrar una nueva incidencia de entrega fallida para iniciar el protocolo de revisión. *(Prioridad: Alta)*
2. Como Operador, quiero visualizar una grilla paginada con el estado actual de mis casos registrados. *(Prioridad: Alta)*
3. Como Supervisor, quiero tener acceso al tablero consolidado de todas las incidencias abiertas a nivel regional. *(Prioridad: Media)*
4. Como Supervisor, quiero asignar explícitamente un técnico responsable a cada incidencia. *(Prioridad: Media)*
5. Como Supervisor, quiero contar con la capacidad de modificar el estado transicional de una incidencia. *(Prioridad: Alta)*
6. Como Auditor, quiero revisar la línea de tiempo inmutable de modificaciones de una incidencia específica. *(Prioridad: Media)*
7. Como Sistema, requiero un servicio programado que sincronice los metadatos de entregas desde el portal Java heredado. *(Prioridad: Baja)*

### Criterios de Aceptación Técnicos
- **Historia 1 (Crear Incidencia):**
  - CA1: Las validaciones de esquema (longitud y presencia) deben ejecutarse tanto en frontend como en backend.
  - CA2: El registro insertado debe forzar el campo 'Estado' a 'Abierto', mitigando intentos de manipulación del cliente.
  - CA3: El evento de creación debe disparar de forma automática el interceptor de auditoría.
- **Historia 5 (Cambiar Estado):**
  - CA1: El endpoint debe rechazar peticiones de usuarios que carezcan del Claim "Supervisor".
  - CA2: Se debe validar en la capa de negocio que el flujo del estado siga un autómata válido (Ej. Rechazar salto de "Abierto" a "Cerrado" directamente).

### Evaluación de Riesgos
1. **Latencia Incontrolada (Terceros):** Retrasos crónicos de red en la integración con el sistema Java, lo cual requerirá un ajuste riguroso del Circuit Breaker.
2. **Degradación de Base de Datos (Auditoría):** El crecimiento exponencial de la tabla `AuditLogs`. Se propone mitigar este riesgo configurando jobs programados para migrar datos históricos a un storage económico tras 12 meses.
3. **Vulnerabilidades de Supply Chain:** Riesgo de adopción inadvertida de paquetes NPM/NuGet con código malicioso. Se integrará una herramienta de escaneo estático tipo Dependabot en el ciclo de Integración Continua (CI) como barrera perimetral.
