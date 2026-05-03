namespace CleanArch.Domain.Settings;

/// <summary>
/// Well-known setting keys organized by group.
/// Using constants prevents magic strings and enables compile-time safety.
///
/// Add new keys here as features grow. The settings module will seed
/// defaults for these keys on first migration.
/// </summary>
public static class SettingKeys
{
    public static class General
    {
        public const string Group = "General";
        public const string ApplicationName = "General.ApplicationName";
        public const string ApplicationUrl = "General.ApplicationUrl";
        public const string DefaultLanguage = "General.DefaultLanguage";
        public const string DefaultTimezone = "General.DefaultTimezone";
        public const string MaintenanceMode = "General.MaintenanceMode";
    }

    public static class Email
    {
        public const string Group = "Email";
        public const string SmtpHost = "Email.SmtpHost";
        public const string SmtpPort = "Email.SmtpPort";
        public const string SmtpUsername = "Email.SmtpUsername";
        public const string SenderName = "Email.SenderName";
        public const string SenderAddress = "Email.SenderAddress";
        public const string EnableSsl = "Email.EnableSsl";
    }

    public static class Security
    {
        public const string Group = "Security";
        public const string MaxLoginAttempts = "Security.MaxLoginAttempts";
        public const string LockoutDurationMinutes = "Security.LockoutDurationMinutes";
        public const string PasswordMinLength = "Security.PasswordMinLength";
        public const string RequireTwoFactor = "Security.RequireTwoFactor";
        public const string SessionTimeoutMinutes = "Security.SessionTimeoutMinutes";
        public const string AllowedFileExtensions = "Security.AllowedFileExtensions";
    }

    public static class Notification
    {
        public const string Group = "Notification";
        public const string EnableEmailNotifications = "Notification.EnableEmailNotifications";
        public const string EnablePushNotifications = "Notification.EnablePushNotifications";
        public const string DigestFrequencyHours = "Notification.DigestFrequencyHours";
    }

    public static class Storage
    {
        public const string Group = "Storage";
        public const string MaxFileSizeMb = "Storage.MaxFileSizeMb";
        public const string StorageProvider = "Storage.StorageProvider";
        public const string BucketName = "Storage.BucketName";
    }

    public static class Ui
    {
        public const string Group = "UI";
        public const string Theme = "UI.Theme";
        public const string ItemsPerPage = "UI.ItemsPerPage";
        public const string DateFormat = "UI.DateFormat";
        public const string EnableDarkMode = "UI.EnableDarkMode";
    }
}
