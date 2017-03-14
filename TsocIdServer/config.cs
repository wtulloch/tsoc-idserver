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
                    Flow = Flows.Hybrid,
                    AllowRememberConsent = true,

                }
            };
        }

        public static IEnumerable<Scope> Scopes()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                StandardScopes.Roles
            };
        }
    }
}