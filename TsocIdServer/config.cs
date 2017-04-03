using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using IdentityModel;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.InMemory;

namespace TsocIdServer
{
    public class Config
    {

        public static IEnumerable<Client> Clients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "tsoc",
                    ClientName = "tsoc-portal",
                    Enabled = true,
                    Flow = Flows.Implicit,
                    AllowRememberConsent = false,
                    RequireConsent = false,
                    AccessTokenType = AccessTokenType.Jwt,
                AllowAccessTokensViaBrowser = true,
                    RedirectUris = { "http://localhost:4200/login-callback","http://localhost:5003/callback" },
                    PostLogoutRedirectUris = {"http://localhost:4200", "http://localhost:5003" },
                    AllowedCorsOrigins = { "http://localhost:4200", "http://localhost:5003" },
                    AllowedScopes =
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.Profile,
                        "tsoc",
                        Constants.StandardScopes.Roles
                    },
                   
                   // LogoutUri = "http://localhost:5003/logout",
                   
                   
                    

                }
            };
        }

        public static IEnumerable<Scope> Scopes()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId,
                StandardScopes.Email,
                StandardScopes.Roles,
                new Scope
                {
                    Enabled = true,
                    Name = "tsoc",
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(Constants.ClaimTypes.Role, true),
                        new ScopeClaim("businessId", true)
                    }
                },
                new Scope
                    {
                        Enabled = true,
                        Name = Constants.StandardScopes.Profile,
                        Claims = new List<ScopeClaim>
                        {
                            new ScopeClaim(Constants.ClaimTypes.GivenName, true),
                            new ScopeClaim(Constants.ClaimTypes.FamilyName, true)
                        }
                    }
            };
        }
    }
}