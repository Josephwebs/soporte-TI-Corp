# TicketManager Frontend - Notas de Desarrollo

En este documento expongo la estructura técnica seleccionada para la capa de presentación y algunas consideraciones de diseño que apuntan a la mantenibilidad del sistema.

## Instrucciones de ejecución
Para levantar el entorno de desarrollo local, seguir los siguientes pasos:
1. Acceder al directorio: `cd frontend`
2. Instalar dependencias: `npm install`
3. Iniciar el servidor: `npm run dev`
4. *Nota:* Asegúrese de tener la API de .NET en ejecución de forma simultánea en `http://localhost:5180`.

## Diseño de la Arquitectura
El proyecto fue construido utilizando React 19, TypeScript y Vite. Para organizar el código fuente, adopté una estructura modular alineada con principios de Clean Architecture para UI:
- `/src/api/`: Centraliza la configuración del cliente HTTP (Axios) y define los interceptores necesarios para inyectar automáticamente los tokens JWT en las solicitudes.
- `/src/components/`: Almacena componentes de presentación puros y reutilizables, como los modales de creación y detalle.
- `/src/features/`: Agrupa componentes "inteligentes" organizados por dominio funcional (ej. `tickets/TicketList.tsx`). Esto facilita la escalabilidad horizontal del código a medida que se añaden nuevos módulos de negocio.
- `/src/types/`: Centraliza las interfaces de TypeScript, las cuales están estrechamente alineadas con los DTOs del backend para asegurar un tipado fuerte de extremo a extremo.

## Mejoras Técnicas Propuestas (Roadmap)
Si el proyecto evolucionara hacia una fase productiva, priorizaría las siguientes implementaciones:

1. **Paginación Dinámica del Lado del Servidor:** Aunque la API subyacente soporta paginación (`?page=&pageSize=`), la interfaz actual presenta los resultados iniciales por defecto. La integración de una tabla virtualizada (como `MUI DataGrid`) permitiría navegar eficientemente por historiales extensos sin degradar la memoria del navegador.
2. **Actualizaciones Optimistas (Optimistic UI):** Sugiero la integración de `React Query` para la gestión del estado asíncrono. Esto nos permitiría reflejar los cambios de estado (ej. al cerrar un ticket) instantáneamente en la interfaz de usuario, resolviendo la validación HTTP en segundo plano.
3. **Validaciones Centralizadas y Control de Eventos:** Extraer la lógica de validación de `Yup` hacia un archivo de esquemas compartidos. Adicionalmente, implementar funciones de *Debounce* en las cajas de búsqueda de texto para reducir la carga de peticiones al servidor.
4. **Seguridad en la Cadena de Suministro (DevSecOps):** Aplicar "Dependency Pinning" en el archivo `package.json` para fijar las versiones exactas de las librerías, y configurar herramientas de análisis de composición de software (como Dependabot) en el pipeline de CI/CD para monitorear vulnerabilidades.
