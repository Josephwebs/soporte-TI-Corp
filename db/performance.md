# Rendimiento Básico

## 1. Índices propuestos
1. **Índice en `Tickets(CreatedById)`:**
   - **Razón:** La clave foránea `CreatedById` se usa frecuentemente en JOINs (por ejemplo, para traer el creador de un ticket o para contar los tickets por usuario). Un índice aquí acelerará estas operaciones.
2. **Índice compuesto en `Tickets(Status, Priority) INCLUDE (CreatedAt)`:**
   - **Razón:** El listado principal de tickets permite filtrar por `Status` y `Priority` de manera combinada, y usualmente ordena por `CreatedAt`. Este índice cubre la consulta para el listado principal, evitando un escaneo completo de la tabla.

## 2. Validación de mejora
- **Plan de ejecución:** Compararía el plan de ejecución antes y después de aplicar el índice en SQL Server Management Studio (SSMS), buscando reducciones en "Table Scans" o "Clustered Index Scans" a favor de "Index Seeks".
- **Estadísticas de E/S y Tiempo:** Activaría `SET STATISTICS IO ON` y `SET STATISTICS TIME ON` para medir la reducción de lecturas lógicas (logical reads) y el tiempo de CPU al ejecutar las consultas más pesadas.

## 3. Posible antipatrón
- **Antipatrón:** Uso de `LIKE '%termino%'` (wildcard al inicio) para la búsqueda por texto.
- **Explicación:** Un comodín al inicio de la cadena impide que el motor de base de datos utilice índices estándar tipo B-Tree, forzando un escaneo completo de la tabla (Index Scan o Table Scan), lo cual degrada severamente el rendimiento en tablas con miles o millones de registros.
- **Solución:** Implementar **Full-Text Search** en SQL Server para búsquedas eficientes sobre `Title` y `Description`.
