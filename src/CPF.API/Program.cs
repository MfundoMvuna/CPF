using System.Security.Claims;
using CPF.Application.DTOs;
using CPF.Application.Interfaces;
using CPF.Infrastructure;
using CPF.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Lambda hosting — seamlessly runs as Lambda or Kestrel
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

// Register all infrastructure services (DB, JWT, SNS, Yoco, etc.)
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

// Auto-create database schema on startup (dev only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CpfDbContext>();
    db.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

// ──────────────────────────────────────────────
// Health check
// ──────────────────────────────────────────────
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// ──────────────────────────────────────────────
// Auth endpoints
// ──────────────────────────────────────────────
app.MapPost("/api/auth/register", async (RegisterRequest request, IAuthService auth) =>
{
    var result = await auth.RegisterAsync(request.FullName, request.Email, request.Password, request.PhoneNumber);
    return result.Success
        ? Results.Ok(new { result.AccessToken, result.RefreshToken })
        : Results.BadRequest(new { result.Error });
});

app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService auth) =>
{
    var result = await auth.LoginAsync(request.Email, request.Password);
    return result.Success
        ? Results.Ok(new { result.AccessToken, result.RefreshToken })
        : Results.Unauthorized();
});

app.MapPost("/api/auth/refresh", async (RefreshTokenRequest request, IAuthService auth) =>
{
    var result = await auth.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
    return result.Success
        ? Results.Ok(new { result.AccessToken, result.RefreshToken })
        : Results.Unauthorized();
});

// ──────────────────────────────────────────────
// Panic Alert endpoints
// ──────────────────────────────────────────────
app.MapPost("/api/panic/trigger", async (TriggerPanicRequest request, IPanicAlertService panic, ClaimsPrincipal user) =>
{
    var userId = GetUserId(user);
    var result = await panic.TriggerAsync(userId, request.Latitude, request.Longitude, request.Description);
    return result.Success
        ? Results.Ok(new { result.AlertId })
        : Results.BadRequest(new { result.Error });
})
.RequireAuthorization();

app.MapGet("/api/panic/active", async (IPanicAlertService panic) =>
{
    var alerts = await panic.GetActiveAlertsAsync();
    return Results.Ok(alerts);
})
.RequireAuthorization();

// ──────────────────────────────────────────────
// Payment endpoints
// ──────────────────────────────────────────────
app.MapPost("/api/payments/create", async (CreatePaymentRequest request, IPaymentService payments, ClaimsPrincipal user) =>
{
    var userId = GetUserId(user);
    var result = await payments.CreatePaymentAsync(userId, request.AmountInCents, request.Description);
    return result.Success
        ? Results.Ok(new { result.PaymentId, result.CheckoutUrl })
        : Results.BadRequest(new { result.Error });
})
.RequireAuthorization();

app.MapPost("/api/payments/webhook", async (HttpContext ctx, IPaymentService payments) =>
{
    // Read raw body for signature validation
    ctx.Request.EnableBuffering();
    using var reader = new StreamReader(ctx.Request.Body);
    var payload = await reader.ReadToEndAsync();
    var signature = ctx.Request.Headers["X-Yoco-Signature"].FirstOrDefault() ?? "";

    try
    {
        await payments.ProcessWebhookAsync(payload, signature);
        return Results.Ok();
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});

// ──────────────────────────────────────────────
// Shift endpoints
// ──────────────────────────────────────────────
app.MapGet("/api/shifts", async (IShiftService shifts, ClaimsPrincipal user) =>
{
    var userId = GetUserId(user);
    var result = await shifts.GetShiftsAsync(userId);
    return Results.Ok(result);
})
.RequireAuthorization();

app.MapPost("/api/shifts/{shiftId:guid}/checkin", async (Guid shiftId, [FromBody] CheckInRequest request, IShiftService shifts, ClaimsPrincipal user) =>
{
    var userId = GetUserId(user);
    var result = await shifts.CheckInAsync(shiftId, userId, request.Latitude, request.Longitude);
    return result.Success ? Results.Ok() : Results.BadRequest(new { result.Error });
})
.RequireAuthorization();

app.MapPost("/api/shifts/{shiftId:guid}/checkout", async (Guid shiftId, IShiftService shifts, ClaimsPrincipal user) =>
{
    var userId = GetUserId(user);
    var result = await shifts.CheckOutAsync(shiftId, userId);
    return result.Success ? Results.Ok() : Results.BadRequest(new { result.Error });
})
.RequireAuthorization();

// ──────────────────────────────────────────────
// Community Feed endpoints
// ──────────────────────────────────────────────
app.MapGet("/api/posts", async (IPostService posts, int page = 1, int pageSize = 20) =>
{
    pageSize = Math.Clamp(pageSize, 1, 50);
    var result = await posts.GetPostsAsync(page, pageSize);
    return Results.Ok(result);
})
.RequireAuthorization();

app.MapPost("/api/posts", async (CreatePostRequest request, IPostService posts, ClaimsPrincipal user) =>
{
    var userId = GetUserId(user);
    var result = await posts.CreatePostAsync(userId, request.Title, request.Content, request.ImageUrl);
    return Results.Created($"/api/posts/{result.Id}", result);
})
.RequireAuthorization();

app.Run();

// ──────────────────────────────────────────────
// Helper
// ──────────────────────────────────────────────
static Guid GetUserId(ClaimsPrincipal user)
{
    var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException("User ID claim not found.");
    return Guid.Parse(id);
}
