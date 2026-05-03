using CleanArch.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Persistence.EFCore.Configurations;

public sealed class GlobalSettingConfiguration : IEntityTypeConfiguration<GlobalSetting>
{
    public void Configure(EntityTypeBuilder<GlobalSetting> builder)
    {
        builder.ToTable("GlobalSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(s => s.Group)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.DataType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.IsReadOnly)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Unique key constraint — one setting per key
        builder.HasIndex(s => s.Key)
            .IsUnique()
            .HasDatabaseName("IX_GlobalSettings_Key");

        // Group lookup index
        builder.HasIndex(s => s.Group)
            .HasDatabaseName("IX_GlobalSettings_Group");

        // Seed default settings
        SeedDefaults(builder);
    }

    private static void SeedDefaults(EntityTypeBuilder<GlobalSetting> builder)
    {
        builder.HasData(
            CreateSeed("d1a01001-0000-0000-0000-000000000001", SettingKeys.General.ApplicationName, "CleanArch API", SettingKeys.General.Group, SettingDataType.Text, "Name of the application", true, 1),
            CreateSeed("d1a01001-0000-0000-0000-000000000002", SettingKeys.General.ApplicationUrl, "https://localhost:5001", SettingKeys.General.Group, SettingDataType.Text, "Base URL of the application", false, 2),
            CreateSeed("d1a01001-0000-0000-0000-000000000003", SettingKeys.General.DefaultLanguage, "en", SettingKeys.General.Group, SettingDataType.Text, "Default language code (ISO 639-1)", false, 3),
            CreateSeed("d1a01001-0000-0000-0000-000000000004", SettingKeys.General.DefaultTimezone, "UTC", SettingKeys.General.Group, SettingDataType.Text, "Default timezone", false, 4),
            CreateSeed("d1a01001-0000-0000-0000-000000000005", SettingKeys.General.MaintenanceMode, "false", SettingKeys.General.Group, SettingDataType.Flag, "Enable maintenance mode (blocks all API requests)", false, 5),

            CreateSeed("d1a02001-0000-0000-0000-000000000001", SettingKeys.Email.SmtpHost, "smtp.example.com", SettingKeys.Email.Group, SettingDataType.Text, "SMTP server hostname", false, 1),
            CreateSeed("d1a02001-0000-0000-0000-000000000002", SettingKeys.Email.SmtpPort, "587", SettingKeys.Email.Group, SettingDataType.WholeNumber, "SMTP server port", false, 2),
            CreateSeed("d1a02001-0000-0000-0000-000000000003", SettingKeys.Email.SenderName, "CleanArch", SettingKeys.Email.Group, SettingDataType.Text, "Sender display name", false, 3),
            CreateSeed("d1a02001-0000-0000-0000-000000000004", SettingKeys.Email.SenderAddress, "noreply@example.com", SettingKeys.Email.Group, SettingDataType.Text, "Sender email address", false, 4),
            CreateSeed("d1a02001-0000-0000-0000-000000000005", SettingKeys.Email.EnableSsl, "true", SettingKeys.Email.Group, SettingDataType.Flag, "Enable SSL for SMTP", false, 5),

            CreateSeed("d1a03001-0000-0000-0000-000000000001", SettingKeys.Security.MaxLoginAttempts, "5", SettingKeys.Security.Group, SettingDataType.WholeNumber, "Maximum failed login attempts before lockout", false, 1),
            CreateSeed("d1a03001-0000-0000-0000-000000000002", SettingKeys.Security.LockoutDurationMinutes, "15", SettingKeys.Security.Group, SettingDataType.WholeNumber, "Account lockout duration in minutes", false, 2),
            CreateSeed("d1a03001-0000-0000-0000-000000000003", SettingKeys.Security.PasswordMinLength, "8", SettingKeys.Security.Group, SettingDataType.WholeNumber, "Minimum password length", false, 3),
            CreateSeed("d1a03001-0000-0000-0000-000000000004", SettingKeys.Security.RequireTwoFactor, "false", SettingKeys.Security.Group, SettingDataType.Flag, "Require two-factor authentication", false, 4),
            CreateSeed("d1a03001-0000-0000-0000-000000000005", SettingKeys.Security.SessionTimeoutMinutes, "60", SettingKeys.Security.Group, SettingDataType.WholeNumber, "Session timeout in minutes", false, 5),

            CreateSeed("d1a04001-0000-0000-0000-000000000001", SettingKeys.Ui.Theme, "light", SettingKeys.Ui.Group, SettingDataType.Text, "Default UI theme (light/dark)", false, 1),
            CreateSeed("d1a04001-0000-0000-0000-000000000002", SettingKeys.Ui.ItemsPerPage, "10", SettingKeys.Ui.Group, SettingDataType.WholeNumber, "Default items per page in lists", false, 2),
            CreateSeed("d1a04001-0000-0000-0000-000000000003", SettingKeys.Ui.DateFormat, "yyyy-MM-dd", SettingKeys.Ui.Group, SettingDataType.Text, "Default date format", false, 3),
            CreateSeed("d1a04001-0000-0000-0000-000000000004", SettingKeys.Ui.EnableDarkMode, "true", SettingKeys.Ui.Group, SettingDataType.Flag, "Allow users to toggle dark mode", false, 4)
        );
    }

    private static object CreateSeed(
        string id, string key, string value, string group,
        SettingDataType dataType, string description, bool isReadOnly, int displayOrder)
    {
        return new
        {
            Id = Guid.Parse(id),
            Key = key,
            Value = value,
            Group = group,
            DataType = dataType,
            Description = description,
            IsReadOnly = isReadOnly,
            DisplayOrder = displayOrder,
            CreatedOnUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "System",
            ModifiedOnUtc = (DateTime?)null,
            ModifiedBy = (string?)null
        };
    }
}
