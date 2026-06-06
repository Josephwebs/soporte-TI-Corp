-- queries.sql

-- 1. Listado paginado de tickets por Status y Priority, incluyendo total de comentarios y nombre del creador
-- Ejemplo: Status = 'Open', Priority = 'High', Page = 1, PageSize = 10
DECLARE @Status NVARCHAR(20) = 'Open';
DECLARE @Priority NVARCHAR(20) = 'High';
DECLARE @PageNumber INT = 1;
DECLARE @PageSize INT = 10;

SELECT 
    t.Id,
    t.Title,
    t.Status,
    t.Priority,
    t.CreatedAt,
    
    u.DisplayName AS CreatorName,
    (SELECT COUNT(*) FROM Comments c WHERE c.TicketId = t.Id) AS TotalComments
FROM Tickets t
JOIN Users u ON t.CreatedById = u.Id
WHERE t.Status = @Status AND t.Priority = @Priority
ORDER BY t.CreatedAt DESC
OFFSET (@PageNumber - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;

-- 2. Top 5 usuarios que más tickets crearon en el último mes
SELECT TOP 5
    u.Id,
    u.DisplayName,
    u.Email,
    COUNT(t.Id) AS TicketsCreated
FROM Users u
JOIN Tickets t ON u.Id = t.CreatedById
WHERE t.CreatedAt >= DATEADD(MONTH, -1, GETDATE())
GROUP BY u.Id, u.DisplayName, u.Email
ORDER BY TicketsCreated DESC;

-- 3. Buscar tickets donde `q` aparezca en Title o Description (case-insensitive si aplica)
DECLARE @q NVARCHAR(100) = 'error';
SELECT 
    Id, Title, Description, Status, Priority, CreatedAt
FROM Tickets
WHERE Title LIKE '%' + @q + '%' OR Description LIKE '%' + @q + '%';

-- 4. Tickets “atrasados”: creados hace más de X días y NO cerrados
DECLARE @Days INT = 7;
SELECT 
    Id, Title, Status, CreatedAt
FROM Tickets
WHERE Status != 'Closed' 
AND CreatedAt < DATEADD(DAY, -@Days, GETDATE());
