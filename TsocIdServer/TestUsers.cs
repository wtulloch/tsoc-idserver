using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using IdentityModel;
using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;

namespace TsocIdServer
{
    public class TestUsers
    {
        public static List<InMemoryUser> Users = new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Subject="1",
                    Username = "Alice",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim(JwtClaimTypes.Name, "Alice Wonderland"),
                        new Claim(JwtClaimTypes.GivenName, "Alice"),
                        new Claim(JwtClaimTypes.FamilyName, "Wonderland"),
                        new Claim(JwtClaimTypes.Email, "alice.wonderland@example.com"),
                        new Claim(JwtClaimTypes.Role, "user")
                    }
                }
            };
    }
}