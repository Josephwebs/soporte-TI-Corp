using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.API.Application.DTOs;
using TicketManager.API.Application.Services;
using TicketManager.API.Domain.Enums;
using TicketManager.API.Infrastructure;
using Xunit;

namespace TicketManager.Tests;

public class TicketServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TicketService _service;

    public TicketServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new AppDbContext(options);
        _service = new TicketService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateTicketAsync_ShouldCreateAndReturnTicket()
    {
        // Arrange
        var dto = new CreateTicketDto
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = Priority.High
        };

        // Act
        var result = await _service.CreateTicketAsync(dto, "testuser@domain.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Ticket", result.Title);
        Assert.Equal("High", result.Priority);
        Assert.Equal("testuser@domain.com", result.CreatedBy);
        Assert.Equal("Open", result.Status); // Debería inicializar en Open
    }

    [Fact]
    public async Task ChangeTicketStatusAsync_ToClosed_ShouldUpdateStatus()
    {
        // Arrange
        var dto = new CreateTicketDto { Title = "T", Description = "D", Priority = Priority.Low };
        var created = await _service.CreateTicketAsync(dto, "u");

        var statusDto = new ChangeStatusDto { Status = Status.Closed };

        // Act
        var result = await _service.ChangeTicketStatusAsync(created.Id, statusDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Closed", result.Status);
    }

    [Fact]
    public async Task ChangeTicketStatusAsync_WhenAlreadyClosed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dto = new CreateTicketDto { Title = "T", Description = "D", Priority = Priority.Low };
        var created = await _service.CreateTicketAsync(dto, "u");

        var closeDto = new ChangeStatusDto { Status = Status.Closed };
        await _service.ChangeTicketStatusAsync(created.Id, closeDto);

        var reopenDto = new ChangeStatusDto { Status = Status.Open };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.ChangeTicketStatusAsync(created.Id, reopenDto));
    }
}
