using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SSW.Rewards.Shared.Configuration;

/// <summary>
/// Extension methods for configuring TenantSettings.
/// </summary>
public static class TenantSettingsExtensions
{
    /// <summary>
    /// Adds TenantSettings to the service collection by binding from configuration and optionally validating on startup.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="validateOnStartup">Whether to validate settings on startup (default: true)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddTenantSettings(
        this IServiceCollection services,
        IConfiguration configuration,
        bool validateOnStartup = true)
    {
        // Bind from configuration (appsettings.json)
        services.Configure<TenantSettings>(configuration.GetSection(TenantSettings.SectionName));

        // Register as singleton for easy injection
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<TenantSettings>>().Value);

        // Validate on startup if requested
        if (validateOnStartup)
        {
            var section = configuration.GetSection(TenantSettings.SectionName);
            var tenantSettings = section.Get<TenantSettings>();
            if (tenantSettings == null)
            {
                throw new InvalidOperationException(
                    $"Configuration section '{TenantSettings.SectionName}' is missing or empty. " +
                    "This section is required when validateOnStartup is enabled.");
            }

            tenantSettings.Validate();
        }

        return services;
    }

    /// <summary>
    /// Validates the TenantSettings configuration at application startup
    /// </summary>
    /// <param name="tenantSettings">The tenant settings to validate</param>
    /// <exception cref="InvalidOperationException">Thrown when required settings are missing or invalid</exception>
    public static void Validate(this TenantSettings tenantSettings)
    {
        var errors = new List<string>();

        // Branding validation
        if (string.IsNullOrWhiteSpace(tenantSettings.Branding.CompanyName))
            errors.Add("TenantSettings.Branding.CompanyName is required");

        if (string.IsNullOrWhiteSpace(tenantSettings.Branding.ApplicationName))
            errors.Add("TenantSettings.Branding.ApplicationName is required");

        // Contact validation
        if (string.IsNullOrWhiteSpace(tenantSettings.Contact.StaffEmailDomain))
            errors.Add("TenantSettings.Contact.StaffEmailDomain is required");

        if (!string.IsNullOrWhiteSpace(tenantSettings.Contact.SupportEmail) &&
            !IsValidEmail(tenantSettings.Contact.SupportEmail))
            errors.Add($"TenantSettings.Contact.SupportEmail '{tenantSettings.Contact.SupportEmail}' is not a valid email address");

        if (!string.IsNullOrWhiteSpace(tenantSettings.Contact.MarketingEmail) &&
            !IsValidEmail(tenantSettings.Contact.MarketingEmail))
            errors.Add($"TenantSettings.Contact.MarketingEmail '{tenantSettings.Contact.MarketingEmail}' is not a valid email address");

        if (!string.IsNullOrWhiteSpace(tenantSettings.Contact.DefaultSenderEmail) &&
            !IsValidEmail(tenantSettings.Contact.DefaultSenderEmail))
            errors.Add($"TenantSettings.Contact.DefaultSenderEmail '{tenantSettings.Contact.DefaultSenderEmail}' is not a valid email address");

        // External services validation
        if (string.IsNullOrWhiteSpace(tenantSettings.ExternalServices.ApiBaseUrl))
            errors.Add("TenantSettings.ExternalServices.ApiBaseUrl is required");

        if (!string.IsNullOrWhiteSpace(tenantSettings.ExternalServices.ApiBaseUrl) &&
            !IsValidUrl(tenantSettings.ExternalServices.ApiBaseUrl))
            errors.Add($"TenantSettings.ExternalServices.ApiBaseUrl '{tenantSettings.ExternalServices.ApiBaseUrl}' is not a valid URL");

        if (string.IsNullOrWhiteSpace(tenantSettings.ExternalServices.IdentityServerUrl))
            errors.Add("TenantSettings.ExternalServices.IdentityServerUrl is required");

        if (!string.IsNullOrWhiteSpace(tenantSettings.ExternalServices.IdentityServerUrl) &&
            !IsValidUrl(tenantSettings.ExternalServices.IdentityServerUrl))
            errors.Add($"TenantSettings.ExternalServices.IdentityServerUrl '{tenantSettings.ExternalServices.IdentityServerUrl}' is not a valid URL");

        // Color validation
        if (!string.IsNullOrWhiteSpace(tenantSettings.Colors.Primary) &&
            !IsValidHexColor(tenantSettings.Colors.Primary))
            errors.Add($"TenantSettings.Colors.Primary '{tenantSettings.Colors.Primary}' is not a valid hex color (expected format: #RRGGBB)");

        if (!string.IsNullOrWhiteSpace(tenantSettings.Colors.Secondary) &&
            !IsValidHexColor(tenantSettings.Colors.Secondary))
            errors.Add($"TenantSettings.Colors.Secondary '{tenantSettings.Colors.Secondary}' is not a valid hex color (expected format: #RRGGBB)");

        if (!string.IsNullOrWhiteSpace(tenantSettings.Colors.Accent) &&
            !IsValidHexColor(tenantSettings.Colors.Accent))
            errors.Add($"TenantSettings.Colors.Accent '{tenantSettings.Colors.Accent}' is not a valid hex color (expected format: #RRGGBB)");

        if (errors.Any())
        {
            throw new InvalidOperationException(
                $"TenantSettings validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$");
    }
}
