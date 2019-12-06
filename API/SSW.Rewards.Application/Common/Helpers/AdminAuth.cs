using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace SSW.Rewards.Application.Common.Helpers
{
    public class AdminAuth
    {
        private const string ADMIN_IDP = "https://sts.windows.net/ac2f7c34-b935-48e9-abdc-11e5d4fcb2b0/";
        private const string BEARER = "Bearer ";
        private const string IDP = "idp: ";

        private static string FindFirstAndReplace(IEnumerable<object> e, string search)
        {
            return e.First(c => c.ToString().Contains(search)).ToString().Replace(search, string.Empty);
        }
        public static bool RequestFromAdmin(HttpRequest Request)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            try
            {
                var token = FindFirstAndReplace(Request.Headers.Values.SelectMany(e => e), BEARER);
                var idp = FindFirstAndReplace(handler.ReadJwtToken(token).Claims, IDP);
                return idp == ADMIN_IDP;
            }
            catch (Exception _)
            {
                return false;
            }

        }
    }
}
