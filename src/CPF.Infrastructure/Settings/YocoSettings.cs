namespace CPF.Infrastructure.Settings;

public class YocoSettings
{
    public string SecretKey { get; set; } = default!;
    public string WebhookSecret { get; set; } = default!;
    public string BaseUrl { get; set; } = "https://payments.yoco.com/api";
}
