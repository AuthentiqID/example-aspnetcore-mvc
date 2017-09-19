using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace MvcClient
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {     
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services
            services.AddMvc();

            // Add authentication services
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect("Authentiq", options => {
                // Set the authority to the Authentiq Provider
                options.Authority = "https://connect.authentiq.io";

                // Configure the Authentiq Client ID and Client Secret
                options.ClientId = Configuration["Authentiq:ClientId"];
                options.ClientSecret = Configuration["Authentiq:ClientSecret"];

                // Set response type to code id_token
                options.ResponseType = "code id_token";

                // Configure the Claims Issuer to be Authentiq
                options.ClaimsIssuer = "Authentiq";

                // Configure the scopes requested from user
                // Check the supported Identity Claims for Authentiq at http://developers.authentiq.io/#identity-claims
                options.Scope.Add("openid");
                options.Scope.Add("aq:push");

                // email will be required and signed
                options.Scope.Add("email~rs");
                options.Scope.Add("profile");

                // Set the callback path, so Authentiq will call back to http://localhost:5002/signin-authentiq 
                // check that you have added the URL, in "Redirect URIs" at the Authentiq dashboard
                options.CallbackPath = new PathString("/signin-authentiq");

                options.SignedOutCallbackPath = new PathString("/signout-callback-authentiq");
                options.RemoteSignOutPath = new PathString("/signout-authentiq");

                // The UserInfo endpoint does not really return any extra claims which were not returned in the id_token
                options.GetClaimsFromUserInfoEndpoint = false;

                options.SaveTokens = true;
            });
        }

        // Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
