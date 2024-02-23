using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace SSW.Rewards.Admin.UI;

public class RolesClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
{
    public RolesClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor) : base(accessor)
    {
    }

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account, RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);
        if (user.Identity.IsAuthenticated)
        {
            var identity = (ClaimsIdentity)user.Identity;

            var roleClaims = identity.FindAll(identity.RoleClaimType);

            if (roleClaims != null && roleClaims.Any())
            {
                var count = roleClaims.Count();

                for (int i = 0; i < count; i++)
                {
                    identity.RemoveClaim(roleClaims.ElementAt(i));
                }

                var rolesElem = account.AdditionalProperties[identity.RoleClaimType];

                if (rolesElem is JsonElement roles)
                {
                    if (roles.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()));
                        }
                    }
                    else
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, roles.GetString()));
                    }
                }
            }
        }

        return user;
    }
}