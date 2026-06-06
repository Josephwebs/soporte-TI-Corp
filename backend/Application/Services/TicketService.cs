using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.API.Application.DTOs;
using TicketManager.API.Domain.Entities;
using TicketManager.API.Domain.Enums;
using TicketManager.API.Infrastructure;

namespace TicketManager.API.Application.Services;

public interface ITicketService
{
    Task<PaginatedResult<TicketDto>> GetTicketsAsync(string? status, string? priority, string? q, int page, int pageSize);
    Task<TicketDto?> GetTicketByIdAsync(Guid id);
    Task<TicketDto> CreateTicketAsync(CreateTicketDto dto, string createdBy);
    Task<TicketDto?> UpdateTicketAsync(Guid id, UpdateTicketDto dto);
    Task<TicketDto?> ChangeTicketStatusAsync(Guid id, ChangeStatusDto dto);
    Task<IEnumerable<CommentDto>> GetCommentsAsync(Guid ticketId);
    Task<CommentDto?> AddCommentAsync(Guid ticketId, CreateCommentDto dto, string createdBy);
}

public class TicketService : ITicketService
{
    private readonly AppDbContext _context;

    public TicketService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<TicketDto>> GetTicketsAsync(string? status, string? priority, string? q, int page, int pageSize)
    {
        var query = _context.Tickets.AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<Status>(status, true, out var s))
        {
            query = query.Where(t => t.Status == s);
        }

        if (!string.IsNullOrEmpty(priority) && Enum.TryParse<Priority>(priority, true, out var p))
        {
            query = query.Where(t => t.Priority == p);
        }

        if (!string.IsNullOrEmpty(q))
        {
            query = query.Where(t => t.Title.Contains(q) || t.Description.Contains(q));
        }

        var totalCount = await query.CountAsync();
        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Priority = t.Priority.ToString(),
                Status = t.Status.ToString(),
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CreatedBy = t.CreatedBy
            })
            .ToListAsync();

        return new PaginatedResult<TicketDto> { TotalCount = totalCount, Items = tickets };
    }

    public async Task<TicketDto?> GetTicketByIdAsync(Guid id)
    {
        var t = await _context.Tickets.FindAsync(id);
        if (t == null) return null;

        return new TicketDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Priority = t.Priority.ToString(),
            Status = t.Status.ToString(),
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            CreatedBy = t.CreatedBy
        };
    }

    public async Task<TicketDto> CreateTicketAsync(CreateTicketDto dto, string createdBy)
    {
        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = Status.Open,
            CreatedBy = createdBy
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        return await GetTicketByIdAsync(ticket.Id) ?? throw new Exception("Error al crear el ticket");
    }

    public async Task<TicketDto?> UpdateTicketAsync(Guid id, UpdateTicketDto dto)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return null;

        if (!string.IsNullOrEmpty(dto.Title)) ticket.Title = dto.Title;
        if (!string.IsNullOrEmpty(dto.Description)) ticket.Description = dto.Description;
        if (dto.Priority.HasValue) ticket.Priority = dto.Priority.Value;

        ticket.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetTicketByIdAsync(id);
    }

    public async Task<TicketDto?> ChangeTicketStatusAsync(Guid id, ChangeStatusDto dto)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return null;

        // Validar transiciones de estado aquí si es necesario
        // ej: if (ticket.Status == Status.Closed) throw Exception("Cannot reopen");
        if (ticket.Status == Status.Closed && dto.Status != Status.Closed)
        {
            throw new InvalidOperationException("No se puede cambiar el estado de un ticket cerrado.");
        }

        ticket.Status = dto.Status;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetTicketByIdAsync(id);
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsAsync(Guid ticketId)
    {
        var exists = await _context.Tickets.AnyAsync(t => t.Id == ticketId);
        if (!exists) return Enumerable.Empty<CommentDto>();

        return await _context.Comments
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Text = c.Text,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy
            })
            .ToListAsync();
    }

    public async Task<CommentDto?> AddCommentAsync(Guid ticketId, CreateCommentDto dto, string createdBy)
    {
        var exists = await _context.Tickets.AnyAsync(t => t.Id == ticketId);
        if (!exists) return null;

        var comment = new Comment
        {
            TicketId = ticketId,
            Text = dto.Text,
            CreatedBy = createdBy
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            CreatedBy = comment.CreatedBy
        };
    }
}
