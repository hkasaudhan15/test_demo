namespace CleanArch.Application.Abstractions.Notifications;

/// <summary>
/// Email service abstraction.
/// </summary>
public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    Task SendTemplatedAsync(string to, string templateName, Dictionary<string, string> parameters, CancellationToken ct = default);
    Task SendBulkAsync(IEnumerable<string> recipients, string subject, string htmlBody, CancellationToken ct = default);
}

/// <summary>
/// Push notification abstraction.
/// </summary>
public interface IPushNotificationService
{
    Task SendToUserAsync(string userId, string title, string body, Dictionary<string, string>? data = null, CancellationToken ct = default);
    Task SendToTopicAsync(string topic, string title, string body, CancellationToken ct = default);
}
