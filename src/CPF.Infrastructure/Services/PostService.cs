using CPF.Application.Interfaces;
using CPF.Domain.Entities;
using CPF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPF.Infrastructure.Services;

public class PostService : IPostService
{
    private readonly CpfDbContext _db;

    public PostService(CpfDbContext db) => _db = db;

    public async Task<IReadOnlyList<PostDto>> GetPostsAsync(int page, int pageSize)
    {
        return await _db.Posts
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostDto(
                p.Id,
                p.UserId,
                p.User.FullName,
                p.Title,
                p.Content,
                p.ImageUrl,
                p.CreatedAtUtc))
            .ToListAsync();
    }

    public async Task<PostDto> CreatePostAsync(Guid userId, string title, string content, string? imageUrl)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Content = content,
            ImageUrl = imageUrl
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        return new PostDto(post.Id, post.UserId, user.FullName, post.Title, post.Content, post.ImageUrl, post.CreatedAtUtc);
    }
}
