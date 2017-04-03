using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using IdentityServer3.Core.Services;

namespace TsocIdServer.Extensions
{
    public class TelstraClaimsFilter : IExternalClaimsFilter
    {
        public IEnumerable<Claim> Filter(string provider, IEnumerable<Claim> claims)
        {
            return claims;
        }
    }
}