namespace SSW.Rewards.AppHost.Hosts;

// AdminUI (Blazor WASM) — references the API and waits for it to be ready.
public static class AdminUiHost
{
    public static IResourceBuilder<ProjectResource> AddAdminUiHost(
        this IDistributedApplicationBuilder builder, IResourceBuilder<ProjectResource> api)
    {
        return builder.AddProject<Projects.AdminUI>("rewards-adminui")
            .WithReference(api)
            .WaitFor(api)
            .WithExternalHttpEndpoints();
    }
}
