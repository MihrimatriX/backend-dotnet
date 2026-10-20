using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Domain.Entities;

public sealed class OutboxMessage : BaseEntity
{
    [Required]
    [StringLength(300)]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Payload { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAtUtc { get; set; }

    public int Attempts { get; set; } = 0;

    [StringLength(2000)]
    public string? LastError { get; set; }
}

