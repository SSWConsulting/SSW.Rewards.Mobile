using FluentAssertions;
using NUnit.Framework;
using SSW.Rewards.Shared.Utils;

namespace SSW.Rewards.Application.UnitTests.Common.Utils;

/// <summary>
/// Unit tests for RegexHelpers business logic.
/// Tests URL validation and handle extraction for social media platforms.
/// </summary>
[TestFixture]
public class RegexHelpersTests
{
    #region LinkedIn Tests

    [Test]
    public void LinkedInRegex_WithValidUrl_ShouldMatch()
    {
        // Arrange
        var validUrls = new[]
        {
            "https://www.linkedin.com/in/john-smith",
            "http://linkedin.com/in/john-smith",
            "https://linkedin.com/in/john.smith",
            "https://www.linkedin.com/in/john_smith-123"
        };

        // Act & Assert
        foreach (var url in validUrls)
        {
            var regex = RegexHelpers.LinkedInRegex();
            regex.IsMatch(url).Should().BeTrue($"URL '{url}' should be valid");
        }
    }

    [Test]
    public void LinkedInRegex_WithInvalidUrl_ShouldNotMatch()
    {
        // Arrange
        var invalidUrls = new[]
        {
            "https://twitter.com/in/john-smith",
            "https://linkedin.com/john-smith",
            "linkedin.com/in/john-smith",
            "not a url"
        };

        // Act & Assert
        foreach (var url in invalidUrls)
        {
            var regex = RegexHelpers.LinkedInRegex();
            regex.IsMatch(url).Should().BeFalse($"URL '{url}' should be invalid");
        }
    }

    [Test]
    public void LinkedInRegex_ExtractHandle_ShouldReturnUsername()
    {
        // Arrange
        var url = "https://www.linkedin.com/in/john-smith-123";
        var regex = RegexHelpers.LinkedInRegex();

        // Act
        var handle = regex.ExtractHandle(url);

        // Assert
        handle.Should().Be("john-smith-123");
    }

    [Test]
    public void LinkedInRegex_WithQueryParams_ShouldExtractHandle()
    {
        // Arrange
        var url = "https://www.linkedin.com/in/john-smith?ref=someref";
        var regex = RegexHelpers.LinkedInRegex();

        // Act
        var handle = regex.ExtractHandle(url);

        // Assert
        handle.Should().Be("john-smith");
    }

    #endregion

    #region GitHub Tests

    [Test]
    public void GitHubRegex_WithValidUrl_ShouldMatch()
    {
        // Arrange
        var validUrls = new[]
        {
            "https://github.com/octocat",
            "http://www.github.com/octocat",
            "https://github.com/octo-cat",
            "https://github.com/octo_cat"
        };

        // Act & Assert
        foreach (var url in validUrls)
        {
            var regex = RegexHelpers.GitHubRegex();
            regex.IsMatch(url).Should().BeTrue($"URL '{url}' should be valid");
        }
    }

    [Test]
    public void GitHubRegex_ExtractHandle_ShouldReturnUsername()
    {
        // Arrange
        var url = "https://github.com/octocat";
        var regex = RegexHelpers.GitHubRegex();

        // Act
        var handle = regex.ExtractHandle(url);

        // Assert
        handle.Should().Be("octocat");
    }

    [Test]
    public void GitHubRegex_WithInvalidUrl_ShouldNotMatch()
    {
        // Arrange
        var invalidUrls = new[]
        {
            "https://gitlab.com/octocat",
            "github.com/octocat",
            "not a url"
        };

        // Act & Assert
        foreach (var url in invalidUrls)
        {
            var regex = RegexHelpers.GitHubRegex();
            regex.IsMatch(url).Should().BeFalse($"URL '{url}' should be invalid");
        }
    }

    #endregion

    #region Twitter/X Tests

    [Test]
    public void TwitterRegex_WithValidUrl_ShouldMatch()
    {
        // Arrange
        var validUrls = new[]
        {
            "https://twitter.com/jack",
            "http://www.twitter.com/jack",
            "https://x.com/elonmusk",
            "https://www.x.com/elonmusk"
        };

        // Act & Assert
        foreach (var url in validUrls)
        {
            var regex = RegexHelpers.TwitterRegex();
            regex.IsMatch(url).Should().BeTrue($"URL '{url}' should be valid");
        }
    }

    [Test]
    public void TwitterRegex_ExtractHandle_ShouldReturnUsername()
    {
        // Arrange
        var url = "https://twitter.com/jack";
        var regex = RegexHelpers.TwitterRegex();

        // Act
        var handle = regex.ExtractHandle(url);

        // Assert
        handle.Should().Be("jack");
    }

    [Test]
    public void TwitterRegex_WithXDomain_ShouldExtractHandle()
    {
        // Arrange
        var url = "https://x.com/elonmusk";
        var regex = RegexHelpers.TwitterRegex();

        // Act
        var handle = regex.ExtractHandle(url);

        // Assert
        handle.Should().Be("elonmusk");
    }

    #endregion

    #region Website Tests

    [Test]
    public void WebsiteRegex_WithValidDomain_ShouldMatch()
    {
        // Arrange
        var validUrls = new[]
        {
            "https://ssw.com.au",
            "http://www.ssw.com.au",
            "https://example.com/path/to/page",
            "https://sub.domain.co.uk"
        };

        // Act & Assert
        foreach (var url in validUrls)
        {
            var regex = RegexHelpers.WebsiteRegex();
            regex.IsMatch(url).Should().BeTrue($"URL '{url}' should be valid");
        }
    }

    [Test]
    public void WebsiteRegex_ExtractHandle_ShouldReturnDomain()
    {
        // Arrange
        var url = "https://ssw.com.au/people/adam-cogan";
        var regex = RegexHelpers.WebsiteRegex();

        // Act
        var handle = regex.ExtractHandle(url);

        // Assert
        handle.Should().Be("ssw.com.au");
    }

    [Test]
    public void WebsiteRegex_WithSubdomain_ShouldExtractFullDomain()
    {
        // Arrange
        var url = "https://rules.ssw.com.au";
        var regex = RegexHelpers.WebsiteRegex();

        // Act
        var handle = regex.ExtractHandle(url);

        // Assert
        handle.Should().Be("rules.ssw.com.au");
    }

    #endregion

    #region ExtractHandle Edge Cases

    [Test]
    public void ExtractHandle_WithInvalidUrl_ShouldReturnEmptyString()
    {
        // Arrange
        var invalidUrl = "not a valid url";
        var regex = RegexHelpers.LinkedInRegex();

        // Act
        var handle = regex.ExtractHandle(invalidUrl);

        // Assert
        handle.Should().BeEmpty();
    }

    [Test]
    public void ExtractHandle_WithEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        var emptyUrl = "";
        var regex = RegexHelpers.LinkedInRegex();

        // Act
        var handle = regex.ExtractHandle(emptyUrl);

        // Assert
        handle.Should().BeEmpty();
    }

    [Test]
    public void ExtractHandle_WithNonMatchingUrl_ShouldReturnEmptyString()
    {
        // Arrange
        var nonMatchingUrl = "https://example.com/page";
        var regex = RegexHelpers.LinkedInRegex();

        // Act
        var handle = regex.ExtractHandle(nonMatchingUrl);

        // Assert
        handle.Should().BeEmpty();
    }

    #endregion
}
