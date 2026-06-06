# Prueba Técnica Fullstack: Sistema de Gestión de Tickets (Helpdesk)

Este repositorio contiene mi solución completa para la prueba técnica. Desarrollé un backend transaccional en .NET 8, un frontend moderno y altamente optimizado en React + TypeScript, y el modelado completo de bases de datos relacionales en SQL.

A lo largo del proyecto, tomé decisiones de arquitectura orientadas a la mantenibilidad y no tuve reservas en apalancarme de tecnologías modernas y asistentes de Inteligencia Artificial para garantizar los más altos estándares de calidad.

## Estructura del Repositorio

- `/backend`: API RESTful construida con .NET 8 siguiendo principios de Clean Architecture.
- `/frontend`: SPA (Single Page Application) en React + Vite + Chakra UI.
- `/db`: Scripts DDL (`schema.sql`) y consultas DML avanzadas (`queries.sql`) además de análisis de performance de índices (`performance.md`).
- `/docs`: Documentación técnica (ADRs), notas de frontend y el caso teórico de análisis de arquitectura y escalabilidad.

## Enfoque de Desarrollo y Calidad (Highlights)

1. **Dominio de UI/UX y CSS (Chakra UI + Framer Motion):**
   Para la interfaz, decidí alejarme de soluciones genéricas y armar un sistema visual robusto utilizando **Chakra UI**. Combiné esto con **Framer Motion** (implementando `LazyMotion` para no castigar el bundle de JS) para dotar a la aplicación de micro-interacciones y animaciones fluidas (modales, validaciones de formularios y transiciones). El objetivo fue entregar una UI que no solo funcione, sino que transmita la calidad de un producto "Premium".
2. **Validación Estática y Seguridad Asistida por IA:**
   Para asegurar que el código estuviera impecable, integré **React Doctor** en el flujo de trabajo. Esta herramienta de análisis estático escaneó la aplicación contra más de 100 reglas arquitectónicas (identificando renders innecesarios y dependencias fantasmas). 
   Adicionalmente, integré **agentes de Inteligencia Artificial** como parte de mi proceso de revisión de código (Code Review) para auditar la seguridad del sitio contra normativas estándar de la industria (ISO). Lejos de ocultar el uso de IA, lo considero un paso fundamental y mandatorio en el ciclo DevSecOps moderno de cualquier desarrollador Senior para mitigar riesgos antes del despliegue.

## Requisitos Previos
- **Backend:** .NET 8 SDK
- **Frontend:** Node.js (v18+)
- **Base de Datos:** Ninguna requerida para probar la app. La aplicación arranca utilizando una base de datos In-Memory pre-cargada con datos de prueba (Seed Data) para facilitar la evaluación inmediata por parte del examinador. Los scripts SQL formales se encuentran en la carpeta `/db`.

## Instrucciones de Ejecución

### 1. Ejecutar la API (.NET)
```bash
cd backend
dotnet build
dotnet run
```
La API estará disponible en: `http://localhost:5180`. 
Puedes acceder a la documentación interactiva de **Swagger** navegando a `http://localhost:5180/swagger`.

### 2. Ejecutar la App Web (React)
Abre una nueva terminal:
```bash
cd frontend
npm install
npm run dev
```
La aplicación web estará disponible en `http://localhost:5173`. 
*(Nota: El frontend realiza un auto-login transparente contra el endpoint `/api/auth/login` para inyectarse a sí mismo un Token JWT con rol de Supervisor, permitiendo evaluar todo el ciclo de vida de los tickets sin fricción).*

## Endpoints Principales

- `GET /api/tickets`: Obtiene el listado de tickets paginado. (Soporta `?q=`, `?status=`, `?priority=`).
- `GET /api/tickets/{id}`: Obtiene un ticket específico.
- `POST /api/tickets`: Crea un nuevo ticket.
- `PATCH /api/tickets/{id}/status`: Cambia el estado (Requiere Autenticación de Rol).
- `GET /api/tickets/{id}/comments`: Lista los comentarios de un ticket.
- `POST /api/tickets/{id}/comments`: Añade un comentario a un ticket.
