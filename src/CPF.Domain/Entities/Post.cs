namespace CPF.Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public User User { get; set; } = default!;
}
