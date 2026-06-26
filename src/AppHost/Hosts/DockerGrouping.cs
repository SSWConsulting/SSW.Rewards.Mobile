using Aspire.Hosting.ApplicationModel;

namespace SSW.Rewards.AppHost.Hosts;

// `aspire run` uses Aspire's own orchestrator (not Docker Compose), so its containers show up
// as separate top-level entries in Docker Desktop / OrbStack. Adding the Compose project/service
// labels makes those tools group them under one "SSW.Rewards" entry. Cosmetic only — the Aspire
// dashboard stays the source of truth; we deliberately use a project name that doesn't collide
// with the (retired) docker-compose.yml so real Compose actions never touch these containers.
public static class DockerGroupingExtensions
{
    public const string DockerProject = "SSW.Rewards";

    public static IResourceBuilder<T> InDockerProject<T>(this IResourceBuilder<T> resource)
        where T : ContainerResource
        => resource.WithContainerRuntimeArgs(
            "--label", $"com.docker.compose.project={DockerProject}",
            "--label", $"com.docker.compose.service={resource.Resource.Name}");
}
