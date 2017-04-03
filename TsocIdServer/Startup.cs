using System;
using System.IdentityModel.Metadata;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services.Default;
using Kentor.AuthServices;
using Kentor.AuthServices.Configuration;
using Kentor.AuthServices.Owin;
using Kentor.AuthServices.WebSso;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.WsFederation;
using Owin;
using TsocIdServer.Extensions;
using AuthenticationOptions = IdentityServer3.Core.Configuration.AuthenticationOptions;


[assembly: OwinStartup(typeof(TsocIdServer.Startup))]

namespace TsocIdServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            app.Use<CoreRedirect>();

            app.Map("/identity", coreApp =>
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
            var idpFactory = new IdentityServerServiceFactory()
                .UseInMemoryClients(Config.Clients())
                .UseInMemoryScopes(Config.Scopes())
                .UseInMemoryUsers(TestUsers.Users);

            var identityServerOptions = new IdentityServerOptions
            {
                SiteName="TSOC Identity Server",
                SigningCertificate = Cert.LoadTestCert(),
                Factory = idpFactory,
                RequireSsl =  false,
                AuthenticationOptions = new AuthenticationOptions
                {
                    EnableLocalLogin = false,
                    IdentityProviders = ConfigureIdentityProviders,
                    EnablePostSignOutAutoRedirect = true
                },
                IssuerUri = "https://tsocId.azurewebsites.net/identity"

            };
            return identityServerOptions;
        }

        private NormalizingClaimsFilter _wifFilter = new NormalizingClaimsFilter();
        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            SetUpTelstaFederation(app, signInAsType);
        }

        private void SetUpTelstaFederation(IAppBuilder app, string signInAsType)
        {
            //The entity Id is the one registered with Telstra. Is the end point that the SAML2 token will be posted back to.
            //Had to reduce the minimum signing algorithm since the Telestra Idp does not use the recommended standard
            var spOptions = new SPOptions
            {
                EntityId = new EntityId("https://tsocid.azurewebsites.net/core"),
                MinIncomingSigningAlgorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1",
            };
            var authServicesOptions = new KentorAuthServicesAuthenticationOptions(false)
            {
                SPOptions = spOptions,
                SignInAsAuthenticationType = signInAsType,
                AuthenticationType = "Telstra",
                Caption = "Telstra",
            };

            //Set AudiencUriMode to never to again be able to work with the Telstra Idp
            authServicesOptions.SPOptions.SystemIdentityModelIdentityConfiguration.AudienceRestriction.AudienceMode = AudienceUriMode.Never;

            var identityProvider = new IdentityProvider(
                new EntityId("signon.bigpond.com"), authServicesOptions.SPOptions)
            {
                SingleSignOnServiceUrl = new Uri("https://signonit.bigpond.com/federation/saml2?SPID=TSOC"),
                SingleLogoutServiceUrl = new Uri("https://signonit.bigpond.com/logout"),
                Binding = Saml2BindingType.HttpRedirect,

                SingleLogoutServiceBinding = Saml2BindingType.HttpRedirect,
                WantAuthnRequestsSigned = false,
                AllowUnsolicitedAuthnResponse = true, //Telstra does not return a InResponseTo attribute 
                LoadMetadata = false //No metadata endpoint provided
            };
            identityProvider.SigningKeys.AddConfiguredKey(GetTelstraSigningCertificate());

            authServicesOptions.IdentityProviders.Add(identityProvider);

            app.UseKentorAuthServicesAuthentication(authServicesOptions);
        }


      

        //Because we don't have a metadata endpoint we need to hardcode the signing certifcate
        private X509Certificate2 GetTelstraSigningCertificate()
        {
            //signing certifcate provided by telstra
            var certBase64 = "MIICBDCCAW0CBEkbTlUwDQYJKoZIhvcNAQEEBQAwSTELMAkGA1UEBhMCYXUxFzAVBgoJkiaJk/IsZAEZFgd0ZWxzdHJhMRAwDgYDVQQLEwdiaWdwb25kMQ8wDQYDVQQDEwZSQUFTU08wHhcNMDgxMTEyMjE0NDUzWhcNMTgxMTEwMjE0NDUzWjBJMQswCQYDVQQGEwJhdTEXMBUGCgmSJomT8ixkARkWB3RlbHN0cmExEDAOBgNVBAsTB2JpZ3BvbmQxDzANBgNVBAMTBlJBQVNTTzCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEA7Hv0RkNMnCD / 8fY3pobKUkcieEEDbc3sC4cfGDn36OyJNcHqEHMnvsyYiNuoB5Gwqb + FI3JknaHXshcjLctiyrYqY2Qb8oErTqYRqEHeVC7J6wpNIpLZ4Z1BZRo7FYABsw2xwKZxygkXHkQhrbwe/SSamp1854PMvI5+apVbXPUCAwEAATANBgkqhkiG9w0BAQQFAAOBgQAPqy+7BByyqWB0TVheAaIHtR7rwehZRiPv5Stc2n39L9vOcG/akDRmuNwkP9EJ45Q0FNKZY3yahm5gVNUqryH19hkh2udKuxkGaC8niBF7s4V8pbqDIRdy8e/F7wgKQ1gq5QO15L6JJY+MpAicX5bhfsTUQ0n+MeISwcKEd3wY5A==";
            var certBytes = Convert.FromBase64String(certBase64);

            var cert = new X509Certificate2(certBytes);
            return cert;
        }
    }
}
