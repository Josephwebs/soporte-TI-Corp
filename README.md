# Prueba Técnica Fullstack: Sistema de Gestión de Tickets (Helpdesk)

Este repositorio contiene la solución completa para la prueba técnica solicitada, que consiste en un backend robusto en .NET 8, un frontend moderno en React + TypeScript y el modelado de bases de datos relacionales en SQL.

## Estructura del Repositorio

- `/backend`: API RESTful construida con .NET 8 siguiendo principios de Clean Architecture.
- `/frontend`: SPA (Single Page Application) en React + Vite + Material UI (MUI).
- `/db`: Scripts DDL (`schema.sql`) y consultas DML avanzadas (`queries.sql`) además de análisis de performance de índices (`performance.md`).
- `/docs`: Documentación técnica que incluye decisiones de arquitectura, notas del frontend y el caso teórico de análisis de escalabilidad.

## Requisitos Previos
- **Backend:** .NET 8 SDK
- **Frontend:** Node.js (v18+)
- **Base de Datos:** Ninguna requerida para probar la app. La aplicación arranca utilizando una base de datos In-Memory pre-cargada con datos de prueba (Seed Data) para facilitar la evaluación inmediata por parte del examinador. Los scripts SQL formales se encuentran documentados en la carpeta `/db`.

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
