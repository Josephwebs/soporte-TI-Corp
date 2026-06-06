using System;
using System.ComponentModel.DataAnnotations;
using TicketManager.API.Domain.Enums;

namespace TicketManager.API.Application.DTOs;

public class TicketDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateTicketDto
{
    [Required, StringLength(120, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Priority Priority { get; set; }
}

public class UpdateTicketDto
{
    [StringLength(120, MinimumLength = 5)]
    public string? Title { get; set; }

    [StringLength(2000, MinimumLength = 10)]
    public string? Description { get; set; }

    public Priority? Priority { get; set; }
}

public class ChangeStatusDto
{
    [Required]
    public Status Status { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateCommentDto
{
    [Required, StringLength(1000, MinimumLength = 2)]
    public string Text { get; set; } = string.Empty;
}

public class PaginatedResult<T>
{
    public int TotalCount { get; set; }
    public IEnumerable<T> Items { get; set; } = new List<T>();
}
