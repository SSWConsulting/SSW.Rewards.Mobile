using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SSW.Rewards.Shared.Configuration;

namespace SSW.Rewards.Application.UnitTests.Common.Configuration;

public class TenantSettingsTests
{
    [Test]
    public void TenantSettings_LoadFromConfiguration_SucceedsWithValidConfig()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["TenantSettings:Branding:CompanyName"] = "Test Company",
            ["TenantSettings:Branding:ApplicationName"] = "Test App",
            ["TenantSettings:Contact:StaffEmailDomain"] = "test.com",
            ["TenantSettings:ExternalServices:ApiBaseUrl"] = "https://api.test.com",
            ["TenantSettings:ExternalServices:IdentityServerUrl"] = "https://identity.test.com",
            ["TenantSettings:Colors:Primary"] = "#FF0000",
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();
        services.AddTenantSettings(configuration, validateOnStartup: false);

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var tenantSettings = serviceProvider.GetRequiredService<TenantSettings>();

        // Assert
        tenantSettings.Should().NotBeNull();
        tenantSettings.Branding.CompanyName.Should().Be("Test Company");
        tenantSettings.Branding.ApplicationName.Should().Be("Test App");
        tenantSettings.Contact.StaffEmailDomain.Should().Be("test.com");
        tenantSettings.ExternalServices.ApiBaseUrl.Should().Be("https://api.test.com");
        tenantSettings.Colors.Primary.Should().Be("#FF0000");
    }

    [Test]
    public void Validate_ThrowsException_WhenRequiredFieldsMissing()
    {
        // Arrange
        var tenantSettings = new TenantSettings
        {
            Branding = new TenantSettings.BrandingSettings
            {
                CompanyName = "", // Missing required field
                ApplicationName = "Test App"
            },
            Contact = new TenantSettings.ContactSettings
            {
                StaffEmailDomain = "test.com"
            },
            ExternalServices = new TenantSettings.ExternalServicesSettings
            {
                ApiBaseUrl = "https://api.test.com",
                IdentityServerUrl = "https://identity.test.com"
            }
        };

        // Act & Assert
        var action = () => tenantSettings.Validate();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*CompanyName is required*");
    }

    [Test]
    public void Validate_ThrowsException_WhenInvalidEmail()
    {
        // Arrange
        var tenantSettings = new TenantSettings
        {
            Branding = new TenantSettings.BrandingSettings
            {
                CompanyName = "Test Company",
                ApplicationName = "Test App"
            },
            Contact = new TenantSettings.ContactSettings
            {
                StaffEmailDomain = "test.com",
                SupportEmail = "invalid-email" // Invalid email
            },
            ExternalServices = new TenantSettings.ExternalServicesSettings
            {
                ApiBaseUrl = "https://api.test.com",
                IdentityServerUrl = "https://identity.test.com"
            }
        };

        // Act & Assert
        var action = () => tenantSettings.Validate();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*not a valid email address*");
    }

    [Test]
    public void Validate_ThrowsException_WhenInvalidUrl()
    {
        // Arrange
        var tenantSettings = new TenantSettings
        {
            Branding = new TenantSettings.BrandingSettings
            {
                CompanyName = "Test Company",
                ApplicationName = "Test App"
            },
            Contact = new TenantSettings.ContactSettings
            {
                StaffEmailDomain = "test.com"
            },
            ExternalServices = new TenantSettings.ExternalServicesSettings
            {
                ApiBaseUrl = "not-a-url", // Invalid URL
                IdentityServerUrl = "https://identity.test.com"
            }
        };

        // Act & Assert
        var action = () => tenantSettings.Validate();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*not a valid URL*");
    }

    [Test]
    public void Validate_ThrowsException_WhenInvalidHexColor()
    {
        // Arrange
        var tenantSettings = new TenantSettings
        {
            Branding = new TenantSettings.BrandingSettings
            {
                CompanyName = "Test Company",
                ApplicationName = "Test App"
            },
            Contact = new TenantSettings.ContactSettings
            {
                StaffEmailDomain = "test.com"
            },
            ExternalServices = new TenantSettings.ExternalServicesSettings
            {
                ApiBaseUrl = "https://api.test.com",
                IdentityServerUrl = "https://identity.test.com"
            },
            Colors = new TenantSettings.ColorSettings
            {
                Primary = "red" // Invalid hex color
            }
        };

        // Act & Assert
        var action = () => tenantSettings.Validate();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*not a valid hex color*");
    }

    [Test]
    public void Validate_ThrowsException_WhenQuizEnabledButNoUrl()
    {
        // Arrange
        var tenantSettings = new TenantSettings
        {
            Branding = new TenantSettings.BrandingSettings
            {
                CompanyName = "Test Company",
                ApplicationName = "Test App"
            },
            Contact = new TenantSettings.ContactSettings
            {
                StaffEmailDomain = "test.com"
            },
            ExternalServices = new TenantSettings.ExternalServicesSettings
            {
                ApiBaseUrl = "https://api.test.com",
                IdentityServerUrl = "https://identity.test.com",
                QuizServiceUrl = "" // Missing when feature enabled
            }
        };

        // Act & Assert
        var action = () => tenantSettings.Validate();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*QuizServiceUrl is required when EnableQuizFeatures is true*");
    }

    [Test]
    public void Validate_Succeeds_WithValidConfiguration()
    {
        // Arrange
        var tenantSettings = new TenantSettings
        {
            Branding = new TenantSettings.BrandingSettings
            {
                CompanyName = "SSW",
                ApplicationName = "SSW Rewards",
                CompanyLegalName = "Superior Software for Windows Pty Ltd",
                CompanyWebsiteUrl = "https://www.ssw.com.au",
                LogoUrl = "https://www.ssw.com.au/logo.png"
            },
            Contact = new TenantSettings.ContactSettings
            {
                StaffEmailDomain = "ssw.com.au",
                SupportEmail = "support@ssw.com.au",
                MarketingEmail = "marketing@ssw.com.au",
                DefaultSenderEmail = "verify@ssw.com.au"
            },
            ExternalServices = new TenantSettings.ExternalServicesSettings
            {
                ApiBaseUrl = "https://api.rewards.ssw.com.au",
                IdentityServerUrl = "https://identity.ssw.com.au",
                QuizServiceUrl = "https://quiz.ssw.com.au",
                AdminPortalUrl = "https://admin.rewards.ssw.com.au"
            },
            Colors = new TenantSettings.ColorSettings
            {
                Primary = "#CC4141",
                Secondary = "#333333",
                Accent = "#CC4141"
            },
            SocialMedia = new TenantSettings.SocialMediaSettings
            {
                LinkedInUrl = "https://www.linkedin.com/company/ssw/",
                TwitterUrl = "https://twitter.com/SSW_TV"
            }
        };

        // Act & Assert
        var action = () => tenantSettings.Validate();
        action.Should().NotThrow();
    }

    [Test]
    public void TenantSettings_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var tenantSettings = new TenantSettings();

        // Assert
        tenantSettings.Colors.Primary.Should().Be("#CC4141");
        tenantSettings.Colors.Secondary.Should().Be("#333333");
        tenantSettings.Colors.Accent.Should().Be("#CC4141");
        tenantSettings.Colors.Background.Should().Be("#FFFFFF");
        tenantSettings.Colors.TextColor.Should().Be("#000000");
    }
}
