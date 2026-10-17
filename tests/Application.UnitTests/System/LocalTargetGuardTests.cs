using FluentAssertions;
using NUnit.Framework;
using SSW.Rewards.Application.System.Commands.Common;

namespace SSW.Rewards.Application.UnitTests.System;

[TestFixture]
public class LocalTargetGuardTests
{
    private const string AzuriteKey =
        "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

    [TestCase("Server=127.0.0.1,59123;User ID=sa;Password=x;TrustServerCertificate=true;Initial Catalog=ssw.rewards")]
    [TestCase("Server=localhost,1433;Database=ssw.rewards;User ID=sa;Password=x")]
    [TestCase("Data Source=tcp:localhost,1433;Initial Catalog=ssw.rewards")]
    [TestCase("Server=::1;Database=ssw.rewards")]
    [TestCase("Server=(localdb)\\MSSQLLocalDB;Database=ssw.rewards;Trusted_Connection=True")]
    [TestCase("Server=.\\SQLEXPRESS;Database=ssw.rewards")]
    [TestCase("Server=host.docker.internal,1433;Database=ssw.rewards")]
    public void IsLocalSql_AcceptsThisMachine(string connectionString) =>
        LocalTargetGuard.IsLocalSql(connectionString).Should().BeTrue();

    [TestCase("Server=tcp:sswrewards-staging.database.windows.net,1433;Initial Catalog=ssw.rewards;User ID=admin;Password=x")]
    [TestCase("Server=10.0.0.5,1433;Database=ssw.rewards")]
    [TestCase("Server=localhost.evil.example;Database=ssw.rewards")]
    [TestCase("Server=;Database=ssw.rewards")]
    [TestCase("not a connection string")]
    public void IsLocalSql_RefusesAnythingElse(string connectionString) =>
        LocalTargetGuard.IsLocalSql(connectionString).Should().BeFalse();

    [TestCase($"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey={AzuriteKey};BlobEndpoint=http://127.0.0.1:59124/devstoreaccount1;")]
    [TestCase($"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey={AzuriteKey};BlobEndpoint=http://localhost:10000/devstoreaccount1;")]
    [TestCase("UseDevelopmentStorage=true")]
    public void IsLocalBlob_AcceptsAzurite(string connectionString) =>
        LocalTargetGuard.IsLocalBlob(connectionString).Should().BeTrue();

    [TestCase("DefaultEndpointsProtocol=https;AccountName=sswrewardsstaging;AccountKey=abc;EndpointSuffix=core.windows.net")]
    [TestCase($"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey={AzuriteKey};BlobEndpoint=http://storage.example.com/devstoreaccount1;")]
    [TestCase("DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=abc;EndpointSuffix=core.windows.net")]
    [TestCase("DefaultEndpointsProtocol=http;AccountName=prodaccount;AccountKey=abc;BlobEndpoint=http://127.0.0.1:10000/prodaccount;")]
    [TestCase("UseDevelopmentStorage=false")]
    [TestCase("not a connection string")]
    public void IsLocalBlob_RefusesAnythingElse(string connectionString) =>
        LocalTargetGuard.IsLocalBlob(connectionString).Should().BeFalse();
}
