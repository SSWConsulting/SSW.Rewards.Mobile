namespace SSW.Rewards.AppHost.Hosts;

// WebAPI — connection strings come from the common infra; everything else
// (Firebase / SendGrid / SMTP / identity authority) is supplied here as Aspire
// parameters and injected under the exact keys the app binds. The WebAPI no
// longer has its own UserSecretsId — config flows in from this AppHost only.
public static class WebApiHost
{
    public static IResourceBuilder<ProjectResource> AddWebApiHost(
        this IDistributedApplicationBuilder builder, CommonInfra common)
    {
        var firebaseCreds = builder.AddParameter("firebase-credentials", secret: true);
        var sendGridApiKey = builder.AddParameter("sendgrid-api-key", secret: true);
        var emailUser = builder.AddParameter("email-user", secret: true);
        var emailPassword = builder.AddParameter("email-password", secret: true);
        var signingAuthority = builder.AddParameter("signing-authority", secret: false);

        return builder.AddProject<Projects.WebAPI>("rewards-webapi")
            .WithEnvironment("ConnectionStrings__DefaultConnection", common.RewardsDatabase)
            .WithEnvironment("ConnectionStrings__HangfireConnection", common.HangfireDatabase)
            // Use Azurite for the content blob store locally (overrides the prod string).
            .WithEnvironment("CloudBlobProviderOptions__ContentStorageConnectionString", common.Blobs)
            .WithEnvironment("Firebase__FirebaseCredentials", firebaseCreds)
            .WithEnvironment("SendGridAPIKey", sendGridApiKey)
            .WithEnvironment("EmailUser", emailUser)
            .WithEnvironment("EmailPassword", emailPassword)
            .WithEnvironment("SigningAuthority", signingAuthority)
            .WaitFor(common.RewardsDatabase)
            .WaitFor(common.HangfireDatabase)
            .WaitFor(common.Blobs)
            .WithExternalHttpEndpoints();
    }
}
