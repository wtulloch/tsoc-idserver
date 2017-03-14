using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services.Default;
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

            app.Map("/core", coreApp =>
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
                }

            };
        }

        private NormalizingClaimsFilter _wifFilter = new NormalizingClaimsFilter();
        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            app.UseWsFederationAuthentication(
                new WsFederationAuthenticationOptions
                {
                    Wtrealm = "urn:Id3Test",
                    MetadataAddress = "https://localhost:44380/core/wsfed/metadata",

                    AuthenticationType = "identityServer",
                    Caption = "Identity Server",
                    SignInAsAuthenticationType = signInAsType,

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
    }
}
