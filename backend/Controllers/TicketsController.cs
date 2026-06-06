using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManager.API.Application.DTOs;
using TicketManager.API.Application.Services;
using System.Security.Claims;

namespace TicketManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protegido por defecto
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    private string GetCurrentUser()
    {
        // Seguro: extraemos identidad validada del token JWT
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == ClaimTypes.Name || c.Type == "sub")?.Value 
            ?? "anonymous@domain.com";
    }

    /// <summary>
    /// Obtiene un listado paginado de los tickets del sistema.
    /// </summary>
    /// <param name="status">Filtrar por estado del ticket (ej. Open, Closed)</param>
    /// <param name="priority">Filtrar por prioridad (ej. Low, High)</param>
    /// <param name="q">Término de búsqueda en título o descripción</param>
    /// <param name="page">Número de página (default 1)</param>
    /// <param name="pageSize">Cantidad de registros por página (default 10)</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<TicketDto>>> GetTickets([FromQuery] string? status, [FromQuery] string? priority, [FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _ticketService.GetTicketsAsync(status, priority, q, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene los detalles de un ticket específico a través de su Id.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> GetTicket(Guid id)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id);
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }

    /// <summary>
    /// Crea un nuevo ticket de soporte.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] CreateTicketDto dto)
    {
        var ticket = await _ticketService.CreateTicketAsync(dto, GetCurrentUser());
        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
    }

    /// <summary>
    /// Actualiza los campos principales de un ticket.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> UpdateTicket(Guid id, [FromBody] UpdateTicketDto dto)
    {
        var ticket = await _ticketService.UpdateTicketAsync(id, dto);
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }

    /// <summary>
    /// Cambia el estado actual del ticket (Ej. Abierto a Cerrado). Requiere rol Supervisor.
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Supervisor")] // Regla de negocio: solo supervisores pueden cambiar estado
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketDto>> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
    {
        // El Error 409 y 500 ahora los maneja el ExceptionMiddleware globalmente en caso de InvalidOperationException
        var ticket = await _ticketService.ChangeTicketStatusAsync(id, dto);
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }

    [HttpGet("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(Guid id)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id);
        if (ticket == null) return NotFound();

        var comments = await _ticketService.GetCommentsAsync(id);
        return Ok(comments);
    }

    [HttpPost("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommentDto>> AddComment(Guid id, [FromBody] CreateCommentDto dto)
    {
        var comment = await _ticketService.AddCommentAsync(id, dto, GetCurrentUser());
        if (comment == null) return NotFound();
        return CreatedAtAction(nameof(GetComments), new { id = id }, comment);
    }
}
