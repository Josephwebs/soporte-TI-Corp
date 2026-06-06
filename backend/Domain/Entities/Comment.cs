using System;

namespace TicketManager.API.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TicketId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;

    public Ticket Ticket { get; set; } = null!;
}
