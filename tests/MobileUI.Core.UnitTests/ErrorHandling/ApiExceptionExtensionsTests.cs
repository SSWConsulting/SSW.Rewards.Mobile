using System.Net;
using FluentAssertions;
using NUnit.Framework;
using SSW.Rewards.Mobile.Services;

namespace SSW.Rewards.Mobile.Core.UnitTests.ErrorHandling;

public class ApiExceptionExtensionsTests
{
    [Test]
    public void IsUnauthorized_401_IsTrue() =>
        new HttpRequestException("expired", null, HttpStatusCode.Unauthorized).IsUnauthorized().Should().BeTrue();

    [Test]
    public void IsUnauthorized_OtherStatus_IsFalse() =>
        new HttpRequestException("forbidden", null, HttpStatusCode.Forbidden).IsUnauthorized().Should().BeFalse();

    [Test]
    public void IsUnauthorized_NoStatus_IsFalse() =>
        new HttpRequestException("offline").IsUnauthorized().Should().BeFalse();

    [Test]
    public void IsUnauthorized_PlainException_IsFalse() =>
        new Exception("Unauthorized").IsUnauthorized().Should().BeFalse();
}
