using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace SSW.Rewards.Admin.UI;

/// <summary>
/// Custom claims principal factory for local development that fetches roles from the database.
/// 
/// NOTE: This is a workaround for SSW.Identity limitation where @ssw.com.au email addresses
/// don't receive proper role claims in JWT tokens. This needs to be fixed in SSW.Identity.
/// 
/// In DEBUG mode, this factory will:
/// 1. Process JWT role claims normally (array format support)
/// 2. Additionally fetch roles from the database via API
/// 3. Replace JWT roles with database roles if fetch succeeds
/// 
/// This ensures developers with @ssw.com.au emails can test role-based features locally.
/// </summary>
public class WebRolesClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WebRolesClaimsPrincipalFactory(
        IAccessTokenProviderAccessor accessor,
        IHttpClientFactory httpClientFactory) : base(accessor)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
        RemoteUserAccount account, 
        RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);
        
        if (user.Identity?.IsAuthenticated ?? false)
        {
            var identity = (ClaimsIdentity)user.Identity;

            // Process JWT role claims (support array format from SSW.Identity)
            ProcessJwtRoleClaims(identity, account);

            // Fetch and apply roles from database (overrides JWT roles)
            await AddDatabaseRolesAsync(identity);
        }

        return user;
    }

    /// <summary>
    /// Process role claims from JWT token, handling both string and array formats.
    /// This is required because SSW.Identity returns roles as a string array.
    /// See: https://github.com/dotnet/aspnetcore/issues/21836
    /// </summary>
    private static void ProcessJwtRoleClaims(ClaimsIdentity identity, RemoteUserAccount account)
    {
        var roleClaims = identity.FindAll(identity.RoleClaimType).ToList();

        if (roleClaims.Any())
        {
            // Remove existing role claims
            foreach (var claim in roleClaims)
            {
                identity.RemoveClaim(claim);
            }

            // Re-add roles from account properties (handles array format)
            if (account.AdditionalProperties.TryGetValue(identity.RoleClaimType, out var rolesElem) 
                && rolesElem is JsonElement roles)
            {
                if (roles.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        var roleValue = role.GetString();
                        if (!string.IsNullOrEmpty(roleValue))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                        }
                    }
                }
                else
                {
                    var roleValue = roles.GetString();
                    if (!string.IsNullOrEmpty(roleValue))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Fetch roles from database via API and replace JWT roles.
    /// This is the workaround for @ssw.com.au email limitation.
    /// 
    /// Future: Remove this when SSW.Identity properly assigns roles to @ssw.com.au users.
    /// </summary>
    private async Task AddDatabaseRolesAsync(ClaimsIdentity identity)
    {
        try
        {
            // Get authenticated HTTP client to call the API
            var client = _httpClientFactory.CreateClient("AuthenticatedClient");
            
            // Call the API to get the user's roles from the database
            var response = await client.GetAsync("api/user/myroles");
            
            if (response.IsSuccessStatusCode)
            {
                var rolesJson = await response.Content.ReadAsStringAsync();
                var databaseRoles = JsonSerializer.Deserialize<string[]>(rolesJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (databaseRoles != null && databaseRoles.Length > 0)
                {
                    // Remove existing role claims (from JWT)
                    var existingRoleClaims = identity.FindAll(ClaimTypes.Role).ToList();
                    foreach (var claim in existingRoleClaims)
                    {
                        identity.RemoveClaim(claim);
                    }

                    // Add database roles
                    foreach (var role in databaseRoles)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }

                    Console.WriteLine($"[WebRolesClaimsPrincipalFactory] Loaded {databaseRoles.Length} role(s) from database: {string.Join(", ", databaseRoles)}");
                }
                else
                {
                    Console.WriteLine("[WebRolesClaimsPrincipalFactory] No database roles returned for user");
                }
            }
            else
            {
                Console.WriteLine($"[WebRolesClaimsPrincipalFactory] Failed to fetch database roles: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[WebRolesClaimsPrincipalFactory] HTTP error fetching database roles: {ex.Message}");
            // Fail silently - keep JWT roles if database fetch fails
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[WebRolesClaimsPrincipalFactory] JSON error parsing database roles: {ex.Message}");
            // Fail silently - keep JWT roles if database fetch fails
        }
    }
}
