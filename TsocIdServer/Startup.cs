using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services.Default;
using Kentor.AuthServices.Owin;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Microsoft.Owin.Security.WsFederation;
using AuthenticationOptions = IdentityServer3.Core.Configuration.AuthenticationOptions;


[assembly: OwinStartup(typeof(TsocIdServer.Startup))]

namespace TsocIdServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            app.Map("/core1", coreApp =>
            {
                coreApp.UseIdentityServer(GetIdentityServerOptions());

            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });


        }

        private IdentityServerOptions GetIdentityServerOptions()
        {
            return new IdentityServerOptions
            {
                SiteName="TSOC Identity Server",
                SigningCertificate = Cert.LoadTestCert(),
                Factory = new IdentityServerServiceFactory()
                                .UseInMemoryClients(Config.Clients())
                                .UseInMemoryScopes(Config.Scopes())
                                .UseInMemoryUsers(TestUsers.Users),
                                
                RequireSsl =  false,
                
                AuthenticationOptions = new AuthenticationOptions
                {
                    EnableLocalLogin = true,
                    IdentityProviders = ConfigureIdentityProviders,
                    EnablePostSignOutAutoRedirect = true
                },
                IssuerUri = "https://tsocId.azurewebsites.net/core"

            };
        }

        private NormalizingClaimsFilter _wifFilter = new NormalizingClaimsFilter();
        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            app.UseWsFederationAuthentication(
                new WsFederationAuthenticationOptions
                {
                   
                    MetadataAddress = "https://tsocid.azurewebsites.net/idpMeta_VIT_UAT_TSOC.xml",
                    AuthenticationType = "Telstra",
                    Caption = "Telstra Signin",
                    SignInAsAuthenticationType = signInAsType,
                    AuthenticationMode = AuthenticationMode.Active,
                    
                    

                    Notifications = new WsFederationAuthenticationNotifications
                    {
                        SecurityTokenValidated = n =>
                        {
                            var incomingClaims = n.AuthenticationTicket.Identity.Claims.ToList();
                            var incomingClaimsHasNameIdentifier = incomingClaims.Any(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);

                            var normalisedClaims = _wifFilter.Filter(incomingClaims);

                            var identity = new ClaimsIdentity(normalisedClaims, n.AuthenticationTicket.Identity.AuthenticationType);

                            n.AuthenticationTicket = new AuthenticationTicket(identity, n.AuthenticationTicket.Properties);


                            return Task.FromResult(0);
                        }
                    },
                });
        }

        private X509Certificate2 GetSigningCertificate()
        {
            //signing certifcate provided by telstra
            var certBase64 = "MIICBDCCAW0CBEkbTlUwDQYJKoZIhvcNAQEEBQAwSTELMAkGA1UEBhMCYXUxFzAVBgoJkiaJk/IsZAEZFgd0ZWxzdHJhMRAwDgYDVQQLEwdiaWdwb25kMQ8wDQYDVQQDEwZSQUFTU08wHhcNMDgxMTEyMjE0NDUzWhcNMTgxMTEwMjE0NDUzWjBJMQswCQYDVQQGEwJhdTEXMBUGCgmSJomT8ixkARkWB3RlbHN0cmExEDAOBgNVBAsTB2JpZ3BvbmQxDzANBgNVBAMTBlJBQVNTTzCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEA7Hv0RkNMnCD / 8fY3pobKUkcieEEDbc3sC4cfGDn36OyJNcHqEHMnvsyYiNuoB5Gwqb + FI3JknaHXshcjLctiyrYqY2Qb8oErTqYRqEHeVC7J6wpNIpLZ4Z1BZRo7FYABsw2xwKZxygkXHkQhrbwe/SSamp1854PMvI5+apVbXPUCAwEAATANBgkqhkiG9w0BAQQFAAOBgQAPqy+7BByyqWB0TVheAaIHtR7rwehZRiPv5Stc2n39L9vOcG/akDRmuNwkP9EJ45Q0FNKZY3yahm5gVNUqryH19hkh2udKuxkGaC8niBF7s4V8pbqDIRdy8e/F7wgKQ1gq5QO15L6JJY+MpAicX5bhfsTUQ0n+MeISwcKEd3wY5A==";
            var certBytes = Convert.FromBase64String(certBase64);

            var cert = new X509Certificate2(certBytes);
            return cert;
        }
    }
}
