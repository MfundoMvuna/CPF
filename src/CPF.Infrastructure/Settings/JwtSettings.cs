namespace CPF.Infrastructure.Settings;

public class JwtSettings
{
    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = "CPF-API";
    public string Audience { get; set; } = "CPF-Mobile";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
