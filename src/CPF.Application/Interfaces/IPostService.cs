namespace CPF.Application.Interfaces;

public interface IPostService
{
    Task<IReadOnlyList<PostDto>> GetPostsAsync(int page, int pageSize);
    Task<PostDto> CreatePostAsync(Guid userId, string title, string content, string? imageUrl);
}

public record PostDto(
    Guid Id,
    Guid UserId,
    string UserFullName,
    string Title,
    string Content,
    string? ImageUrl,
    DateTime CreatedAtUtc);
